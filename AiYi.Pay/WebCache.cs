using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiYi.Pay
{
    public static class CacheExtensions
    {
        private static readonly object _syncObject = new object();

        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire, int cacheTime = 86400)
        {
            try
            {
                //lock (_syncObject)
                //{
                if (cacheManager.IsSet(key))
                {
                    return cacheManager.Get<T>(key);
                }
                else
                {
                    try
                    {
                        var result = acquire();
                        if (result != null)
                        {
                            cacheManager.Set<T>(key, result, cacheTime);
                            return result;
                        }
                        else
                        {
                            return default(T);
                        }
                    }
                    catch
                    {
                        return default(T);
                    }

                }
                //}
            }
            catch
            {
                return default(T);
            }
        }
    }
    public class MemoryCacheManager : ICacheManager
    {
        public static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }



        public bool IsSet(string key)
        {
            object cached;
            return _cache.TryGetValue(key, out cached);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveAll(params string[] keys)
        {
            keys.ToList().ForEach(item => _cache.Remove(item));
        }

        public bool Set<T>(string key, T data, int cacheTime)
        {
            if (cacheTime == 0)
            {
                cacheTime = 1800;
            }
            if (data == null)
                return false;
            TimeSpan ts = DateTime.Now.AddSeconds(cacheTime) - DateTime.Now;
            _cache.Set(key, data, new MemoryCacheEntryOptions().SetAbsoluteExpiration(ts));
            return true;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (_cache != null)
                _cache.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// 缓存管理接口
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>


        /// <summary>
        /// 加入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        bool Set<T>(string key, T data, int cacheTime);

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsSet(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
        void RemoveAll(params string[] keys);

    }
    public class WebCache
    {
        private static readonly ICacheManager _cacheManager = new MemoryCacheManager();
        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        public static bool Add(string key, object data, int cacheTime = 120)
        {
            return _cacheManager.Set(key, data, cacheTime);
        }
        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsSet(string key)
        {
            return _cacheManager.IsSet(key);
        }
        /// <summary>
        /// 移除指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            _cacheManager.Remove(key);
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="keys"></param>
        public void RemoveAll(params string[] keys)
        {
            _cacheManager.RemoveAll(keys);
        }
        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key, Func<T> acquire, int cacheTime = 120)
        {
            return _cacheManager.Get(key, acquire, cacheTime);
        }
    }
}