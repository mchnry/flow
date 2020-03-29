using Newtonsoft.Json;

namespace Mchnry.Flow.Logic.Define
{
    public class Equation: IExpression
    {

        public string Id { get; set; }

        public bool TrueCondition { get; set; } = true;
        
        public Rule First { get; set; }

        
        public Rule Second { get; set; }

        
        public Operand Condition { get; set; }

        [JsonIgnore]
        public string RuleIdWithContext => this.Id;

        [JsonIgnore]
        public string ShortHand => string.Format("{0}{1}", this.TrueCondition ? "" : "!", this.Id);
    }
}
