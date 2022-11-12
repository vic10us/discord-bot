using bot.Features.EightBall;
using MediatR;

namespace bot.Queries;

public class GetRandom8BallResponse : IRequest<EightBallResponse> { }
