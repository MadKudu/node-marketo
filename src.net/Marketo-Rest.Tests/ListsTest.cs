using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Marketo.Tests
{
    [TestClass, TestCategory("Lists")]
    public class ListsTest
    {
        readonly MarketoClient marketo = Helper.config.Connection();

        [TestMethod]
        public async Task Lists_Find()
        {
            // finds a list by id
            var res = await marketo.List.Find(new { id = 1 });
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(1001, (int)res.result[0].id);
            Assert.IsNotNull((string)res.result[0].name);
            Assert.IsNotNull((string)res.result[0].workspacename);

            // finds a list by name and workspaceName
            res = await marketo.List.Find(new { name = "Lions", workspace = "Default" });
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(1001, (int)res.result[0].id);
            Assert.AreEqual("Lions", (int)res.result[1].name);
            Assert.AreEqual("Default", (int)res.result[1].workspaceName);
        }

        [TestMethod]
        public async Task Lists_ById()
        {
            // finds a list by id
            var res = await marketo.List.ById(1);
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(1001, (int)res.result[0].id);
            Assert.IsNotNull((string)res.result[0].name);
            Assert.IsNotNull((string)res.result[0].workspacename);
        }

        [TestMethod]
        public async Task Lists_IsMember()
        {
            // finds a lead as member of list
            var res = await marketo.List.IsMember(1001, new int[42]);
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(42, (int)res.result[0].id);
            Assert.AreEqual("memberOf", (string)res.result[0].status);

            // does not find a lead missing from a list
            res = await marketo.List.IsMember(1001, new int[44]);
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(44, (int)res.result[0].id);
            Assert.AreEqual("notmemberOf", (string)res.result[0].status);
        }

        [TestMethod]
        public async Task Lists_RemoveLeadsFromList()
        {
            // finds a lead as member of list
            var res = await marketo.List.RemoveLeadsFromList(1001, new int[42]);
            Assert.AreEqual(1, (int)res.result.length);
            Assert.AreEqual(42, (int)res.result[0].id);
            Assert.AreEqual("removed", (string)res.result[0].status);
        }
    }
}
