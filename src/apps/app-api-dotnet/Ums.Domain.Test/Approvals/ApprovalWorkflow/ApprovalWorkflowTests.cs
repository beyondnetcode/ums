namespace Ums.Domain.Test.Approvals.ApprovalWorkflow;

using Ums.Domain.Approvals.ApprovalWorkflow;
using Xunit;

public class ApprovalWorkflowTests
{
    private static readonly TenantId ValidTenantId = TenantId.Load(Guid.NewGuid().ToString());
    private static readonly Code ValidCode = Code.Create("WORKFLOW-001");
    private static readonly Name ValidName = Name.Create("Employee Onboarding");
    private static readonly Description ValidDescription = Description.Create("Standard onboarding workflow");
    private static readonly UserCategory ValidUserCategory = UserCategory.Internal;
    private static readonly SystemSuiteId? ValidSystemSuiteId = SystemSuiteId.Load(Guid.NewGuid().ToString());
    private static readonly ActorId ValidActor = ActorId.Create("user-001");
    #region Create

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidTenantId, result.Value.TenantId);
        Assert.Equal(ValidCode, result.Value.Code);
        Assert.Equal(ValidName, result.Value.Name);
        Assert.Equal(ValidDescription, result.Value.Description);
        Assert.Equal(ValidUserCategory, result.Value.TargetUserCategory);
        Assert.True(result.Value.RequiresApproval);
        Assert.Equal(ValidSystemSuiteId, result.Value.SystemSuiteId);
        Assert.Empty(result.Value.RequiredDocuments);
    }

    [Fact]
    public void Create_WithoutSystemSuiteId_ReturnsSuccess()
    {
        var result = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, false, null, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.SystemSuiteId);
        Assert.False(result.Value.RequiresApproval);
    }

    #endregion

    #region AddRequiredDocument

    [Fact]
    public void AddRequiredDocument_WithValidData_ReturnsSuccess()
    {
        var workflow = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor).Value;
        var documentTypeId = DocumentTypeId.Load(Guid.NewGuid().ToString());

        var result = workflow.AddRequiredDocument(documentTypeId, true, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Single(workflow.RequiredDocuments);
        Assert.True(workflow.RequiredDocuments.First().IsMandatory);
    }

    [Fact]
    public void AddRequiredDocument_WithOptionalDocument_ReturnsSuccess()
    {
        var workflow = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor).Value;
        var documentTypeId = DocumentTypeId.Load(Guid.NewGuid().ToString());

        var result = workflow.AddRequiredDocument(documentTypeId, false, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.False(workflow.RequiredDocuments.First().IsMandatory);
    }

    [Fact]
    public void AddRequiredDocument_WithDuplicateDocumentType_ReturnsFailure()
    {
        var workflow = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor).Value;
        var documentTypeId = DocumentTypeId.Load(Guid.NewGuid().ToString());
        workflow.AddRequiredDocument(documentTypeId, true, ValidActor);

        var result = workflow.AddRequiredDocument(documentTypeId, false, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.DocumentTypeAlreadyRequired, result.Error);
    }

    #endregion

    #region RemoveRequiredDocument

    [Fact]
    public void RemoveRequiredDocument_WhenDocumentExists_ReturnsSuccess()
    {
        var workflow = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor).Value;
        var documentTypeId = DocumentTypeId.Load(Guid.NewGuid().ToString());
        workflow.AddRequiredDocument(documentTypeId, true, ValidActor);
        var documentId = workflow.RequiredDocuments.First().Id;

        var result = workflow.RemoveRequiredDocument(documentId, ValidActor);

        Assert.True(result.IsSuccess);
        Assert.Empty(workflow.RequiredDocuments);
    }

    [Fact]
    public void RemoveRequiredDocument_WhenDocumentNotFound_ReturnsFailure()
    {
        var workflow = ApprovalWorkflow.Create(
            ValidTenantId, ValidCode, ValidName, ValidDescription, ValidUserCategory, true, ValidSystemSuiteId, ValidActor).Value;
        var fakeId = IdValueObject.Create();

        var result = workflow.RemoveRequiredDocument(fakeId, ValidActor);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Common.NotFound, result.Error);
    }

    #endregion
}
