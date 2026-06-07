using MassTransit;
using Microsoft.Extensions.Logging;
using Ums.Domain.Events;
using BeyondNetCode.Shell.Ddd.Interfaces;

namespace Ums.Infrastructure.Hosting.EventHandlers;

/// <summary>
/// HARDENING-03: Revokes tokens immediately when a user is deleted.
/// The revocation expiry is set to 24 hours — well beyond any reasonable JWT TTL.
/// Once the revocation entry expires, the user cannot have a valid token anyway
/// (they are deleted, so no IdP will issue them a new one).
/// </summary>
public sealed class UserDeletedTokenRevocationHandler(
    ITokenRevocationStore revocationStore,
    ILogger<UserDeletedTokenRevocationHandler> logger)
    : IConsumer<UserDeletedEvent>
{
    private static readonly TimeSpan RevocationWindow = TimeSpan.FromHours(24);

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var userId    = context.Message.UserId.ToString();
        var revokeUntil = DateTime.UtcNow.Add(RevocationWindow);

        await revocationStore.RevokeAsync(userId, revokeUntil, context.CancellationToken);

        logger.LogInformation(
            "HARDENING-03: Revoked tokens for deleted user {UserId} until {RevokeUntil:O}.",
            userId, revokeUntil);
    }
}

/// <summary>
/// HARDENING-03: Revokes tokens immediately when a user is blocked.
/// Unlike deletion, blocking may be temporary, so the revocation window is shorter (4 h).
/// If the user is later restored, their next successful login will issue a fresh token
/// that is not in the revocation list.
/// </summary>
public sealed class UserBlockedTokenRevocationHandler(
    ITokenRevocationStore revocationStore,
    ILogger<UserBlockedTokenRevocationHandler> logger)
    : IConsumer<UserBlockedEvent>
{
    private static readonly TimeSpan RevocationWindow = TimeSpan.FromHours(4);

    public async Task Consume(ConsumeContext<UserBlockedEvent> context)
    {
        var userId    = context.Message.UserId.ToString();
        var revokeUntil = DateTime.UtcNow.Add(RevocationWindow);

        await revocationStore.RevokeAsync(userId, revokeUntil, context.CancellationToken);

        logger.LogInformation(
            "HARDENING-03: Revoked tokens for blocked user {UserId} until {RevokeUntil:O}. Reason: {Reason}",
            userId, revokeUntil, context.Message.Reason);
    }
}
