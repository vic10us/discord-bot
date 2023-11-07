using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using BotAdminUI;
using BotAdminUI.Components;
using BotAdminUI.Services;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("hostsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables(prefix: "BOT_");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddCommandLine(args);

var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddCascadingAuthenticationState();

services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//services.AddSingleton<DiscordSocketClient>(sc =>
//{
//    var config = new DiscordSocketConfig()
//    {
//        GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildScheduledEvents
//    };
//    var client = new DiscordSocketClient(config);
//    client.LoginAsync(TokenType.Bot, builder.Configuration["Discord:Token"]).Wait();
//    client.StartAsync().Wait();
//    return client;
//});

services.AddKeyedSingleton<DiscordRestClient>("BotClient", (s, k) =>
{
    var client = new DiscordRestClient(new DiscordRestConfig
    {
        LogLevel = LogSeverity.Debug,
    });
    client.LoginAsync(TokenType.Bot, builder.Configuration["Discord:Token"]).Wait();
    return client;
});

services.AddKeyedScoped<DiscordRestClient>("UserClient", (s, k) => {
    var httpContext = s.GetRequiredService<IHttpContextAccessor>();
    var accessToken = httpContext.HttpContext.GetTokenAsync("Discord", "access_token").GetAwaiter().GetResult();

    var client = new DiscordRestClient(new DiscordRestConfig
    {
        LogLevel = LogSeverity.Debug,
    });

    if (httpContext.HttpContext?.User.Identity?.IsAuthenticated ?? false) {
        client.LoginAsync(TokenType.Bearer, accessToken).Wait();
    }

    return client;
});

services.AddScoped<IDiscordUserService, DiscordUserService>();
services.AddHttpClient();

services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // "Discord"; // JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
})
.AddOAuth("Discord", options => {
    options.ClientId = builder.Configuration["DiscordAuth:ClientId"];
    options.ClientSecret = builder.Configuration["DiscordAuth:ClientSecret"];

    options.AuthorizationEndpoint = builder.Configuration["Discord:AuthorizationEndpoint"];
    options.TokenEndpoint = builder.Configuration["Discord:TokenEndpoint"];
    options.UserInformationEndpoint = builder.Configuration["Discord:UserInformationEndpoint"];

    options.Scope.Add("identify");
    options.Scope.Add("guilds");
    options.Scope.Add("email");
    options.Scope.Add("guilds.members.read");

    options.CallbackPath = new PathString("/signin-discord");
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.UInteger64);
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username", ClaimValueTypes.String);
    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "global_name", ClaimValueTypes.String);
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
    options.ClaimActions.MapJsonKey("urn:discord:discriminator", "discriminator", ClaimValueTypes.UInteger32);
    options.ClaimActions.MapJsonKey("urn:discord:avatar", "avatar", ClaimValueTypes.String);
    options.ClaimActions.MapJsonKey("urn:discord:avatar_url", "avatar_url", ClaimValueTypes.String);
    options.ClaimActions.MapJsonKey("urn:discord:verified", "verified", ClaimValueTypes.Boolean);

    options.SaveTokens = true;
    options.AccessDeniedPath = "/access-denied";

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonNode.Parse(json);
            
            if (doc == null) return;

            //DiscordRestClient client = new DiscordRestClient();
            //await client.LoginAsync(Discord.TokenType.Bearer, context.AccessToken);
            //var restUser = await client.GetCurrentUserAsync();
            var userId = doc["id"]?.GetValue<string?>();
            var avatarId = doc["avatar"]?.GetValue<string?>();
            
            var userAvatarUrl = DiscordUserService.GetUserAvatarUrl(userId, avatarId);  //restUser.GetAvatarUrl();

            //doc["access_token"] = context.AccessToken;
            //doc["refresh_token"] = context.RefreshToken;
            doc["avatar_url"] = userAvatarUrl;

            Console.WriteLine(doc.ToJsonString(new JsonSerializerOptions()
            {
                WriteIndented = true
            }));

            var user = JsonDocument.Parse(doc.ToJsonString()).RootElement;
            context.RunClaimActions(user);
        }
    };
});

services.AddAdminUIServices(builder.Configuration);

// Add services to the container.
services.AddRazorComponents()
    .AddInteractiveServerComponents();
    //.AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// Need to use Discord for authentication and autorization

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    //.AddInteractiveWebAssemblyRenderMode()
    //.AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
