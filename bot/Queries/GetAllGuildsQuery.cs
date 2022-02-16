using MediatR;
using System.Collections.Generic;

namespace bot.Queries;

public class GetAllGuildsQuery : IRequest<List<Dtos.Guild>> { }
