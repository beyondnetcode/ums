namespace Ums.Shell.Aop.Aspects.Logger.Serilog
{
    public interface IExecutionContextAccessor
    {
        ExecutionContextSnapshot Current { get; }

        void Set(ExecutionContextSnapshot snapshot);
    }
}
