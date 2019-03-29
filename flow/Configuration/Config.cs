using Mchnry.Core.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Configuration
{
    public class Config
    {

        
        public Convention Convention { get; set; } = new Convention();

        public bool ValidateLintHash { get; set; } = true;

        public ICacheManager Cache { get; set; } = new MemoryCacheManager();

        internal int Ordinal { get; set; } = 0;

        


    }
}
