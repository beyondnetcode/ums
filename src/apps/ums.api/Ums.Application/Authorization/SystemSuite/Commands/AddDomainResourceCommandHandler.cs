using System.Threading;
using System.Threading.Tasks;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.SystemSuite.DomainResource;
using Ums.Domain.Authorization.SystemSuite.Module;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Application.Common;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed class AddDomainResourceCommandHandler(ISystemSuiteRepository repository, IUserContext userContext)
    : ICommandHandler<AddDomainResourceCommand>
{
    public async Task<Result> Handle(AddDomainResourceCommand request, CancellationToken cancellationToken)
    {
        var suite = await repository.GetByIdAsync(request.SystemSuiteId, cancellationToken);
        if (suite is null)
        {
            return Result.Failure(DomainErrors.Common.NotFound);
        }

        var type = DomainEnumerationParser.FromName<DomainResourceType>(request.Type);
        if (type is null) return Result.Failure($"Invalid DomainResourceType: {request.Type}");

        var moduleId = request.ModuleId.HasValue ? ModuleId.Load(request.ModuleId.Value) : null;
        var parentId = request.ParentResourceId.HasValue ? IdValueObject.Load(request.ParentResourceId.Value) : null;

        var result = suite.AddDomainResource(
            moduleId,
            parentId,
            type,
            Code.Create(request.Code),
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
