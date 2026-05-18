namespace Ums.Application.Abstractions.Messaging;

using MediatR;
using Ums.Domain.Kernel;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
