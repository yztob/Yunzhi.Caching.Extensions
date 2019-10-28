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
        #region 私有成员
        private TimeSpan? _expired = null;
        private DateTime? _expiredDate = null;
        #endregion

        #region 公共方法

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
        /// 获取或者设定过期时间，null按默认过期时间处理，TimeSpan.Zero则最永久有效
        /// </summary>
        /// <remarks>
        /// 设置当前属性后，会以自动当前时间计算<see cref="ExpiredTime"/>，并赋值。
        /// 设置本属性主要是为了<see cref="Extended"/>延展需要。
        /// </remarks>
        public TimeSpan? Expired
        {
            set
            {
                _expired = value;

                if (value.HasValue)
                    _expiredDate = DateTime.Now.Add(value.Value);

                //永久有效
                if (value == TimeSpan.Zero)
                    _expiredDate = DateTime.Now.AddYears(100);
            }
            get
            {
                return _expired;
            }
        }

        /// <summary>
        /// 获取或者设置缓存项到期时间。
        /// </summary>
        public DateTime? ExpiredTime
        {
            get
            {
                return _expiredDate;
            }
            set
            {
                _expiredDate = value;
            }
        }
        /// <summary>
        /// 获取或设置当前缓存项是否自动顺延过期时间
        /// </summary>
        public bool Extended { get; set; }
        #endregion

    }
}
