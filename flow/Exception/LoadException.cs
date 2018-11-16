using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Exception
{
    public class LoadEvaluatorException : System.Exception
    {

        public string EvaluatorId { get; set; }

        public LoadEvaluatorException(string evaluatorId) : base("Error Loading Evaluator")
        {
            this.EvaluatorId = evaluatorId;

        }

        public LoadEvaluatorException(string evaluatorId, System.Exception innerException) : base("Error Loading Evaluator", innerException)
        {
            this.EvaluatorId = evaluatorId;
         
        }

    }

    public class LoadActionException : System.Exception
    {

        public string ActionId { get; set; }

        public LoadActionException(string actionId) : base("Error Loading Action")
        {
            this.ActionId = actionId;

        }

        public LoadActionException(string actionId, System.Exception innerException) : base("Error Loading Action", innerException)
        {
            this.ActionId = actionId;

        }

    }
}
