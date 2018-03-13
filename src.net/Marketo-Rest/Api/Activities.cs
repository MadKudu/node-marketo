using Marketo.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class Activities.
    /// </summary>
    public class Activities
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Activities"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public Activities(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Gets all types.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetAllTypes(dynamic options = null)
        {
            var path = util.createPath("activities", "types.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        /// <exception cref="ArgumentNullException">activity</exception>
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

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        /// <exception cref="ArgumentNullException">activity</exception>
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
