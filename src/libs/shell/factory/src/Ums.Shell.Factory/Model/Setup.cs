namespace Ums.Shell.Factory.Model
{
    public class Setup
    {
        public List<SetupItem> Items { get; }

        public Setup(List<SetupItem> items)
        {
            Items = items;
        }

        public Setup()
        {
            Items = new List<SetupItem>();
        }
    }
}
