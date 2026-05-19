namespace Ums.Presentation.Endpoints.Identity.Tenant;

using Ums.Application.Identity.Tenant.Branding.Commands;
using Ums.Application.Identity.Tenant.Branding.DTOs;

public static class TenantBrandingEndpoints
{
    public static IEndpointRouteBuilder MapTenantBrandingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants/{tenantId:guid}/branding")
            .WithTags("Tenant Branding");

        group.MapPost("/", async (
            Guid tenantId,
            [FromBody] SetBrandingRequest request,
            IMediator mediator,
            HttpContext context,
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
            return result.ToCreated(r => $"/tenants/{r.TenantId}/branding", context);
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
            HttpContext context,
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
            return result.ToNoContent(context);
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
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveBrandingCommand(tenantId), ct);
            return result.ToNoContent(context);
        })
        .WithName("RemoveBranding")
        .WithSummary("Remove tenant branding")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/dns/verify", async (
            Guid tenantId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new VerifyBrandingDnsCommand(tenantId), ct);
            return result.ToNoContent(context);
        })
        .WithName("VerifyBrandingDns")
        .WithSummary("Verify branding DNS configuration")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/dns/fail", async (
            Guid tenantId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new FailBrandingDnsCommand(tenantId), ct);
            return result.ToNoContent(context);
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
