using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using v10.Events.Core.Commands;
using v10.Events.Core.CQRS.Queries;
using v10.Events.Core.Dtos;

namespace bot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuildsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<GuildsController> _logger;

    public GuildsController(ILogger<GuildsController> logger, IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGuilds()
    {
        var query = new GetAllGuildsQuery();
        _logger.LogInformation("Retrieving list of Guilds");
        var result = await _mediator.Send(query);

        return result.Match<IActionResult>(Ok, error => NotFound());
    }

    [HttpGet("{guildId}")]
    public async Task<IActionResult> GetGuild(ulong guildId)
    {
        var query = new GetGuildByIdQuery(guildId);
        _logger.LogInformation("Retrieving Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(query);

        return result.Match<IActionResult>(
                    guild => guild != null ? Ok(guild) : NotFound(),
                    error => NotFound()
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuild([FromBody] CreateGuildRequest request)
    {
        var command = _mapper.Map<CreateGuildCommand>(request);
        var result = await _mediator.Send(command);

        var response = result.Match<IActionResult>(
            guild => CreatedAtAction("GetGuild", new { guildId = guild.GuildId }, guild),
            error => BadRequest(error.Message));

        return response;
    }

    [HttpPut("{guildId}")]
    public async Task<IActionResult> UpdateGuild(
      [FromRoute] ulong guildId, [FromBody] UpdateGuildCommand command)
    {
        if (guildId != StringToNullableUInt64(command.GuildId)) throw new ArgumentException("Id mismatch", nameof(guildId));
        _logger.LogInformation("Replacing Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(command);

        var response = result.Match<IActionResult>(
                guild => AcceptedAtAction("GetGuild", "Guilds", new { guildId = guild }),
                error => NotFound()
            );

        return response;
    }

    static ulong? StringToNullableUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? (ulong?)val : null;

    [HttpDelete("{guildId}")]
    public async Task<IActionResult> DeleteGuild(ulong guildId)
    {
        _logger.LogInformation("Deleting Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(new DeleteGuildCommand(guildId));
        return result.Match<IActionResult>(
                    guild => Accepted(),
                    error => NotFound());
    }
}
