using System;

namespace Marketo.Domain
{
    public class Activity
    {
        public long LeadId { get; set; }
        public DateTime ActivityDate { get; set; }
        public long ActivityTypeId { get; set; }
        public string PrimaryAttribute { get; set; }
        public Attribute[] Attributes { get; set; }

        public class Attribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
