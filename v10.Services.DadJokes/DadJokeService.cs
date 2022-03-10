using Newtonsoft.Json;
using v10.Services.DadJokes.Models;
using v10.Services.Jokes;

namespace v10.Services.DadJokes;

public class DadJokeService : IDadJokeService, IJokeServiceImpl<IDadJoke>
{
    private readonly HttpClient _httpClient;
    private readonly IDadJokeServiceConfiguration _config;

    public DadJokeService(HttpClient client, IDadJokeServiceConfiguration config)
    {
        _httpClient = client;
        _httpClient.BaseAddress = new Uri(config.BaseUrl);
        _config = config;
    }

    public async Task<IDadJoke> GetJokeAsync()
    {
        var r = new HttpRequestMessage(HttpMethod.Get, "/");
        r.Headers.Add("Accept", "application/json");
        var o = await _httpClient.SendAsync(r);
        o.EnsureSuccessStatusCode();
        var json = await o.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<DadJoke>(json);
        // var o = await _httpClient.GetFromJsonAsync<DadJoke>("/");
        if (resp == null) throw new Exception("Unable to get dad joke :(");
        return resp;
    }
}
