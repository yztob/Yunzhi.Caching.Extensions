using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

/*
 * 对CacheItem重新抽象以便于在LiteDB中存储
 *       
 * @Alphaair
 * 20191027 create.
**/

namespace Yunzhi.Caching.LiteDB
{
    /// <summary>
    /// 用于在LiteDB持久化存储的缓存条目
    /// </summary>
    public class LiteCacheItem
    {
        /// <summary>
        /// 获取或设置唯一键
        /// </summary>
        [BsonId]
        public string Key { get; set; }
        /// <summary>
        /// 获取或设置缓存值
        /// </summary>
        /// <remarks>序列化后的</remarks>
        public string Value { get; set; }
        /// <summary>
        /// 获取或设置获取或者设定过期时间，null按默认过期时间处理，TimeSpan.Zero则最永久有效
        /// </summary>
        public TimeSpan? Expired { get; set; }
        /// <summary>
        /// 获取或者设置缓存项到期时间
        /// </summary>
        public DateTime? ExpiredTime { get; set; }
        /// <summary>
        /// 获取或设置当前缓存项是否自动顺延过期时间
        /// </summary>
        public bool Extended { get; set; }
    }
}
