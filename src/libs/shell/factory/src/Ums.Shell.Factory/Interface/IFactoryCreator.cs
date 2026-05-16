namespace Ums.Shell.Factory.Interfaces
{
    public interface IFactoryCreator
    {
        T Create<T>(Type type) where T:class;
    }
}