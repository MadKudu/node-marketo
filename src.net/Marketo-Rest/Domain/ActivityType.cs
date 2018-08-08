using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Marketo.Domain
{
    /// <summary>
    /// Class ActivityType.
    /// </summary>
    public class ActivityType
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the primary attribute.
        /// </summary>
        /// <value>The primary attribute.</value>
        public Attribute PrimaryAttribute { get; set; }
        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public Attribute[] Attributes { get; set; }

        /// <summary>
        /// Class Attribute.
        /// </summary>
        public class Attribute
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the type of the data.
            /// </summary>
            /// <value>The type of the data.</value>
            public string DataType { get; set; }
        }

        /// <summary>
        /// Froms the results.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>ActivityType[].</returns>
        public static ActivityType[] FromResults(JObject result) => result != null ? JsonConvert.DeserializeObject<ActivityType[]>(result["result"].ToString(), MarketoClient.JsonSerializerSettings) : null;
    }
}
