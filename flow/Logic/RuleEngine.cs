using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Logic
{

    public struct EvaluatorKey
    {
        public string id;
        public string context;
        public bool result;

    }

    public interface IRuleEngine
    {
        Dictionary<EvaluatorKey, IRuleEvaluator> Evaluators { get; }
        Dictionary<EvaluatorKey, bool?> Results { get; set; }
    }

    public class RuleEngine
    {
    }
}
