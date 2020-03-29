using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mchnry.Flow.Cache
{
    public abstract class CacheManagerBase : ICacheManager
    {

        protected string ns = string.Empty;

        //public abstract object this[string key] { get; set; }

        public abstract bool BackingStoreAvailable { get; }
        public abstract int Count { get; }
        public abstract string[] keys { get; }


        public abstract bool Contains(string key);
        public abstract void Flush();

        public abstract void Insert<T>(string key, T value);

        public abstract void Remove(string key);
        public abstract ICacheManager Spawn(string nameSpace);

        public string NameSpace {
            get {
                return this.ns;
            }
        }

        public string QualifiedKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return this.NameSpace;

            if (key.StartsWith(".")) key = key.Substring(1, key.Length - 1);
            if (key.EndsWith(".")) key = key.Substring(0, key.Length - 1);


            return (this.NameSpace.Length == 0) ? key : this.NameSpace + "." + key;
        }

        public abstract T Read<T>(string key);


        public T Read<T>(string key, Func<T> Get, bool cacheBeforeReturn)
        {



            object toReturn = Read<T>(key);

            if (toReturn != null)
            {
                return (T)toReturn;
            }
            {
                T created = Get();

                if (cacheBeforeReturn && ((ICacheManager)this).BackingStoreAvailable) Insert<T>(key, created);

                return created;

            }
        }


        public T Read<T>(string key, Func<T> Get, Func<T, bool> forceRefresh, bool cacheBeforeReturn)
        {
            object toReturn = Read<T>(key);

            if (toReturn != null)
            {

                if (forceRefresh((T)toReturn))
                {
                    this.Remove(key);
                    return this.Read<T>(key, Get, cacheBeforeReturn);

                }
                else
                {

                    return (T)toReturn;
                }
            }

            T created = Get();

            if (cacheBeforeReturn && this.BackingStoreAvailable) this.Insert<T>(key, created);

            return created;


        }

        public async Task<T> ReadAsync<T>(string key, Func<Task<T>> GetAsync, Func<T, bool> forceRefresh, bool cacheBeforeReturn)
        {

            object toReturn = Read<T>(key);
            T created = default(T);

            if (toReturn != null)
            {

                if (forceRefresh((T)toReturn))
                {
                    this.Remove(key);
                    return await this.ReadAsync<T>(key, GetAsync, cacheBeforeReturn);

                }
                else
                {

                    return (T)toReturn;
                }
            }

            created = await GetAsync();

            if (cacheBeforeReturn && this.BackingStoreAvailable) this.Insert<T>(key, created);

            return created;


        }

        public async Task<T> ReadAsync<T>(string key, Func<Task<T>> GetAsync, bool cacheBeforeReturn)
        {
            object toReturn = Read<T>(key);


            if (toReturn != null)
            {
                return (T)toReturn;
            }
            {
                T created = await GetAsync();

                if (cacheBeforeReturn && ((ICacheManager)this).BackingStoreAvailable) this.Insert<T>(key, created);

                return created;

            }

        }
    }
}
