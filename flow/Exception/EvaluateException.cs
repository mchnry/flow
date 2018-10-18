using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Exception
{
    public class EvaluateException: System.Exception
    {

        public string EvaluatorId { get; set; }
        public string Context { get; set; }
        public EvaluateException(string evaluatorId, string context, System.Exception innerException): base("Error in Evaluator", innerException)
        {
            this.EvaluatorId = evaluatorId;
            this.Context = context;
        }
       
    }
}
