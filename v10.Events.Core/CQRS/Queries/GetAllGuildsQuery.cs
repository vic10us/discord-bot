using LanguageExt.Common;
using MediatR;

namespace v10.Events.Core.CQRS.Queries;

public class GetAllGuildsQuery : IRequest<Result<List<Dtos.Guild>>> { }
