using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mchnry.Flow.Cache
{

    public class MemoryCacheManager : CacheManagerBase
    {



        private System.Collections.Hashtable Cache = null;


        public MemoryCacheManager()
        {

            if (Cache == null)
                Cache = new System.Collections.Hashtable();
        }

        public MemoryCacheManager(string nameSpace)
        {
            this.Cache = new System.Collections.Hashtable();
            ns = nameSpace;
        }

        internal MemoryCacheManager(string nameSpace, System.Collections.Hashtable cache)
        {
            this.Cache = cache;
            ns = nameSpace;
        }

        #region ICacheManager Members

        public override ICacheManager Spawn(string nameSpace)
        {
            return new MemoryCacheManager(this.QualifiedKey(nameSpace), this.Cache);
        }

        public override void Insert<T>(string key, T value)
        {

            if ((key == null) || (key.Length == 0)) throw new ArgumentNullException("key is a null reference.");

            lock (Cache.SyncRoot)
            {

                //if (Cache.Contains(this.QualifiedKey(key))) throw new InvalidOperationException("An element with this key already exists.");

                Cache[this.QualifiedKey(key)] = value;
                //Cache.Add(this.QualifiedKey(key), value);
            }

        }

        public override bool Contains(string key)
        {
            lock (Cache.SyncRoot)
            {
                return Cache.Contains(this.QualifiedKey(key));
            }
        }

        public override int Count {
            get {
                return keys.Count();
            }
        }


        public override void Flush()
        {
            lock (Cache.SyncRoot)
            {

                IEnumerator e = this.keys.GetEnumerator();

                bool found = true;
                while (found)
                {
                    found = false;
                    e = this.keys.GetEnumerator();
                    while (e.MoveNext())
                    {
                        //if (e.Key.ToString().StartsWith(this.NameSpace, true, System.Globalization.CultureInfo.InvariantCulture))
                        //{
                        Cache.Remove(e.Current.ToString());
                        found = true;
                        break;
                        //}
                    }
                }
            }
        }


        public override void Remove(string key)
        {
            lock (Cache.SyncRoot)
            {
                Cache.Remove(this.QualifiedKey(key));
            }
        }


        public override T Read<T>(string key)
        {

            object cached = null;
            lock (Cache.SyncRoot)
            {
                cached = Cache[this.QualifiedKey(key)];
            }
            if (cached != null)
            {
                return (T)cached;
            }

            return default(T);


        }

        public override bool BackingStoreAvailable { get { return true; } }

        public override string[] keys {
            get {
                lock (Cache.SyncRoot)
                {


                    List<string> toReturn = new List<string>();

                    IEnumerator e = Cache.Keys.GetEnumerator();
                    string key;
                    while (e.MoveNext())
                    {
                        key = e.Current.ToString();
                        if (((string.IsNullOrEmpty(this.NameSpace)) || (e.Current.ToString().StartsWith(this.NameSpace))) && (!toReturn.Contains(key)))
                        {
                            toReturn.Add(key);
                        }
                    }

                    return toReturn.ToArray();

                }
            }
        }






        #endregion




    }

}
