using System.Threading.Tasks;

namespace bot.Features.StrangeLaws;

public interface IStrangeLawsService
{
    Task<string> Get();
}
