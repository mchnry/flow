using System;

namespace Mchnry.Flow.Analysis
{
    public class HitAndRun: ICloneable
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

        public object Clone()
        {
            return new HitAndRun(this.Id) { HitCount = this.HitCount, RunCount = this.RunCount };
        }
    }



}
