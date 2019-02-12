using Mchnry.Flow;
using Mchnry.Flow.Configuration;
using Xunit;

namespace Test.Analysis
{
    public class ConventionHelperTests
    {

        Config DefaultConfig = new Config();
        Config CustomConfig {  get {

                Config myConvention = new Config()
                {
                    Convention = new Convention()
                    {
                        Delimeter = "_"
                    }
                };
                myConvention.Convention.SetPrefix(NamePrefixOptions.Action, "a");
                myConvention.Convention.SetPrefix(NamePrefixOptions.Activity, "v");
                myConvention.Convention.SetPrefix(NamePrefixOptions.Equation, "q");
                myConvention.Convention.SetPrefix(NamePrefixOptions.Evaluator, "e");

                return myConvention;
            }
        }


        //negate good
        [Fact]
        public void Negate()
        {

            string action = "action.test";
            string renamedActivity = ConventionHelper.ChangePrefix(NamePrefixOptions.Action, NamePrefixOptions.Activity, action, this.DefaultConfig.Convention);
            string evaluator = "evaluator.test";
            string renamedEquation = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, evaluator, this.DefaultConfig.Convention);

            Assert.Equal("activity.test", renamedActivity);
            Assert.Equal("equation.test", renamedEquation);
        }

        //can't negate an action
        [Fact]
        public void NegateActionEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string action = "action.test";
                ConventionHelper.NegateEquationName(action, DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameActionToEquationEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string action = "action.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Action, NamePrefixOptions.Equation, action, this.DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameActionToEvaluatorEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string action = "action.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Action, NamePrefixOptions.Evaluator, action, this.DefaultConfig.Convention);
            });
        }

        [Fact]
        public void RenameEquationToEvaluator()
        {
            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string rule = "equation.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Equation, NamePrefixOptions.Evaluator, rule, this.DefaultConfig.Convention);
            });
        }

        [Fact]
        public void RenameEquationToActionEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string rule = "equation.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Equation, NamePrefixOptions.Action, rule, this.DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameEquationToActivityEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string rule = "equation.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Equation, NamePrefixOptions.Activity, rule, this.DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameEvaluatorToActionEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string rule = "evaluator.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Action, rule, this.DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameEvaluatorToActivityEx()
        {

            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string rule = "evaluator.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Activity, rule, this.DefaultConfig.Convention);
            });
        }
        [Fact]
        public void RenameActivityToActionEx()
        {
            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string action = "activity.test";
                ConventionHelper.ChangePrefix(NamePrefixOptions.Activity, NamePrefixOptions.Action, action, this.DefaultConfig.Convention);
            });
        }

        [Fact]
        public void NegateConventionNotFoundEx()
        {
            var exception = Assert.Throws<ConventionMisMatchException>(() =>
            {
                string equation = "mismatch.test";
                ConventionHelper.NegateEquationName(equation, this.DefaultConfig.Convention);
            });
        }

        //negate good, follows convention
        [Fact]
        public void NegateFollowsConvention()
        {
            string equation = "q_test";
            string negated = ConventionHelper.NegateEquationName(equation, this.CustomConfig.Convention);
            Assert.Equal("q_NOT_test", negated);
        }
        //change prefix good
        [Fact]
        public void ChangePrefixToEquation()
        {
            string equation = "e_test";
            string changed = ConventionHelper.ChangePrefix(NamePrefixOptions.Evaluator, NamePrefixOptions.Equation, equation, this.CustomConfig.Convention);

            Assert.Equal("q_test", changed);
        }
        [Fact]
        public void ChangePrefixToActivity()
        {
            string test = "a_test";
            string changed = ConventionHelper.ChangePrefix(NamePrefixOptions.Action, NamePrefixOptions.Activity, test, this.CustomConfig.Convention);

            Assert.Equal("v_test", changed);
        }
        //change prefix throws exception if convention not found



    }
}
