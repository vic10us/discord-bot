namespace v10.Services.RedneckJokes;

public interface IRedneckJokeService
{
    Task<string> GetQuote();
}
