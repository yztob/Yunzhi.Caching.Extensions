using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

using Yunzhi.Caching;

/*
 * 泛型缓存项目
 *       
 * @Alphaair
 * 20200211 create.
**/

namespace Yunzhi.Caching.Extensions.MongoDb
{
    /// <summary>
    /// 泛型缓存项目
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CacheItem<T> : CacheItem
    {

        /// <summary>
        /// 获取或设置缓存条目值
        /// </summary>
        public T Value { set; get; }

        /// <summary>
        /// 初始化缓存项目
        /// </summary>
        /// <param name="key"></param>
        public CacheItem(string key)
            : base(key)
        {
        }
    }
}
