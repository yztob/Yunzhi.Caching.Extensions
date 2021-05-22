using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Yunzhi.Common;
using Yunzhi.Caching;
using Yunzhi.Caching.Configuration;
using Newtonsoft.Json;
using LiteDB;

/*
 * 基于LiteDB的文件数据共享（缓存）提供程序
 *       
 * @Alphaair
 * 20191027 create.
 * 20210522 优化过期清理逻辑。
**/

namespace Yunzhi.Caching.Extensions.LiteDb
{
    /// <summary>
    /// 基于LiteDb缓存提供程序
    /// </summary>
    /// <remarks>
    /// path lite_db数据文件存放路径
    /// </remarks>
    public class LiteDbProvider : ICacheProvider
    {
        #region 私有成员
        private string _liteDbPath = "lite_cache.db";
        private LiteDatabase _db = null;
        private ILiteCollection<LiteCacheItem> _collection = null;
        private long _lastClean = 0;
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置当前提供程序的缓存唯一键
        /// </summary>
        public string Key
        {
            set;
            get;
        }
        #endregion

        #region 构造方法
        /// <summary>
        /// 初始化提供程序
        /// </summary>
        /// <param name="key">缓存实例键</param>
        public LiteDbProvider(string key)
        {
            this.Key = key;
            this.InitLiteDb();
        }

        /// <summary>
        /// 使用缓存配置，初始化提供程序
        /// </summary>
        /// <param name="key">缓存实例键</param>
        /// <param name="config">配置实例</param>
        public LiteDbProvider(string key, CacheInstanceElement config)
        {
            this.Key = key;
            if (config != null)
            {
                var path = config.Parameters?.GetValue("path");
                if (!string.IsNullOrWhiteSpace(path))
                    _liteDbPath = path;
            }

            this.InitLiteDb();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 初始LiteDB
        /// </summary>
        private void InitLiteDb()
        {
            _db = new LiteDatabase(_liteDbPath);
            _collection = _db.GetCollection<LiteCacheItem>(this.Key);
            _collection.EnsureIndex(x => x.Key, true);

            //进行一次清理
            this.Clean();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取<paramref name="key"/>指示的键是否已经在当前缓存队列中存在
        /// </summary>
        /// <param name="key">要检查的缓存Key</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return _collection.Exists(x => x.Key == key);
        }

        /// <summary>
        /// 获取指定键缓存值
        /// </summary>
        /// <param name="key">缓存项键</param>
        /// <typeparam name="T">缓存的 值类型</typeparam>
        /// <returns>存在返回值，否则返回<typeparamref name="T"/>的默认类型。</returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            this.CleanAsync();
            var item = _collection.FindById(key);
            if (item == null || item.ExpiredTime < DateTime.Now)
                return default(T);

            //自动延展过期时间
            if (item.Extended && item.Expired.HasValue)
            {
                item.Expired = item.Expired.Value;
                _collection.Update(item);
            }

            return JsonConvert.DeserializeObject<T>(item.Value);
        }

        /// <summary>
        /// 设定指定键缓存值
        /// </summary>
        /// <param name="key">缓存项键</param>
        /// <param name="value">缓存项值</param>
        /// <param name="expired">缓存项过期时长，null按默认过期时间处理，TimeSpan.Zero则最永久有效</param>
        /// <param name="extended">表示每次访问后是否将缓存顺延相应的时间。</param>
        /// <typeparam name="T">缓存的 值类型</typeparam>
        /// <remarks>如果缓存存在更新缓存值，否则创建一个新的缓存</remarks>
        public void Set<T>(string key, T value, TimeSpan? expired = null, bool extended = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            this.CleanAsync();
            var item = new LiteCacheItem()
            {
                Key = key,
                Value = JsonConvert.SerializeObject(value),
                Expired = expired,
                Extended = true
            };

            if (_collection.Exists(x => x.Key == key))
                _collection.Update(item);
            else
                _collection.Insert(item);
        }

        /// <summary>
        /// 删除<paramref name="key"/>指示键的缓存项
        /// </summary>
        /// <param name="key">要删除的缓存键</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _collection.Delete(key);
            this.CleanAsync();
        }

        /// <summary>
        /// 清理掉实例内已经过期的缓存
        /// </summary>
        public void Clean()
        {
            _collection.DeleteMany(x => x.ExpiredTime < DateTime.Now);
            _lastClean = Saber.Timestamp();
        }

        /// <summary>
        /// 异步方式清理过期数据，只有间隔到才会触发
        /// </summary>
        public void CleanAsync()
        {
            if (Saber.Timestamp() - _lastClean >= 60 * 10)
                Task.Run(this.Clean);
        }

        /// <summary>
        /// 获取当前缓存队列中有多少缓存项
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return _collection.Count();
        }

        #endregion
    }
}
