using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mchnry.Flow.Cache
{
    public interface ICacheManager
    {
        /// <summary>
        /// Inserts object into the cache at the namespace.key index.
        /// </summary>
        /// <exception cref="System.ArgumentException">An element with the same key already exists.</exception>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        /// <param name="key">unique key within cache manager's namespace</param>
        /// <param name="value">object to insert</param>
        void Insert<T>(string key, T value);

        /// <summary>
        /// Determines if an item exists at the namespace.key index.
        /// </summary>
        /// <param name="key">unique key within cache manager's namespace</param>
        /// <returns></returns>
        bool Contains(string key);
        int Count { get; }
        string[] keys { get; }
        string NameSpace { get; }
        void Flush();
        //object GetData(string key);
        void Remove(string key);
        //object this[string key] { get; set; }
        string QualifiedKey(string key);
        ICacheManager Spawn(string nameSpace);
        bool BackingStoreAvailable { get; }

        T Read<T>(string key);

        T Read<T>(string key, Func<T> Get, bool cacheBeforeReturn);
        Task<T> ReadAsync<T>(string key, Func<Task<T>> Get, bool cacheBeforeReturn);
        T Read<T>(string key, Func<T> Get, Func<T, bool> forceRefresh, bool cacheBeforeReturn);

        Task<T> ReadAsync<T>(string key, Func<Task<T>> GetAsync, Func<T, bool> forceRefresh, bool cacheBeforeReturn);
    }
}
