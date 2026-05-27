using System.Threading;
using System.Threading.Tasks;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Application.Common;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed class UpdateDomainResourceCommandHandler(ISystemSuiteRepository repository, IUserContext userContext)
    : ICommandHandler<UpdateDomainResourceCommand>
{
    public async Task<Result> Handle(UpdateDomainResourceCommand request, CancellationToken cancellationToken)
    {
        var suite = await repository.GetByIdAsync(request.SystemSuiteId, cancellationToken);
        if (suite is null)
        {
            return Result.Failure(DomainErrors.Common.NotFound);
        }

        var result = suite.UpdateDomainResource(
            IdValueObject.Load(request.DomainResourceId),
            Name.Create(request.Name),
            Description.Create(request.Description),
            ActorId.Create(userContext.UserId ?? string.Empty));

        if (result.IsFailure)
        {
            return result;
        }

        await repository.UpdateAsync(suite, cancellationToken);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
