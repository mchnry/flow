using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Logic.Define
{

    public class RuleEqualityComparer : IEqualityComparer<Rule>
    {
        bool IEqualityComparer<Rule>.Equals(Rule x, Rule y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Rule>.GetHashCode(Rule obj)
        {
            return obj.GetHashCode();
        }
    }

    public class Rule: ICloneable, IExpression
    {

        public static implicit operator Rule(string shortHand)
        {
            string toParts = shortHand ?? throw new ArgumentNullException("shortHand");

            bool trueCondition = !toParts.StartsWith("!");
            toParts = toParts.Replace("!", string.Empty);

            string[] parts = toParts.Split('|');
            string id = parts[0];

            
            Context myContext = default;
            if (parts.Length > 1)
            {
                

                myContext = parts[1];


            }

            
            return new Rule() { Id = id, TrueCondition = trueCondition, Context = myContext };
            
        }


        public string Id { get; set; }

        public Context Context { get; set; }
  
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
                (this.Context == null) ? "" : "|" + this.Context.ToString());
        }
        [JsonIgnore]
        public string RuleIdWithContext {
            get {
                return string.Format("{0}{1}",
                
                this.Id,
                (this.Context == null) ? "" : "|" + this.Context.ToString());
            }
        }

        [JsonIgnore]
        public string ShortHand => string.Format("{0}{1}", (this.TrueCondition) ? "" : "!", this.RuleIdWithContext);

        public override bool Equals(object obj)
        {
            bool toReturn = false;
            if (obj != null && obj is Rule) {
                Rule toCompare = (Rule)obj;
                if (this.Id.Equals(toCompare.Id, StringComparison.OrdinalIgnoreCase))
                {
                    bool? sameName = (this.Context?.Name.Equals(toCompare.Context?.Name, StringComparison.OrdinalIgnoreCase));
                    if (sameName.GetValueOrDefault(false))
                    {
                        toReturn = ((this.Context.Keys.Count(toCompare.Context.Keys.Contains) == this.Context.Keys.Count()) 
                                && (toCompare.Context.Keys.Count(this.Context.Keys.Contains) == toCompare.Context.Keys.Count()));

                    }
                }
            }

            return toReturn;
        }
    }
}
