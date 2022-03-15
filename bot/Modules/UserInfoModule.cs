using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using bot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HandlebarsDotNet;
using v10.Data.Abstractions.Models;
using v10.Data.MongoDB;

namespace bot.Modules;

public class UserInfoModule : CustomModule<SocketCommandContext>
{
  public ImageApiService ImageService { get; set; }
  public BotDataService BotDataService { get; set; }

  public static class TemplateConstants
  {
    public const string Rank = "rank.hbs";
    public const string RankImage = "rank.png";
    public const string OnePixelImage = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
  }

  [Command("rank")]
  [Alias("r")]
  public async Task GetRank(IUser user = null)
  {
    user ??= Context.User;
    var guildId = Context.Guild?.Id ?? 0;
    var userId = user.Id;
    var userData = BotDataService.GetLevelData(guildId, userId);

    var source = await GetHandlebarsImageTemplate(TemplateConstants.Rank);
    var template = Handlebars.Compile(source);
    var data = new RankData();
    var gaid = (user as SocketGuildUser)?.GuildAvatarId;
    var url = (string.IsNullOrWhiteSpace(gaid)) ? user.GetAvatarUrl(ImageFormat.Png)
        : $"https://cdn.discordapp.com/guilds/{guildId}/users/{userId}/avatars/{gaid}.png";
    var guildUserAvatarUrl = $"https://cdn.discordapp.com/guilds/{guildId}/users/{userId}/avatars/{gaid}.png";
    var userAvatarUrl = user.GetAvatarUrl(ImageFormat.Png);

    try
    {
      var (ct, profileImageData) = await DownloadImage(url);
      data.avatar.image = $"data:{ct};base64,{Convert.ToBase64String(profileImageData)}";
    }
    catch
    {
      data.avatar.image = $"data:image/png;base64,{TemplateConstants.OnePixelImage}";
    }

    data.name = user.Username;
    data.code = user.Discriminator;
    data.rank = BotDataService.GetUserRank(guildId, userId);
    (data.level, data.xp.current, data.xp.required) = BotDataService.ComputeLevelAndXp(userData.level, userData.xp);

    // data.xp.current = 500;
    data.statusColor = GetStatusColor(user.Status);

    var svg = template(data);

    var imageStream = await ImageService.ConvertSvgImage(svg);
    await SendImageEmbed(imageStream, $"{user.Username}#{user.Discriminator} Rank Card", TemplateConstants.RankImage, Color.Blue);
  }

  [Command("userinfo")]
  public async Task UserInfoAsync(IUser user = null)
  {
    user ??= Context.User;
    var message = $"{user} is {user.Id} [{user.Status}] {user.GetAvatarUrl()}";
    await ReplyAsync(message);
  }

  // Ban a user
  [Command("ban")]
  [RequireContext(ContextType.Guild)]
  // make sure the user invoking the command can ban
  [RequireUserPermission(GuildPermission.BanMembers)]
  // make sure the bot itself can ban
  [RequireBotPermission(GuildPermission.BanMembers)]
  public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
  {
    //user.Guild.AddGuildUserAsync()
    await user.Guild.AddBanAsync(user, reason: reason);
    await ReplyAsync("ok!");
  }

  [Command("addxp")]
  [RequireUserPermission(GuildPermission.ManageRoles)]
  public Task AddXp(IUser user, ulong amount)
  {
    var guildId = Context.Guild?.Id ?? 0;
    var userId = user.Id;
    _ = BotDataService.AddXp(guildId, userId, amount);
    return Task.CompletedTask;
  }

  [Command("removexp")]
  [RequireUserPermission(GuildPermission.ManageRoles)]
  public Task RemoveXp(IUser user, ulong amount)
  {
    var guildId = Context.Guild?.Id ?? 0;
    var userId = user.Id;
    _ = BotDataService.RemoveXp(guildId, userId, amount);
    return Task.CompletedTask;
  }

  private static async Task<string> GetHandlebarsImageTemplate(string name)
  {
    var assembly = Assembly.GetEntryAssembly();
    var resourceStream = assembly?.GetManifestResourceStream($"bot.Features.Images.Templates.{name}");
    if (resourceStream == null) return "";
    using var reader = new StreamReader(resourceStream, Encoding.UTF8);
    return await reader.ReadToEndAsync();
  }

  private static byte[] CopyToArray(Stream stream)
  {
    using var result = new MemoryStream();
    stream.CopyTo(result);
    return result.ToArray();
  }

  private static async Task<(string, Stream)> GetStreamAsync(string url)
  {
    using var client = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    // add Content, Headers, etc to request
    // request.Content = new StringContent(yourJsonString, System.Text.Encoding.UTF8, "application/json");
    // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync();
    var contentType = response.Content.Headers.ContentType.MediaType;
    return (contentType, stream);
  }

  private static async Task<(string, byte[])> DownloadImage(string url)
  {
    var (ct, str) = await GetStreamAsync(url);
    //using var client = new HttpClient();
    //var image = await client.GetStreamAsync(new Uri(url));
    return (ct, CopyToArray(str));
  }

  //private static async Task<byte[]> DownloadImage(string url)
  //{
  //    using var client = new HttpClient();
  //    var image = await client.GetStreamAsync(new Uri(url));
  //    return CopyToArray(image);
  //}

  private static string GetStatusColor(UserStatus status)
  {
    return status switch
    {
      UserStatus.Idle => "#FAA51B",
      UserStatus.Invisible => "#747F8D",
      UserStatus.Online => "#44B37F",
      UserStatus.AFK => "#FAA51B",
      UserStatus.DoNotDisturb => "#F04848",
      UserStatus.Offline => "#747F8D",
      _ => "#747F8D"
    };
  }
}
