using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Diagnostics
{
    public class NullStepTracer : IStepTracer
    {
        public StepTrace Root => "Null StepTracer";

        public void Flush()
        {
            //do nothing
        }

        public StepTrace Trace(StepTrace toTrace)
        {
            //do nothing
            return "Null StepTracer";
        }

        public StepTrace Trace(StepTrace previous, string toTrace)
        {
            return "Null StepTracer";
        }
    }
}
