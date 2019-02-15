using Newtonsoft.Json;
using System;

namespace Mchnry.Flow.Logic.Define
{
    public class Rule: ICloneable, IExpression
    {

        public static implicit operator Rule(string shortHand)
        {
            string toParts = shortHand ?? throw new ArgumentNullException("shortHand");

            bool trueCondition = !toParts.StartsWith("!");
            toParts = toParts.Replace("!", string.Empty);

            string[] parts = toParts.Split('|');
            string id = parts[0];

            string ctx = string.Empty;
            if (parts.Length > 1)
            {
                ctx = parts[1];
            }

            return new Rule() { Id = id, TrueCondition = trueCondition, Context = ctx };

        }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "ctx")]
        public string Context { get; set; }
        [JsonProperty(PropertyName = "t")]
        public bool TrueCondition { get; set; }

        public object Clone()
        {
            return new Rule() { Context = this.Context, Id = this.Id, TrueCondition = this.TrueCondition };
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}",
                (TrueCondition) ? "" : "!",
                this.Id,
                (string.IsNullOrEmpty(this.Context)) ? "" : "|" + this.Context);
        }
        [JsonIgnore]
        internal string RuleIdWithContext {
            get {
                return string.Format("{0}{1}",
                
                this.Id,
                (string.IsNullOrEmpty(this.Context)) ? "" : "|" + this.Context);
            }
        }
    }
}
