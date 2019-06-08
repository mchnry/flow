using System.Collections.Generic;

namespace Mchnry.Flow.Analysis
{

    public interface IArticulateExpression {
        string Id { get; }
        string Literal { get; }
    }

    public interface IArticulateActivity {
        string Id { get; }
        string Literal { get; }

    }

    public class ArticulateAction : IArticulateActivity
    {
        public string Id { get; set; }
        public ArticulateContext Context { get; set; }
        public string Literal { get; set; }

    }
    public class NothingAction: IArticulateActivity
    {
        public string Id => "Nothing";
        public string Literal => "Do Nothing";
    }

    public class ArticulateActivity : IArticulateActivity
    {
        public string Id { get; set; }
        public List<ArticulateReaction> Reactions { get; set; }
        public string Literal => "Activity";
    }

    public class ArticulateContext
    {
        public string Literal { get; set; }
        public string Value { get; set; }
    }

    public class ArticulateReaction
    {
        public IArticulateExpression If { get; set; }
        public IArticulateActivity Then { get; set; }
    }

    public class ArticulateEvaluator : IArticulateExpression
    {
        public string Id { get; set; }
        public bool TrueCondition { get; set; }
        public ArticulateContext Context { get; set; }
        public string Literal { get; set; }
    }

    public class ArticulateExpression: IArticulateExpression
    {
        public string Id { get; set; }
        public IArticulateExpression First { get; set; }
        public string Condition { get; set; }
        public IArticulateExpression Second { get; set; }
        public string Literal => "Equation";
    }

    public class TrueExpression: IArticulateExpression
    {
        public string Id => "Always";
        public string Literal => "Always True";
    }


}
