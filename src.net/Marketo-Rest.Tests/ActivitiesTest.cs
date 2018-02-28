using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Marketo.Tests
{
    [TestClass, TestCategory("Activities")]
    public class ActivitiesTest
    {
        readonly MarketoClient marketo = Helper.config.Connection();

        [TestMethod]
        public async Task Activities_ListActivityTypes()
        {
            // lists activity types
            var resp = await marketo.Activities.GetActivityTypes();
            var activity = resp.result[0];
            Assert.AreEqual(1, (int)activity.id);
            Assert.AreEqual("Visit Webpage", (string)activity.name);
            Assert.IsNotNull((string)activity.primaryAttribute);
        }
    }
}
