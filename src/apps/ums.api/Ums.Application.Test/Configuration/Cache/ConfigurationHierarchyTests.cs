namespace Ums.Application.Test.Configuration.Cache;

using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Enums;
using Ums.Infrastructure.Configuration;
using Xunit;

using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

/// <summary>
/// Verifies that the 4-level cascade (Module → Suite → Tenant → Global) in
/// InMemoryConfigurationCache and ConfigurationProvider resolves the most
/// specific available value for a given scope context (BR-1, AC-3 of FS-13).
/// </summary>
public class ConfigurationHierarchyTests
{
    private const string ConfigCode = "TIMEOUT";
    private static readonly ActorId Actor = ActorId.Create("system-test");

    private static AppConfigurationAggregate MakeConfig(
        string value,
        Guid? tenantId    = null,
        Guid? suiteId     = null,
        Guid? moduleId    = null)
    {
        var result = AppConfigurationAggregate.Create(
            tenantId  is null ? null : TenantId.Load(tenantId.Value),
            suiteId   is null ? null : SystemSuiteId.Load(suiteId.Value),
            moduleId  is null ? null : IdValueObject.Load(moduleId.Value),
            Code.Create(ConfigCode),
            ConfigurationValue.Create(value),
            Description.Create($"desc-{value}"),
            isInheritable: true,
            isEncrypted: false,
            Actor);

        return result.Value;
    }

    // ── 4-level cascade ───────────────────────────────────────────────────────

    [Fact]
    public void GetWithPrecedence_GlobalOnly_ReturnsGlobal()
    {
        var cache = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId: null);

        Assert.NotNull(result);
        Assert.Equal("global", result.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_TenantOverridesGlobal()
    {
        var tenantId = Guid.NewGuid();
        var cache = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);
        cache.PopulateTenant(tenantId, [MakeConfig("tenant", tenantId)]);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId);

        Assert.Equal("tenant", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_SuiteOverridesTenant()
    {
        var tenantId = Guid.NewGuid();
        var suiteId  = Guid.NewGuid();
        var cache    = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);
        cache.PopulateTenant(tenantId, [MakeConfig("tenant", tenantId)]);
        cache.PopulateSuite(suiteId, [MakeConfig("suite", tenantId, suiteId)]);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId);

        Assert.Equal("suite", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_ModuleOverridesSuite()
    {
        var tenantId  = Guid.NewGuid();
        var suiteId   = Guid.NewGuid();
        var moduleId  = Guid.NewGuid();
        var cache     = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);
        cache.PopulateTenant(tenantId, [MakeConfig("tenant", tenantId)]);
        cache.PopulateSuite(suiteId, [MakeConfig("suite", tenantId, suiteId)]);
        cache.PopulateModule(moduleId, [MakeConfig("module", tenantId, suiteId, moduleId)]);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId, moduleId);

        Assert.Equal("module", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_ModuleMissing_FallsBackToSuite()
    {
        var tenantId = Guid.NewGuid();
        var suiteId  = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var cache    = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);
        cache.PopulateSuite(suiteId, [MakeConfig("suite", tenantId, suiteId)]);
        // No module-level value populated

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId, moduleId);

        Assert.Equal("suite", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_SuiteAndModuleMissing_FallsBackToTenant()
    {
        var tenantId = Guid.NewGuid();
        var suiteId  = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var cache    = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);
        cache.PopulateTenant(tenantId, [MakeConfig("tenant", tenantId)]);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId, moduleId);

        Assert.Equal("tenant", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_AllScopesMissing_FallsBackToGlobal()
    {
        var cache = new InMemoryConfigurationCache();
        cache.PopulateGlobal([MakeConfig("global")]);

        var result = cache.GetWithPrecedence(ConfigCode, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("global", result!.Props.Value.GetValue());
    }

    [Fact]
    public void GetWithPrecedence_NoValueAnywhere_ReturnsNull()
    {
        var cache = new InMemoryConfigurationCache();

        var result = cache.GetWithPrecedence("MISSING_CODE", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    // ── Invalidation ─────────────────────────────────────────────────────────

    [Fact]
    public void InvalidateSuite_RemovesSuiteEntry_FallsBackToTenant()
    {
        var tenantId = Guid.NewGuid();
        var suiteId  = Guid.NewGuid();
        var cache    = new InMemoryConfigurationCache();
        cache.PopulateTenant(tenantId, [MakeConfig("tenant", tenantId)]);
        cache.PopulateSuite(suiteId, [MakeConfig("suite", tenantId, suiteId)]);

        cache.InvalidateSuite(suiteId);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId);
        Assert.Equal("tenant", result!.Props.Value.GetValue());
    }

    [Fact]
    public void InvalidateModule_RemovesModuleEntry_FallsBackToSuite()
    {
        var tenantId = Guid.NewGuid();
        var suiteId  = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var cache    = new InMemoryConfigurationCache();
        cache.PopulateSuite(suiteId, [MakeConfig("suite", tenantId, suiteId)]);
        cache.PopulateModule(moduleId, [MakeConfig("module", tenantId, suiteId, moduleId)]);

        cache.InvalidateModule(moduleId);

        var result = cache.GetWithPrecedence(ConfigCode, tenantId, suiteId, moduleId);
        Assert.Equal("suite", result!.Props.Value.GetValue());
    }
}
