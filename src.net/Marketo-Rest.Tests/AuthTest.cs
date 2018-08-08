using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Marketo.Tests
{
    [TestClass, TestCategory("Auth")]
    public class AuthTest
    {
        readonly MarketoClient marketo = Helper.config.Connection();

        [TestMethod]
        public void Auth_GetAndSetAccessToken()
        {
            Assert.IsNull(marketo.AccessToken);
            marketo.AccessToken = "test";
            Assert.AreEqual("test", marketo.AccessToken);
        }

        [TestMethod]
        public async Task Auth_Attempt()
        {
            var authCalls = 0;
            marketo.OnAccessToken = x => { authCalls++; };
            marketo.OnResponse = x => { };
            marketo.AccessToken = "none";
            var stats = await marketo.Stats.UsageLast7Days();
            Assert.IsNotNull(stats);
            Assert.AreEqual(1, authCalls);
        }
    }
}
