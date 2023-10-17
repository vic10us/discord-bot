﻿using bot.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using v10.Services.RedneckJokes;

namespace bot.Handlers;

public class GetRedneckJokeResponseHandler : IRequestHandler<GetRedneckJokeResponse, string>
{
    private readonly IRedneckJokeService service;

    public GetRedneckJokeResponseHandler(IRedneckJokeService service)
    {
        this.service = service;
    }

    public async Task<string> Handle(GetRedneckJokeResponse request, CancellationToken cancellationToken)
    {
        return await service.GetQuote();
    }
}
