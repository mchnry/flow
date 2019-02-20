using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{

    public interface IRuleResult
    {
        void Fail();
        void FailWithValidation(Validation validation);
        void Pass();
        void SetResult(bool result);
    }

    internal class RuleResult : IRuleResult
    {
        private readonly Action<bool, Validation> status;

        public RuleResult(Action<bool, Validation> status) 
        {
            this.status = status;
        }
        void IRuleResult.Fail()
        {
            status(false, null);
        }

        void IRuleResult.FailWithValidation(Validation validation)
        {
            status(false, validation);
        }
        void IRuleResult.SetResult(bool result)
        {
            status(result, null);
        }

        void IRuleResult.Pass()
        {
            status(true, null);
        }
    }
}
