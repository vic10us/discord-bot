using MediatR;
using v10.Services.DadJokes.Models;

namespace v10.Services.DadJokes.Queries;

public class GetDadJokeResponse : IRequest<IDadJoke> { }
