using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Yunzhi.Common;
using Yunzhi.NoSql;
using Yunzhi.Configuration;
using Yunzhi.Caching.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

/*
 * 基于Mongodb的缓存提供程序
 *       
 * @Alphaair
 * 20200211 create.
 * 20210522 优化过期清理逻辑。
**/

namespace Yunzhi.Caching.Extensions.MongoDb
{
    /// <summary>
    /// MongoDB缓存提供程序
    /// </summary>
    /// <remarks>
    /// 可配置参数：
    /// mongodb 连接字符串或连接字符串名称
    /// cleanInterval 清理间隔,单位：分钟，默认10分钟，不低于3分钟
    /// </remarks>
    public class MongoDbProvider : Yunzhi.Caching.ICacheProvider
    {
        #region 私有成员
        private MongoDbAccess _access;
        private long _lastClean = 0;
        private long _cleanInterval = 60 * 10;
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取缓存实例唯一键
        /// </summary>
        public string Key { set; get; }
        #endregion

        #region 构造方法
        /// <summary>
        /// 全局初始化
        /// </summary>
        static MongoDbProvider()
        {
            BsonClassMap.RegisterClassMap<CacheItem>(map => {
                map.AutoMap();
                map.MapIdMember(x => x.Key);
                map.SetIgnoreExtraElements(true);
            });
        }

        /// <summary>
        /// 初始化提供程序
        /// </summary>
        /// <param name="key">缓存实例键</param>
        public MongoDbProvider(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            this.Key = key;
            var config = YunzhiAppConfiguration.Bind<CacheSection>("yunzhi:cache");

            var connstr = config.Parameters.GetValue<string>("mongodb");
            if (string.IsNullOrWhiteSpace(connstr))
                throw new Exception("请在缓存池中配置mongodb参数。");
            
            _cleanInterval = config.Parameters.GetValue<int>("cleanInterval", 10);
            _cleanInterval = Math.Max(_cleanInterval, 3);
            _cleanInterval *= 60;

            _access = new MongoDbAccess(connstr);

            this.CleanAsync();
        }

        /// <summary>
        /// 初始化提供程序
        /// </summary>
        /// <param name="key">缓存实例键</param>
        /// <param name="config">配置实例</param>
        public MongoDbProvider(string key, CacheInstanceElement config)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            this.Key = key;
            var connstr = config.Parameters.GetValue<string>("mongodb");
            if (string.IsNullOrWhiteSpace(connstr))
                throw new Exception("请在缓存池中配置mongodb参数。");

            _access = new MongoDbAccess(connstr);

            this.CleanAsync();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取集合操作实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private MongoDbCollectionAccess<CacheItem<T>> GetCollection<T>()
        {
            var coll = _access.GetCollectionAccess<CacheItem<T>>($"cache_{this.Key}");
            return coll;
        }
        #endregion

        #region 公共方法
        /// <inheritdoc/>
        public void Clean()
        {
            this.GetCollection<CacheItem>().Collection.DeleteMany(x => x.ExpiredTime <= DateTime.Now);
            _lastClean = Saber.Timestamp();
        }

        /// <summary>
        /// 异步方式清理过期数据，只有间隔到才会触发
        /// </summary>
        public void CleanAsync()
        {
            if (Saber.Timestamp() - _lastClean >= _cleanInterval)
                Task.Run(this.Clean);
        }

        /// <inheritdoc/>
        public long Count()
        {
            return this.GetCollection<CacheItem>().Collection.CountDocuments(new BsonDocument());
        }

        /// <inheritdoc/>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return this.GetCollection<CacheItem>().Collection.CountDocuments(x => x.Key == key) > 0;
        }

        /// <inheritdoc/>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            this.CleanAsync();

            var coll = this.GetCollection<T>();
            var item = coll.Get(x => x.Key == key, false);
            if (item == null || item.ExpiredTime < DateTime.Now)
            {
                coll.Delete(x => x.Key == key);
                return default(T);
            }

            //自动延展过期时间
            if (item.Extended && item.Expired.HasValue)
            {
                item.SetExpired(item.Expired);
                coll.Update(item, x => x.Key == key);
            }

            return item.Value;
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var coll = this.GetCollection<CacheItem>();
            coll.Delete(x => x.Key == key);
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, TimeSpan? expired = null, bool extended = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            this.CleanAsync();
            var item = new CacheItem<T>(key)
            {
                Value = value,
                Extended = extended
            };
            item.SetExpired(expired);

            var coll = this.GetCollection<T>();
            if (this.Exists(key))
                coll.Update(item, x => x.Key == key);
            else
                coll.Insert(item);
        }
        #endregion
    }
}
