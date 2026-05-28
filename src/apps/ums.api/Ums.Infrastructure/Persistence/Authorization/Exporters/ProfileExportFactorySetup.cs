using Ums.Shell.Factory.Impl;
using Ums.Application.Authorization.Profile.Exporters;

namespace Ums.Infrastructure.Persistence.Authorization.Exporters;

internal sealed class ProfileExportFactorySetup : AbstractFactorySetupSource
{
    public ProfileExportFactorySetup()
    {
        For<ProfileExportCriteria, IProfileExporter>().Create<ProfileJsonExporter>().When(x => x.Format == "JSON");
        For<ProfileExportCriteria, IProfileExporter>().Create<ProfileXmlExporter>().When(x => x.Format == "XML");
        For<ProfileExportCriteria, IProfileExporter>().Create<ProfileCsvExporter>().When(x => x.Format == "CSV");
    }
}
