namespace Ums.Shell.Factory.Demo.Models
{
    public class Strategy
    {
        public ePersonalizationType PersonalizationType { get; set; }
        public required string Name { get; set; }
        public ModelLogic? ModelLogic { get; set; }
    }
}
