using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Exporters;

public interface IProfileExporter
{
    string Export(ProfileDto profile);
}
