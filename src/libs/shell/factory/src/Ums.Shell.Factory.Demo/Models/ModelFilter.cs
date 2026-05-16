namespace Ums.Shell.Factory.Demo.Models
{
    public class ModelLogic
    {
        public string CommercialProductFilter { get; set; }
        public string[] LinkedTactics { get; set; }
        public string[] CustomOffersCuv { get; set; }
        public string[] DemoKits { get; set; }

        public string[] CommercialProducts { get; set; }
        public string Message { get; set; }

        public ModelLogic()
        {
            CommercialProductFilter = string.Empty;
            LinkedTactics = new string[0];
            CustomOffersCuv = new string[0];
            DemoKits = new string[0];
            CommercialProducts = new string[0];
            Message = string.Empty;
        }
    }
}
