using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{
    public struct HitAndRun
    {

        public HitAndRun(string Id)
        {
            this.Id = Id;
            this.HitCount = 0;
            this.RunCount = 0;
        }
        public int HitCount { get; set; }
        public int RunCount { get; set; }

        public string Id { get; }
    }
}
