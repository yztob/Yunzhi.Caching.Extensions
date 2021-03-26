using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yunzhi.Common;
using Yunzhi.Caching;
using Yunzhi.Logging;
using Yunzhi.Caching.Extensions.LiteDb;
using UnitTest.Models;

/*
 * 提供程序单元测试
 *       
 * @Alphaair
 * 20191028 create.
**/
namespace UnitTest
{
    /// <summary>
    /// 提供程序单元测试
    /// </summary>
    [TestClass]
    public class ProviderTest
    {
        /// <summary>
        /// 全读写测试
        /// </summary>
        [TestMethod]
        public void FullTest()
        {
            var session = CachePool.Instance["default"];
            Assert.IsNotNull(session);

            var text = "This is empty text.";
            var num = 14526;
            var dec = 142.93323m;
            var now = DateTime.Now;
            var flag = false;
            var obj = new Models.User()
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "alphaair",
                Password = "test pad"
            };

            session.Set<User>("user", obj, null, true);
            var objVal = session.Get<User>("user");
            Assert.AreEqual(obj.Id, objVal.Id);

            //Lite
            var cache = CachePool.Instance["lite_cache"];
            var nval = cache.Get<int>("decimal");
            Assert.AreEqual(nval, 0);

            cache.Set<string>("text", text);
            cache.Set<int>("number", num);
            cache.Set<decimal>("decimal", dec, 5);
            cache.Set<bool>("flag", flag);
            cache.Set<User>("user", obj);

            var val = cache.Get<string>("text");
            Assert.AreEqual(val, text);

            var user = cache.Get<User>("user");
            Assert.AreEqual(user.Id, obj.Id);

            //System.Threading.Thread.Sleep(6000);
            cache.Provider.Clean();

            //MongoDb
            cache = CachePool.Instance["mongo_cache"];
            dec = cache.Get<decimal>("decimal");
            Assert.AreEqual(dec, 0);

            cache.Set<string>("text", text);
            cache.Set<int>("number", num);
            cache.Set<decimal>("decimal", dec, 5);
            cache.Set<bool>("flag", flag);
            cache.Set<User>("user", obj);

            val = cache.Get<string>("text");
            Assert.AreEqual(val, text);

            user = cache.Get<User>("user");
            Assert.AreEqual(user.Id, obj.Id);

            System.Threading.Thread.Sleep(6000);
            cache.Provider.Clean();

        }
    }
}
