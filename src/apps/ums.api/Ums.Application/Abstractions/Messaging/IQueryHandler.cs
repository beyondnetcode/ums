namespace Ums.Application.Abstractions.Messaging;

using MediatR;
using Ums.Domain.Kernel;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
