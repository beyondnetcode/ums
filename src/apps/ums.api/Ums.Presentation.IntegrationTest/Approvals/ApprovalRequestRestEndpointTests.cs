using System.Net.Http.Json;
using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Approvals;

public sealed class ApprovalRequestRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private static readonly Guid ManualWorkflowId = Guid.Parse("88888888-1111-1111-1111-111111111111");
    private static readonly Guid AutoWorkflowId = Guid.Parse("88888888-2222-2222-2222-222222222222");
    private readonly HttpClient _client;

    public ApprovalRequestRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-User-Id", "3fa85f64-5717-4562-b3fc-2c963f66afa6");
    }

    [Fact]
    public async Task Create_WithApprovalRequiredWorkflow_ShouldRemainPending()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new
        {
            WorkflowId = ManualWorkflowId,
            TargetUserId = Guid.Parse("7d6a4f3f-1f01-4f08-a5e0-111111111111"),
            TargetProfileId = (Guid?)null,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/approval-requests", command, cancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateApprovalRequestResponse>(cancellationToken);
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/approval-requests/{created!.ApprovalRequestId}", cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var payload = await getResponse.Content.ReadFromJsonAsync<ApprovalRequestDto>(cancellationToken);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Create_WithAutoApproveWorkflow_ShouldBeApprovedImmediately()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new
        {
            WorkflowId = AutoWorkflowId,
            TargetUserId = Guid.Parse("7d6a4f3f-1f01-4f08-a5e0-222222222222"),
            TargetProfileId = (Guid?)null,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/approval-requests", command, cancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CreateApprovalRequestResponse>(cancellationToken);
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/approval-requests/{created!.ApprovalRequestId}", cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var payload = await getResponse.Content.ReadFromJsonAsync<ApprovalRequestDto>(cancellationToken);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Approved");
    }
}
