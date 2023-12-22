namespace v10.Services.Jokes;

public interface IJokeResolver
{
    public IJokeServiceImpl GetJokeService(string name);
}
