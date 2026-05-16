namespace Ums.Shell.Ddd.Helpers
{
    public static class IdHelper
    {
        public static Guid GetGuidFromString(string value)
        {
            Guid guid = Guid.Empty;

            var isGuidValid = Guid.TryParse(value, out guid);

            if (!isGuidValid)
            {
                throw new ArgumentNullException($"Value: {value} has invalid format.");                
            }

            return new Guid(value);
        }
    }
}
