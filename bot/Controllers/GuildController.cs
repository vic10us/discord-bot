using bot.Features.Database;
using bot.Features.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuildController : ControllerBase
{
    public readonly BotDataService _botDataService;

    public GuildController(BotDataService botDataService)
    {
        _botDataService = botDataService;
    }

    [HttpGet()]
    public async Task<IEnumerable<GuildData>> ListAsync()
    {
        return await _botDataService.GetGuildsAsync();
    }

    [HttpGet("{id}")]
    public GuildData Get(ulong id)
    {
        return _botDataService.GetGuild(id);
    }

    //[HttpPost("{id}")]
    //public async Task<GuildData> Create(GuildData guildData)
    //{
    //    throw new NotImplementedException();
    //}

    [HttpPut("{id}")]
    public void Update([FromRoute] ulong id, [FromBody] GuildData guildData)
    {
        if ($"{id}" != guildData.guildId) throw new ArgumentException("Id mismatch", nameof(id));
        _botDataService.UpdateGuild(id, guildData);
    }
}
