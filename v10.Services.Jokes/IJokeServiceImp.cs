namespace v10.Services.Jokes;

public interface IJokeServiceImpl { }

public interface IJokeServiceImpl<T> : IJokeServiceImpl
{
    public Task<T> GetJokeAsync();
}
