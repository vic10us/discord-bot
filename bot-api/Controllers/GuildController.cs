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
        return Ok(result);
    }

    [HttpGet("{guildId}")]
    public async Task<IActionResult> GetGuild(ulong guildId)
    {
        var query = new GetGuildByIdQuery(guildId);
        _logger.LogInformation("Retrieving Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuild([FromBody] CreateGuildRequest request)
    {
        var command = _mapper.Map<CreateGuildCommand>(request);
        var result = await _mediator.Send(command);
        return CreatedAtAction("GetGuild", new { guildId = result.GuildId }, result);
    }

    [HttpPut("{guildId}")]
    public async Task<IActionResult> UpdateGuild(
      [FromRoute] ulong guildId, [FromBody] UpdateGuildCommand command)
    {
        if (guildId != StringToNullableUInt64(command.GuildId)) throw new ArgumentException("Id mismatch", nameof(guildId));
        _logger.LogInformation("Replacing Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(command);
        return AcceptedAtAction("GetGuild", "Guilds", new { guildId = result });
    }

    static ulong? StringToNullableUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? (ulong?)val : null;

    [HttpDelete("{guildId}")]
    public async Task<IActionResult> DeleteGuild(ulong guildId)
    {
        _logger.LogInformation("Deleting Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(new DeleteGuildCommand(guildId));
        return result ? Accepted() : NotFound();
    }
}
