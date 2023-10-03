using AutoMapper;
using bot.Commands;
using bot.Dtos;
using bot.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace bot.Controllers;

[Route("webhooks")]
public class WebHookController : ControllerBase
{
    private readonly ILogger _logger;

    public WebHookController(ILogger<WebHookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("zendesk")]
    public async Task HandleZendeskWebhook()
    {
        var reader = new StreamReader(HttpContext.Request.Body);
        var rawMessage = await reader.ReadToEndAsync();

        HttpContext.Request.Headers.TryGetValue("x-zendesk-webhook-id", out var webhookId);
        HttpContext.Request.Headers.TryGetValue("x-zendesk-webhook-signature", out var signature);
        HttpContext.Request.Headers.TryGetValue("x-zendesk-webhook-signature-timestamp", out var timestamp);

        _logger.LogInformation("Received a webhook call from Zendesk");
        _logger.LogInformation("X-Zendesk-Webhook-Id: {webhookId}", webhookId);
        _logger.LogInformation("X-Zendesk-Webhook-Signature: {signature}", signature);
        _logger.LogInformation("X-Zendesk-Webhook-Signature-Timestamp: {timestamp}", timestamp);
        _logger.LogInformation(rawMessage);
    }
}

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

    ulong? StringToNullableUInt64(string value)
        => ulong.TryParse(value, out ulong val) ? (ulong?)val : null;

    [HttpDelete("{guildId}")]
    public async Task<IActionResult> DeleteGuild(ulong guildId)
    {
        _logger.LogInformation("Deleting Guild with guildId: {guildId}", guildId);
        var result = await _mediator.Send(new DeleteGuildCommand(guildId));
        return result ? Accepted() : NotFound();
    }
}
