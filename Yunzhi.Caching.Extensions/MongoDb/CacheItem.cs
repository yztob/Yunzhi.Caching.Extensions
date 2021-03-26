using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

using Yunzhi.Caching;
using Yunzhi.NoSql;

/*
 * 表示一个缓存项目
 *       
 * @Alphaair
 * 20200211 create.
**/

namespace Yunzhi.Caching.Extensions.MongoDb
{
    /// <summary>
    /// 表示一个缓存项目
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CacheItem
    {
        #region 私有成员
        private DateTimeStorage _dateStorage = null;
        private TimeSpan? _expired = null;
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取缓存项键
        /// </summary>
        public string Key { get; private set; }
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
                    this.ExpiredTime = DateTime.Now.Add(value.Value);
                else
                    this.ExpiredTime = null;

                //永久有效
                if (value == TimeSpan.Zero)
                    this.ExpiredTime = DateTime.Now.AddYears(100);
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
                return _dateStorage.GetNullable(nameof(ExpiredTime));
            }

            set
            {
                //奇怪的会先于构造方法触发，疑似因为bson反列化的原因
                if (_dateStorage == null)
                    _dateStorage = new DateTimeStorage();

                _dateStorage.Storage(nameof(ExpiredTime), value);
            }
        }

        /// <summary>
        /// 获取或设置当前缓存项是否自动顺延过期时间。
        /// </summary>
        public bool Extended { get; set; }
        #endregion

        #region 构造方法
        /// <summary>
        /// 初始化缓存项目
        /// </summary>
        /// <param name="key"></param>
        public CacheItem(string key)
        {
            this.Key = key;
            _dateStorage = new DateTimeStorage();
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 设置过期时长，<see cref="Expired"/>与<see cref="ExpiredTime"/>联动设置
        /// 不使用Seter设定是因为，序列和反序列的原因导至死循环
        /// </summary>
        /// <param name="expired">过期时间，同<see cref="TimeSpan"/></param>
        public void SetExpired(TimeSpan? expired)
        {
            this.Expired = expired;

            if (expired.HasValue)
                this.ExpiredTime = DateTime.Now.Add(expired.Value);
            else if (expired == TimeSpan.Zero)            //永久有效
                this.ExpiredTime = DateTime.Now.AddYears(100);
            else
                this.ExpiredTime = null;

        }
        #endregion
    }
}
