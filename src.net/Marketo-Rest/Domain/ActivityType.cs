using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Marketo.Domain
{
    public class ActivityType
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Attribute PrimaryAttribute { get; set; }
        public Attribute[] Attributes { get; set; }

        public class Attribute
        {
            public string Name { get; set; }
            public string DataType { get; set; }
        }

        public static ActivityType[] FromResults(JObject result) => result != null ? JsonConvert.DeserializeObject<ActivityType[]>(result["result"].ToString(), MarketoClient.JsonSerializerSettings) : null;
    }
}
