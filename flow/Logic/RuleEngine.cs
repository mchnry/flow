namespace Mchnry.Flow.Logic
{
    using Mchnry.Core.Cache;
    using Mchnry.Flow.Logic.Define;
    using System;
    using System.Collections.Generic;
    using System.Text;


    public class RuleEngine : IRuleEngine
    {
        public string CurrentProcessId => throw new NotImplementedException();

        public ICacheManager State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IValidationContainer GetContainer(Evaluator definition, string context)
        {
            throw new NotImplementedException();
        }

        public IRuleEvaluator GetEvaluator(Evaluator definition)
        {
            throw new NotImplementedException();
        }

        public bool? GetResult(Evaluator defintion, string context)
        {
            throw new NotImplementedException();
        }

        public void SetResult(Evaluator definition, string context, bool result)
        {
            throw new NotImplementedException();
        }
    }
}
