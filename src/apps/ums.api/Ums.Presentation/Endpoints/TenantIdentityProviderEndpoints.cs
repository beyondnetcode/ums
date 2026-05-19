namespace Ums.Presentation.Endpoints;

using Ums.Application.Identity.Tenant.ActivateIdentityProvider;
using Ums.Application.Identity.Tenant.DeactivateIdentityProvider;
using Ums.Application.Identity.Tenant.RegisterIdentityProvider;
using Ums.Application.Identity.Tenant.RemoveIdentityProvider;

public static class TenantIdentityProviderEndpoints
{
    public static IEndpointRouteBuilder MapTenantIdentityProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants/{tenantId:guid}/identity-providers")
            .WithTags("Tenant Identity Providers");

        group.MapPost("/", async (
            Guid tenantId,
            [FromBody] RegisterIdentityProviderRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterIdentityProviderCommand(tenantId, request.Code, request.Name, request.Description, request.Strategy);
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/api/tenants/{r.TenantId}/identity-providers/{r.IdentityProviderId}");
        })
        .WithName("RegisterIdentityProvider")
        .WithSummary("Register a new identity provider")
        .Produces<RegisterIdentityProviderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{idpId:guid}/activate", async (
            Guid tenantId,
            Guid idpId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateIdentityProviderCommand(tenantId, idpId), ct);
            return result.ToNoContent();
        })
        .WithName("ActivateIdentityProvider")
        .WithSummary("Activate an identity provider")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{idpId:guid}/deactivate", async (
            Guid tenantId,
            Guid idpId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateIdentityProviderCommand(tenantId, idpId), ct);
            return result.ToNoContent();
        })
        .WithName("DeactivateIdentityProvider")
        .WithSummary("Deactivate an identity provider")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{idpId:guid}", async (
            Guid tenantId,
            Guid idpId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveIdentityProviderCommand(tenantId, idpId), ct);
            return result.ToNoContent();
        })
        .WithName("RemoveIdentityProvider")
        .WithSummary("Remove an identity provider")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}

public sealed record RegisterIdentityProviderRequest(string Code, string Name, string Description, string Strategy);
