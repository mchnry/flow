using Mchnry.Core.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Logic
{

    public struct EvaluatorKey
    {
        private string context;

        public string Id { get; set; }
        public string Context {
            get {
                return this.context ?? string.Empty;

            } set { this.context = value; }
        }

    }

    public interface IRuleEngine
    {
        IRuleEvaluator GetEvaluator(Define.Evaluator definition);

        bool? GetResult(Define.Evaluator defintion, string context);
        void SetResult(Define.Evaluator definition, string context, bool result);
        string CurrentProcessId { get; }
        ICacheManager State { get; set; }

        IValidationContainer GetContainer(Define.Evaluator definition, string context);
    }

    public class RuleEngine
    {
    }
}
