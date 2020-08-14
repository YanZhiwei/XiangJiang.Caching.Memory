using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using XiangJiang.Caching.Abstractions;
using XiangJiang.Common;
using XiangJiang.Core;

namespace XiangJiang.Caching.Memory
{
    /// <summary>
    ///     本地内存缓存
    /// </summary>
    /// <seealso cref="ICacheProvider" />
    public sealed class MemoryCacheProvider : ICacheProvider
    {
        #region Fields

        /// <summary>
        ///     ObjectCache
        /// </summary>
        /// <value>
        ///     The cache.
        /// </value>
        private readonly ObjectCache _cache;

        #endregion Fields

        #region Constructors

        public MemoryCacheProvider()
        {
            _cache = MemoryCache.Default;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     根据Key获取缓存
        /// </summary>
        /// <typeparam name="T">缓存类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>
        ///     缓存
        /// </returns>
        public T Get<T>(string key)
        {
            Checker.Begin().NotNullOrEmpty(key, nameof(key));
            return (T)_cache[key];
        }

        /// <summary>
        ///     是否设置缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>
        ///     <c>true</c> if the specified key is set; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSet(string key)
        {
            Checker.Begin().NotNullOrEmpty(key, nameof(key));
            return _cache.Contains(key);
        }

        /// <summary>
        ///     移除缓存
        /// </summary>
        /// <param name="key">键</param>
        public void Remove(string key)
        {
            Checker.Begin().NotNullOrEmpty(key, nameof(key));
            _cache.Remove(key);
        }

        /// <summary>
        ///     根据正则表达式移除缓存
        /// </summary>
        /// <param name="pattern">移除缓存</param>
        public void RemoveByPattern(string pattern)
        {
            Checker.Begin().NotNullOrEmpty(pattern, nameof(pattern));
            this.RemoveByPattern(pattern, _cache.Select(p => p.Key));
        }

        /// <summary>
        ///     设置缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="data">值</param>
        /// <param name="cacheTimeMinute">过期时间，单位分钟</param>
        public void Set(string key, object data, uint cacheTimeMinute)
        {
            Checker.Begin()
                .NotNullOrEmpty(key, nameof(key))
                .NotNull(data, nameof(data));
            if (!CheckCacheData(data)) return;

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTimeMinute)
            };
            _cache.Add(new CacheItem(key, data), policy);
        }

        /// <summary>
        ///     设置缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="data">值</param>
        /// <param name="dependFile">文件依赖</param>
        /// <exception cref="FileNotFoundException"></exception>
        public void Set(string key, object data, string dependFile)
        {
            Checker.Begin()
                .NotNullOrEmpty(key, nameof(key))
                .NotNull(data, nameof(data))
                .CheckFileExists(dependFile);
            if (!CheckCacheData(data)) return;
            var policy = new CacheItemPolicy();
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { dependFile }));
            _cache.Add(new CacheItem(key, data), policy);
        }

        private bool CheckCacheData(object data)
        {
            if (data == null) return false;
            if (data is IEnumerable enumerable && enumerable.IsNullOrEmpty()) return false;
            return true;
        }

        #endregion Methods
    }
}
