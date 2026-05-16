using Ums.Shell.Factory.Test.Interfaces;

namespace Ums.Shell.Factory.Test.Impl
{
    public class DoSomethingLessThan18 : IDoSomething
    {
        public bool Apply()
        {
            return true;
        }
    }
}