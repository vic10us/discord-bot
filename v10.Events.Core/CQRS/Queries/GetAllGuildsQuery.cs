using MediatR;

namespace v10.Events.Core.CQRS.Queries;

public class GetAllGuildsQuery : IRequest<List<Dtos.Guild>> { }
