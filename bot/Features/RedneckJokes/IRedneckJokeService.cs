using System.Threading.Tasks;

namespace bot.Features.RedneckJokes
{
    public interface IRedneckJokeService
    {
        Task<string> GetQuote();
    }
}