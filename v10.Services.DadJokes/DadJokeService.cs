using Newtonsoft.Json;
using v10.Services.DadJokes.Models;
using v10.Services.Jokes;

namespace v10.Services.DadJokes;

public class DadJokeService : IDadJokeService, IJokeServiceImpl<IDadJoke>
{
    private readonly HttpClient _httpClient;

    public DadJokeService(
        HttpClient client
        )
    {
        _httpClient = client;
        _httpClient.BaseAddress = new Uri(config.BaseUrl);
    }

    public async Task<IDadJoke> GetJokeAsync()
    {
        var r = new HttpRequestMessage(HttpMethod.Get, "/");
        r.Headers.Add("Accept", "application/json");
        var o = await _httpClient.SendAsync(r);
        o.EnsureSuccessStatusCode();
        var json = await o.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<DadJoke>(json);
        return resp == null ? throw new Exception("Unable to get dad joke :(") : (IDadJoke)resp;
    }
}
