namespace Ums.Presentation.Endpoints.Authorization.SystemSuite;

using Ums.Application.Common;
using Ums.Application.Authorization.SystemSuite.Commands;
using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Application.Authorization.SystemSuite.Queries;

public static class SystemSuiteEndpoints
{
    public static IEndpointRouteBuilder MapSystemSuiteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/system-suites")
            .WithTags("SystemSuites");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllSystemSuitesQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllSystemSuites")
        .WithSummary("Get system suites using server-side pagination")
        .Produces<PagedResult<SystemSuiteDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateSystemSuiteCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/system-suites/{r.SystemSuiteId}", context);
        })
        .WithName("CreateSystemSuite")
        .WithSummary("Create a new system suite")
        .Produces<CreateSystemSuiteResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}", async (Guid systemSuiteId, UpdateSystemSuiteCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId }, ct);
            return result.ToNoContent(context);
        })
        .WithName("UpdateSystemSuite")
        .WithSummary("Update a system suite")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{systemSuiteId:guid}/status", async (Guid systemSuiteId, string status, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SetSystemSuiteStatusCommand(systemSuiteId, status), ct);
            return result.ToNoContent(context);
        })
        .WithName("SetSystemSuiteStatus")
        .WithSummary("Set system suite status")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // ── Module lifecycle ─────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/modules", async (Guid systemSuiteId, AddModuleCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId }, ct);
            return result.ToNoContent(context);
        }).WithName("AddModule")
          .WithSummary("Add a module to the system suite")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}/modules/{moduleId:guid}", async (Guid systemSuiteId, Guid moduleId, UpdateModuleCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateModule")
          .WithSummary("Update module name, description, or sort order")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{systemSuiteId:guid}/modules/{moduleId:guid}", async (Guid systemSuiteId, Guid moduleId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveModuleCommand(systemSuiteId, moduleId), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveModule")
          .WithSummary("Remove an inactive module from the system suite")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{systemSuiteId:guid}/modules/{moduleId:guid}/activate", async (Guid systemSuiteId, Guid moduleId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateModuleCommand(systemSuiteId, moduleId), ct);
            return result.ToNoContent(context);
        }).WithName("ActivateModule")
          .WithSummary("Activate a deactivated module")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{systemSuiteId:guid}/modules/{moduleId:guid}/deactivate", async (Guid systemSuiteId, Guid moduleId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateModuleCommand(systemSuiteId, moduleId), ct);
            return result.ToNoContent(context);
        }).WithName("DeactivateModule")
          .WithSummary("Deactivate an active module")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        // ── Menu lifecycle ────────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus", async (Guid systemSuiteId, Guid moduleId, AddMenuCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId }, ct);
            return result.ToNoContent(context);
        }).WithName("AddMenu")
          .WithSummary("Add a menu to a module")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, UpdateMenuCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId, MenuId = menuId }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateMenu")
          .WithSummary("Update menu label, description, or sort order")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveMenuCommand(systemSuiteId, moduleId, menuId), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveMenu")
          .WithSummary("Remove a menu from a module")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        // ── SubMenu lifecycle ─────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus", async (Guid systemSuiteId, Guid moduleId, Guid menuId, AddSubMenuCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId, MenuId = menuId }, ct);
            return result.ToNoContent(context);
        }).WithName("AddSubMenu")
          .WithSummary("Add a submenu to a menu")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus/{subMenuId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, Guid subMenuId, UpdateSubMenuCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId, MenuId = menuId, SubMenuId = subMenuId }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateSubMenu")
          .WithSummary("Update submenu label, description, or sort order")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus/{subMenuId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, Guid subMenuId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveSubMenuCommand(systemSuiteId, moduleId, menuId, subMenuId), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveSubMenu")
          .WithSummary("Remove a submenu from a menu")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Option lifecycle ──────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus/{subMenuId:guid}/options", async (Guid systemSuiteId, Guid moduleId, Guid menuId, Guid subMenuId, AddOptionCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId, MenuId = menuId, SubMenuId = subMenuId }, ct);
            return result.ToNoContent(context);
        }).WithName("AddOption")
          .WithSummary("Add an option to a submenu")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus/{subMenuId:guid}/options/{optionId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, Guid subMenuId, Guid optionId, UpdateOptionCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, ModuleId = moduleId, MenuId = menuId, SubMenuId = subMenuId, OptionId = optionId }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateOption")
          .WithSummary("Update option label, description, action code, or sort order")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{systemSuiteId:guid}/modules/{moduleId:guid}/menus/{menuId:guid}/submenus/{subMenuId:guid}/options/{optionId:guid}", async (Guid systemSuiteId, Guid moduleId, Guid menuId, Guid subMenuId, Guid optionId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveOptionCommand(systemSuiteId, moduleId, menuId, subMenuId, optionId), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveOption")
          .WithSummary("Remove an option from a submenu")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        // ── App settings ─────────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/app-settings", async (Guid systemSuiteId, AddAppSettingCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId }, ct);
            return result.ToNoContent(context);
        }).WithName("AddAppSetting")
          .WithSummary("Add a configuration key-value pair to the system suite")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}/app-settings/{key}", async (Guid systemSuiteId, string key, UpdateAppSettingCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId, Key = key }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateAppSetting")
          .WithSummary("Update the value of an existing app setting")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{systemSuiteId:guid}/app-settings/{key}", async (Guid systemSuiteId, string key, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveAppSettingCommand(systemSuiteId, key), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveAppSetting")
          .WithSummary("Remove an app setting from the system suite")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Actions ──────────────────────────────────────────────────────────

        group.MapPost("/{systemSuiteId:guid}/actions", async (Guid systemSuiteId, RegisterActionCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId }, ct);
            return result.ToNoContent(context);
        }).WithName("RegisterAction")
          .WithSummary("Register a new action code that can be used in permission templates")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{systemSuiteId:guid}/actions/{code}", async (Guid systemSuiteId, string code, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveActionCommand(systemSuiteId, code), ct);
            return result.ToNoContent(context);
        }).WithName("RemoveAction")
          .WithSummary("Remove an unused action from the system suite")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
