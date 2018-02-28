using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Marketo.Tests
{
    [TestClass, TestCategory("Leads")]
    public class LeadsTest
    {
        readonly MarketoClient marketo = Helper.config.Connection();

        [TestMethod]
        public async Task Leads_ById()
        {
            // finds a lead by id only
            var res = await marketo.Lead.ById(1);
            Assert.AreEqual(1, (int)res.result.length);

            var lead = res.result[0];
            Assert.AreEqual(1, (int)lead.id);
            Assert.IsNotNull((string)lead.email);
            Assert.IsNotNull((string)lead.lastName);

            // finds a lead but only retrieve the email field
            res = await marketo.Lead.ById(1, new { fields = new[] { "email" } });
            Assert.AreEqual(1, (int)res.result.length);

            lead = res.result[0];
            Assert.AreEqual(1, (int)lead.id);
            //Assert.AreEqual(2, lead.keys().length);
            Assert.IsNotNull((string)lead.email);
            Assert.IsNotNull((string)lead.lastName);


            // finds a lead and retrieve a subset of fields using csv
            res = await marketo.Lead.ById(1, new { fields = "email,lastName" });
            Assert.AreEqual(1, (int)res.result.length);

            lead = res.result[0];
            Assert.AreEqual(1, (int)lead.id);
            //Assert.AreEqual(3, lead.keys().length);
            Assert.IsNotNull((string)lead.email);
            Assert.IsNotNull((string)lead.lastName);
        }


        [TestMethod]
        public async Task Leads_Find()
        {
            // uses a single filter value
            var res = await marketo.Lead.Find("id", new object[] { 1 });
            Assert.AreEqual(1, (int)res.result.length);

            var lead = res.result[0];
            Assert.AreEqual(1, (int)lead.id);
            Assert.IsNotNull((string)lead.email);
            Assert.IsNotNull((string)lead.lastName);

            // uses multiple filter values
            res = await marketo.Lead.Find("id", new object[] { 1, 2 });
            Assert.AreEqual(2, (int)res.result.length);
            Assert.AreEqual(1, (int)res.result[0].id);
            Assert.AreEqual(2, (int)res.result[1].id);

            // uses multiple filter values and retrieve a subset of fields
            res = await marketo.Lead.Find("id", new object[] { 1, 2 }, new { fields = new[] { "email", "lastName" } });
            Assert.AreEqual(2, (int)res.result.length);
            Assert.AreEqual(1, (int)res.result[0].id);
            Assert.AreEqual(2, (int)res.result[1].id);

            lead = res.result[0];
            Assert.AreEqual(1, (int)lead.id);
            Assert.IsNotNull((string)lead.email);
            Assert.IsNotNull((string)lead.lastName);
        }
    }
}
