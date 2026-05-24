using Newtonsoft.Json;

namespace Ums.Shell.Aop.Aspects.Logger
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}

