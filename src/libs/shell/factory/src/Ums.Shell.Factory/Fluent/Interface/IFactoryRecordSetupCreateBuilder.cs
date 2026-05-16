namespace Ums.Shell.Factory.Fluent.Interfaces
{
    public interface IFactoryRecordSetupCreateBuilder<out TTarget, in TService>
    {
        IFactoryRecordSetupWhenBuilder<TTarget> Create<TImplementation>() where TImplementation : TService;
    }
}