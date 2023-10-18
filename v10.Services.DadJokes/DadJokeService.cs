using Newtonsoft.Json;
using v10.Services.DadJokes.Models;

namespace v10.Services.DadJokes;

public class DadJokeService : IDadJokeService
{
    private readonly HttpClient _httpClient;

    public DadJokeService(
        IHttpClientFactory clientFactory
        )
    {
        _httpClient = clientFactory.CreateClient("DadJokeService");
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
