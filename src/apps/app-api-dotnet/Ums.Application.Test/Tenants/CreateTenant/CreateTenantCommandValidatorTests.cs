namespace Ums.Application.Test.Tenants.CreateTenant;

using FluentValidation.TestHelper;
using Ums.Application.Tenants.CreateTenant;

public class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator;

    public CreateTenantCommandValidatorTests()
    {
        _validator = new CreateTenantCommandValidator();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_HasValidationError()
    {
        var command = new CreateTenantCommand("", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Code);
    }

    [Fact]
    public void Code_WhenWhitespace_HasValidationError()
    {
        var command = new CreateTenantCommand("   ", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Code);
    }

    [Fact]
    public void Code_WhenExceedsMaxLength_HasValidationError()
    {
        var command = new CreateTenantCommand(new string('A', 51), "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Code);
    }

    [Fact]
    public void Code_WhenValid_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Code);
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", new string('A', 151), "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Name_WhenValid_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    #endregion

    #region Type Validation

    [Fact]
    public void Type_WhenEmpty_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Type_WhenNotSupported_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INVALID_TYPE", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Type_WhenInternal_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Type_WhenSupplier_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "SUPPLIER", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }

    #endregion

    #region IdpStrategy Validation

    [Fact]
    public void IdpStrategy_WhenNotSupported_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "INVALID_STRATEGY", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.IdpStrategy);
    }

    [Fact]
    public void IdpStrategy_WhenNull_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.IdpStrategy);
    }

    [Fact]
    public void IdpStrategy_WhenEmpty_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.IdpStrategy);
    }

    [Fact]
    public void IdpStrategy_WhenAzureAd_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "AzureAd", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.IdpStrategy);
    }

    [Fact]
    public void IdpStrategy_WhenOkta_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "Okta", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.IdpStrategy);
    }

    [Fact]
    public void IdpStrategy_WhenInternalBcrypt_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "InternalBcrypt", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.IdpStrategy);
    }

    #endregion

    #region CompanyReference Validation

    [Fact]
    public void CompanyReference_WhenExceedsMaxLength_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, new string('A', 151), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CompanyReference);
    }

    [Fact]
    public void CompanyReference_WhenNull_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.CompanyReference);
    }

    [Fact]
    public void CompanyReference_WhenValid_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, "COMP-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.CompanyReference);
    }

    #endregion

    #region ParentTenantId Validation

    [Fact]
    public void ParentTenantId_WhenEmptyGuid_HasValidationError()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ParentTenantId);
    }

    [Fact]
    public void ParentTenantId_WhenNull_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.ParentTenantId);
    }

    [Fact]
    public void ParentTenantId_WhenValidGuid_PassesValidation()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", null, null, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.ParentTenantId);
    }

    #endregion

    #region Full Command Validation

    [Fact]
    public void Validate_WithAllValidFields_ReturnsValid()
    {
        var command = new CreateTenantCommand("TEST-001", "Test Tenant", "INTERNAL", "AzureAd", "COMP-123", Guid.NewGuid());

        var result = _validator.TestValidate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var command = new CreateTenantCommand("", "", "INVALID_TYPE", "INVALID_STRATEGY", new string('A', 200), Guid.Empty);

        var result = _validator.TestValidate(command);

        Assert.True(result.Errors.Count >= 4);
    }

    #endregion
}
