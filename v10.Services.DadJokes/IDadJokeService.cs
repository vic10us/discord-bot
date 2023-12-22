using v10.Services.DadJokes.Models;

namespace v10.Services.DadJokes;

public interface IDadJokeService {
    public Task<IDadJoke> GetJokeAsync();
}
