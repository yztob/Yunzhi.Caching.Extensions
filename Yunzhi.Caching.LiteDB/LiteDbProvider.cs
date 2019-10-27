using System;
using System.Collections.Generic;
using System.Text;

using Yunzhi.Common;
using Yunzhi.Caching;
using Yunzhi.Caching.Configuration;
using LiteDB;

/*
 * 基于LiteDB的文件数据共享（缓存）提供程序
 *       
 * @Alphaair
 * 20191027 create.
**/

namespace Yunzhi.Caching.LiteDB
{
    /// <summary>
    /// 基于LiteDb缓存提供程序
    /// </summary>
    public class LiteDbProvider : ICacheProvider
    {
        #region 私有成员
        private string _liteDbPath = "lite_cache.db";
        private LiteDatabase _db = null;
        private LiteCollection<LiteCacheItem> _collection = null;
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
        public LiteDbProvider()
        {
            this.InitLiteDb();
        }

        /// <summary>
        /// 使用缓存配置，初始化提供程序
        /// </summary>
        /// <param name="config">配置实例</param>
        public LiteDbProvider(PoolElement config)
        {
            if (config != null)
                return;

            var path = config.Parameters?.GetValue("path");
            if (!string.IsNullOrWhiteSpace(path))
                _liteDbPath = path;

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
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 清除当前缓存队列中的所有缓存项
        /// </summary>
        public void Clean()
        {
            _db.DropCollection(this.Key);
        }

        /// <summary>
        /// 获取当前缓存队列中有多少缓存项
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return _collection.Count();
        }

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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            throw new NotImplementedException();
        }

        public void Set(string key, object value, TimeSpan? expired = null, bool extended = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            throw new NotImplementedException();
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
        }
        #endregion
    }
}
