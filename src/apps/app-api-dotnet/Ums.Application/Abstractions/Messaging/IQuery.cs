namespace Ums.Application.Abstractions.Messaging;

using MediatR;
using Ums.Domain.Kernel;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
