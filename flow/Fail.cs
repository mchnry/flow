using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    /// <summary>
    /// Passed to RuleEvaluator Implementations so that the 
    /// implementation can report its result to the engine.
    /// </summary>
    public interface IRuleResult
    {
        /// <summary>
        /// The result of the evaluation is false.
        /// </summary>
        void Fail();
        /// <summary>
        /// the result of the evaluation is false.
        /// </summary>
        /// <param name="validation">A validation to return to the caller.</param>
        void FailWithValidation(Validation validation);

        /// <summary>
        /// the result of the evaluation is true.
        /// </summary>
        void Pass();

        /// <summary>
        /// manually set the result of the evaluation.
        /// </summary>
        /// <param name="result"></param>
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
