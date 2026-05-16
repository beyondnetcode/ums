namespace Ums.Shell.Factory.Fluent.Interfaces
{
    public interface IFactoryRecordSetupWhenBuilder<out TTarget>
    {
        void When(Func<TTarget, bool> selector);
    }
}