namespace Ums.Application.Identity.Auth.Commands;

public sealed record ForgotPasswordCommand(string TenantCode, string Email)
    : ICommand<ForgotPasswordResponse>;

public sealed record ForgotPasswordResponse(
    string Message,
    string? SimulatedTemporaryPassword  // visible in dev/simulated mode only
);
