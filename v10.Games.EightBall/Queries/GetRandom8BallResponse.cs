using MediatR;
using v10.Games.EightBall.Models;

namespace v10.Games.EightBall.Queries;

public class GetRandom8BallResponse : IRequest<EightBallResponse> { }
