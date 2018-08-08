using System;

namespace Marketo.Domain
{
    /// <summary>
    /// Class Activity.
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// Gets or sets the lead identifier.
        /// </summary>
        /// <value>The lead identifier.</value>
        public long LeadId { get; set; }
        /// <summary>
        /// Gets or sets the activity date.
        /// </summary>
        /// <value>The activity date.</value>
        public DateTime ActivityDate { get; set; }
        /// <summary>
        /// Gets or sets the activity type identifier.
        /// </summary>
        /// <value>The activity type identifier.</value>
        public long ActivityTypeId { get; set; }
        /// <summary>
        /// Gets or sets the primary attribute.
        /// </summary>
        /// <value>The primary attribute.</value>
        public string PrimaryAttribute { get; set; }
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
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public string Value { get; set; }
        }
    }
}
