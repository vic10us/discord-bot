using AutoMapper;
using bot.Dtos;

namespace bot.Profiles;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<string, ulong>().ConvertUsing(x => ulong.Parse(x));
        CreateMap<ulong, string>().ConvertUsing(x => $"{x}");
        CreateMap<Features.Database.Models.Guild, Guild>();
        CreateMap<Guild, Features.Database.Models.Guild>();
    }
}
