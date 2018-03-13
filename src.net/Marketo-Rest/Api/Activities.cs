using Marketo.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Activities
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Activities(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> GetAllTypes(dynamic options = null)
        {
            var path = util.createPath("activities", "types.json");
            return _connection.get(path, options);
        }

        public Task<dynamic> AddRange(dynamic[] activity, dynamic options = null)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            var path = util.createPath("activities", "external.json");
            var input = activity.Select(x => new
            {
                leadId = dyn.getProp<long>(x, "leadId"),
                activityDate = dyn.hasProp(x, "activityDate") ? dyn.getProp<DateTime>(x, "activityDate").ToString("o") : null,
                activityTypeId = dyn.getProp<long>(x, "activityTypeId"),
                primaryAttributeValue = dyn.getProp<string>(x, "primaryAttributeValue"),
                attributes = dyn.getProp<object>(x, "attributes"),
            }).ToArray();
            var data = new
            {
                input,
            };
            return _connection.postJson(path, data, options);
        }

        public Task<dynamic> AddRange(Activity[] activity, dynamic options = null)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            var path = util.createPath("activities", "external.json");
            var input = activity.Select(x => new
            {
                leadId = x.LeadId,
                activityDate = x.ActivityDate.ToString("o"),
                activityTypeId = x.ActivityTypeId,
                primaryAttributeValue = x.PrimaryAttribute,
                attributes = x.Attributes.Select(y => new
                {
                    name = y.Name,
                    value = y.Value,
                }).ToArray(),
            }).ToArray();
            var data = new
            {
                input,
            };
            return _connection.postJson(path, data, options);
        }
    }
}
