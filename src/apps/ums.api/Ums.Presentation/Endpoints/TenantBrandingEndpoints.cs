namespace Ums.Presentation.Endpoints;

using Ums.Application.Identity.Tenant.FailBrandingDns;
using Ums.Application.Identity.Tenant.RemoveBranding;
using Ums.Application.Identity.Tenant.SetBranding;
using Ums.Application.Identity.Tenant.UpdateBranding;
using Ums.Application.Identity.Tenant.VerifyBrandingDns;

public static class TenantBrandingEndpoints
{
    public static IEndpointRouteBuilder MapTenantBrandingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants/{tenantId:guid}/branding")
            .WithTags("Tenant Branding");

        group.MapPost("/", async (
            Guid tenantId,
            [FromBody] SetBrandingRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new SetBrandingCommand(
                tenantId,
                request.Logo,
                request.LogoFormat,
                request.PrimaryColor,
                request.BackgroundStyle,
                request.HeadlineText,
                request.SecondaryText,
                request.PrimaryButtonLabel,
                request.FooterText,
                request.CustomDomain,
                request.MagicLinkFallbackEnabled);
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/api/tenants/{r.TenantId}/branding");
        })
        .WithName("SetBranding")
        .WithSummary("Set branding for a tenant")
        .Produces<SetBrandingResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/", async (
            Guid tenantId,
            [FromBody] UpdateBrandingRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateBrandingCommand(
                tenantId,
                request.Logo,
                request.LogoFormat,
                request.PrimaryColor,
                request.BackgroundStyle,
                request.HeadlineText,
                request.SecondaryText,
                request.PrimaryButtonLabel,
                request.FooterText,
                request.CustomDomain,
                request.MagicLinkFallbackEnabled);
            var result = await mediator.Send(command, ct);
            return result.ToNoContent();
        })
        .WithName("UpdateBranding")
        .WithSummary("Update tenant branding")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/", async (
            Guid tenantId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveBrandingCommand(tenantId), ct);
            return result.ToNoContent();
        })
        .WithName("RemoveBranding")
        .WithSummary("Remove tenant branding")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/dns/verify", async (
            Guid tenantId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new VerifyBrandingDnsCommand(tenantId), ct);
            return result.ToNoContent();
        })
        .WithName("VerifyBrandingDns")
        .WithSummary("Verify branding DNS configuration")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/dns/fail", async (
            Guid tenantId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new FailBrandingDnsCommand(tenantId), ct);
            return result.ToNoContent();
        })
        .WithName("FailBrandingDns")
        .WithSummary("Mark branding DNS verification as failed")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}

public sealed record SetBrandingRequest(
    string Logo,
    string LogoFormat,
    string PrimaryColor,
    string BackgroundStyle,
    string HeadlineText,
    string SecondaryText,
    string PrimaryButtonLabel,
    string FooterText,
    string? CustomDomain,
    bool MagicLinkFallbackEnabled);

public sealed record UpdateBrandingRequest(
    string Logo,
    string LogoFormat,
    string PrimaryColor,
    string BackgroundStyle,
    string HeadlineText,
    string SecondaryText,
    string PrimaryButtonLabel,
    string FooterText,
    string? CustomDomain,
    bool MagicLinkFallbackEnabled);
