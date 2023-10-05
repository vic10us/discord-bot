using v10.Games.EightBall.Models;

namespace v10.Games.EightBall;

public interface IEightBallService
{
    EightBallResponse GetRandomResponse();
}
