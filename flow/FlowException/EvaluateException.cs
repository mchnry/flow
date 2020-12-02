using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Logic;

namespace Mchnry.Flow
{
    public class EvaluateException: System.Exception
    {

        public string EvaluatorId { get; set; }
        public string Context { get; set; }
        public IRuleEvaluatorX<string> ShouldIDoIt { get; set; }

        public EvaluateException(string evaluatorId, string context, System.Exception innerException): base("Error in Evaluator", innerException)
        {
            this.EvaluatorId = evaluatorId;
            this.Context = context;
        }
       
    }
}
