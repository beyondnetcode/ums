namespace Ums.Presentation.IntegrationTest.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ums.Application.Common.Interfaces;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Xunit;

public class AuthorizationRepositoryTests
{
    private readonly Guid _testTenantId = Guid.NewGuid();
    private readonly Guid _testSystemSuiteId = Guid.NewGuid();

    [Fact]
    public async Task SystemSuite_CrudOperations_WorkCorrectly()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        var systemSuite = new SystemSuiteRecord
        {
            Id = _testSystemSuiteId,
            TenantId = _testTenantId,
            Code = "UMS-SYSTEM",
            Name = "UMS System Suite",
            Description = "Main system suite",
            StatusId = 0,
            CreatedBy = "test",
            CreatedAtUtc = DateTime.UtcNow,
            AuditTimeSpan = ""
        };

        await context.SystemSuites.AddAsync(systemSuite);
        await context.SaveChangesAsync();

        var retrieved = await context.SystemSuites.FindAsync(_testSystemSuiteId);
        retrieved.Should().NotBeNull();
        retrieved!.Code.Should().Be("UMS-SYSTEM");

        retrieved.Name = "Updated System Suite";
        await context.SaveChangesAsync();

        var updated = await context.SystemSuites.FindAsync(_testSystemSuiteId);
        updated!.Name.Should().Be("Updated System Suite");
    }

    [Fact]
    public async Task Role_CrudOperations_WorkCorrectly()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        var role = new RoleRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            SystemSuiteId = _testSystemSuiteId,
            Code = "ADMIN-ROLE",
            Value = "Administrator Role",
            Description = "Admin role description",
            HierarchyLevel = 1,
            PromotionOrder = 1,
            IsActive = true,
            CreatedBy = "test",
            CreatedAtUtc = DateTime.UtcNow,
            AuditTimeSpan = ""
        };

        await context.Roles.AddAsync(role);
        await context.SaveChangesAsync();

        var retrieved = await context.Roles.FindAsync(role.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Code.Should().Be("ADMIN-ROLE");

        var allRoles = await context.Roles.Where(r => r.TenantId == _testTenantId).ToListAsync();
        allRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Profile_CrudOperations_WorkCorrectly()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var profile = new ProfileRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            UserId = userId,
            RoleId = roleId,
            ScopeId = 0,
            IsActive = true,
            CreatedBy = "test",
            CreatedAtUtc = DateTime.UtcNow,
            AuditTimeSpan = ""
        };

        await context.Profiles.AddAsync(profile);
        await context.SaveChangesAsync();

        var retrieved = await context.Profiles.FindAsync(profile.Id);
        retrieved.Should().NotBeNull();
        retrieved!.UserId.Should().Be(userId);
        retrieved.IsActive.Should().BeTrue();

        var profilesByUser = await context.Profiles.Where(p => p.UserId == userId).ToListAsync();
        profilesByUser.Should().HaveCount(1);
    }

    private UmsPlatformDbContext CreateContext(DbContextOptions<UmsPlatformDbContext> options)
    {
        var tenantContext = new TestTenantContext(_testTenantId);
        return new UmsPlatformDbContext(options, tenantContext);
    }

    private class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? OrganizationId => tenantId;
        public void SetOrganizationId(Guid organizationId) { }
    }
}

public class ConfigurationRepositoryTests
{
    private readonly Guid _testTenantId = Guid.NewGuid();
    private readonly Guid _testSystemSuiteId = Guid.NewGuid();

    [Fact]
    public async Task FeatureFlag_CrudOperations_WorkCorrectly()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        var flag = new FeatureFlagRecord
        {
            Id = Guid.NewGuid(),
            SystemSuiteId = _testSystemSuiteId,
            TenantId = _testTenantId,
            FlagCode = "NEW_FEATURE",
            FlagTypeId = 0,
            FlagTargets = "{}",
            StatusId = 0,
            CreatedBy = "test",
            CreatedAtUtc = DateTime.UtcNow,
            AuditTimeSpan = ""
        };

        await context.FeatureFlags.AddAsync(flag);
        await context.SaveChangesAsync();

        var retrieved = await context.FeatureFlags.FindAsync(flag.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FlagCode.Should().Be("NEW_FEATURE");

        retrieved.StatusId = 1;
        await context.SaveChangesAsync();

        var updated = await context.FeatureFlags.FindAsync(flag.Id);
        updated!.StatusId.Should().Be(1);
    }

    [Fact]
    public async Task FeatureFlag_QueryBySystemSuite_ReturnsCorrectFlags()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        await context.FeatureFlags.AddRangeAsync(
            new FeatureFlagRecord
            {
                Id = Guid.NewGuid(),
                SystemSuiteId = _testSystemSuiteId,
                FlagCode = "FLAG-1",
                FlagTypeId = 0,
                FlagTargets = "{}",
                StatusId = 0,
                CreatedBy = "test",
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = ""
            },
            new FeatureFlagRecord
            {
                Id = Guid.NewGuid(),
                SystemSuiteId = _testSystemSuiteId,
                FlagCode = "FLAG-2",
                FlagTypeId = 0,
                FlagTargets = "{}",
                StatusId = 0,
                CreatedBy = "test",
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = ""
            },
            new FeatureFlagRecord
            {
                Id = Guid.NewGuid(),
                SystemSuiteId = Guid.NewGuid(),
                FlagCode = "OTHER-SUITE-FLAG",
                FlagTypeId = 0,
                FlagTargets = "{}",
                StatusId = 0,
                CreatedBy = "test",
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = ""
            }
        );
        await context.SaveChangesAsync();

        var suiteFlags = await context.FeatureFlags
            .Where(f => f.SystemSuiteId == _testSystemSuiteId)
            .ToListAsync();

        suiteFlags.Should().HaveCount(2);
    }

    [Fact]
    public async Task FeatureFlagCriteria_AddAndRetrieve_WorkCorrectly()
    {
        var options = new DbContextOptionsBuilder<UmsPlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = CreateContext(options);

        var flagId = Guid.NewGuid();
        var flag = new FeatureFlagRecord
        {
            Id = flagId,
            SystemSuiteId = _testSystemSuiteId,
            FlagCode = "CRITERIA-FLAG",
            FlagTypeId = 0,
            FlagTargets = "{}",
            StatusId = 0,
            CreatedBy = "test",
            CreatedAtUtc = DateTime.UtcNow,
            AuditTimeSpan = ""
        };

        await context.FeatureFlags.AddAsync(flag);

        var criteria = new FeatureFlagCriteriaRecord
        {
            Id = Guid.NewGuid(),
            FeatureFlagId = flagId,
            CriteriaType = "UserRole",
            Operator = "Equals",
            Value = "test-value",
            CreatedAtUtc = DateTime.UtcNow
        };

        await context.FeatureFlagCriteria.AddAsync(criteria);
        await context.SaveChangesAsync();

        var retrievedCriteria = await context.FeatureFlagCriteria
            .Where(c => c.FeatureFlagId == flagId)
            .ToListAsync();

        retrievedCriteria.Should().HaveCount(1);
        retrievedCriteria.First().Value.Should().Be("test-value");
    }

    private UmsPlatformDbContext CreateContext(DbContextOptions<UmsPlatformDbContext> options)
    {
        var tenantContext = new TestTenantContext(_testTenantId);
        return new UmsPlatformDbContext(options, tenantContext);
    }

    private class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? OrganizationId => tenantId;
        public void SetOrganizationId(Guid organizationId) { }
    }
}
