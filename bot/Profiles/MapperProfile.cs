﻿using AutoMapper;
using bot.Commands;
using bot.Dtos;

namespace bot.Profiles;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<string, ulong>().ConvertUsing(x => ulong.Parse(x));
        CreateMap<ulong, string>().ConvertUsing(x => $"{x}");
        CreateMap<v10.Data.Abstractions.Models.Guild, Guild>();
        CreateMap<Guild, v10.Data.Abstractions.Models.Guild>();
        CreateMap<CreateGuildRequest, CreateGuildCommand>();
        CreateMap<UpdateGuildRequest, UpdateGuildCommand>();
    }
}
