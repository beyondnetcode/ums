namespace Ums.Application.Test.Authorization.Template;

using Ums.Application.Authorization.Template.Queries;
using Ums.Application.Authorization.Template.DTOs;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TemplateQueryHandlerTests
{
    private readonly Mock<IPermissionTemplateRepository> _repo = new();

    private static PermissionTemplate MakeTemplate(TemplateStatus status)
    {
        var template = PermissionTemplate.Create(
            TenantId.Load(Guid.NewGuid()),
            RoleId.Load(Guid.NewGuid()),
            SystemSuiteId.Load(Guid.NewGuid()),
            ActorId.Create("user-001")).Value;

        if (status == TemplateStatus.Published)
        {
            template.Publish(ActorId.Create("user-001"));
        }
        else if (status == TemplateStatus.Deprecated)
        {
            template.Publish(ActorId.Create("user-001"));
            template.Deprecate(ActorId.Create("user-001"));
        }

        return template;
    }

    // =========================================================================
    #region GetPermissionTemplateByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var template = MakeTemplate(TemplateStatus.Draft);
        var templateId = template.Props.Id.GetValue();
        _repo.Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(template);

        var query = new GetPermissionTemplateByIdQuery(templateId);
        var handler = new GetPermissionTemplateByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(templateId, result.Value.TemplateId);
        Assert.Equal("Draft", result.Value.Status);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PermissionTemplate?)null);

        var query = new GetPermissionTemplateByIdQuery(Guid.NewGuid());
        var handler = new GetPermissionTemplateByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("template not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllPermissionTemplatesQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithoutTenantFilter_ReturnsAllItems()
    {
        var t1 = MakeTemplate(TemplateStatus.Draft);
        var t2 = MakeTemplate(TemplateStatus.Published);
        var list = new List<PermissionTemplate> { t1, t2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPermissionTemplatesQuery(
            TenantId: null,
            Criteria: "version",
            Status: "all",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPermissionTemplatesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalItems);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithTenantFilter_ReturnsTenantItems()
    {
        var tenantId = Guid.NewGuid();
        var t1 = MakeTemplate(TemplateStatus.Draft);
        var list = new List<PermissionTemplate> { t1 };

        _repo.Setup(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPermissionTemplatesQuery(
            TenantId: tenantId,
            Criteria: "version",
            Status: "all",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPermissionTemplatesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_FiltersStatus()
    {
        var t1 = MakeTemplate(TemplateStatus.Draft);
        var t2 = MakeTemplate(TemplateStatus.Published);
        var list = new List<PermissionTemplate> { t1, t2 };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllPermissionTemplatesQuery(
            TenantId: null,
            Criteria: "version",
            Status: "Published",
            Search: null,
            SortBy: null,
            SortOrder: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllPermissionTemplatesQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        Assert.Equal("Published", result.Value.Items[0].Status);
    }

    #endregion
}
