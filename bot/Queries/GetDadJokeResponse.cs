using bot.Features.DadJokes;
using MediatR;

namespace bot.Queries;

public class GetDadJokeResponse : IRequest<DadJoke> { }
