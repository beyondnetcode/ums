using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Queries;

public sealed record GetProfileByIdQuery(Guid ProfileId) : IQuery<ProfileDto>;
