using Mchnry.Flow.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{

    public interface INeedIntent
    {
        LogicIntent Intent(string evaluatorId);
    }

    public class LogicIntent
    {

        internal readonly string evaluatorId;

        public IContext Context { get; set; } = default(IContext);
        

        public LogicIntent(string evaluatorId)
        {
            this.evaluatorId = evaluatorId;
            
        }



        public Context HasContext(string literal)
        {

            //if my context is already valued, it means that this intent was 
            //already inferred.  If T is not string, then we need to convert the existing values
            //and throw an exception if explicit intent does not match inferrred.

            List<ContextItem> seed = null;
            if (this.Context != null)
            {
                seed = this.Context.Values;
            }

            Context newContext = new Context(literal);
            if (seed != null)
            {
                newContext = newContext.HasValues(seed);
            }

            this.Context = newContext;
            return newContext;



        }
    }

    public struct ContextItem
    {
        public string Key { get; set; }
        public string Literal { get; set; }
    }

    public interface IContext
    {
        string Literal { get; }
        List<ContextItem> Values { get; }
        ValidateOptions ListType { get; }
        bool Exclusive { get; }
    }

    public class Context : IContext
    {
        public Context(string literal)
        {
            this.Literal = literal;
        }


        internal ValidateOptions ListType { get; set; } = ValidateOptions.OneOf;
        ValidateOptions IContext.ListType { get => this.ListType; }
        bool IContext.Exclusive { get => this.Exclusive; }
        internal List<ContextItem> Values { get; set; } = new List<ContextItem>();
        internal bool Exclusive { get; set; } = false;

        List<ContextItem> IContext.Values {
            get {
                return (from x in this.Values
                        select x).ToList();
            }
        }

        public string Literal { get; }

        internal Context InitializeValues(List<ContextItem> values)
        {
            this.Values = new List<ContextItem>();
            values.ForEach(s =>
            {
                //box/unbox
                //T toAdd = (T)Convert.ChangeType(s, typeof(T));

                this.Values.Add(s);
            });
            return this;
        }


        /// <summary>
        /// Options are inferred when traversing the logic.  However, caller can
        /// use this to provide details (literals) about those options.
        /// Caller need only provide the values that exist in the logic
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public Context HasValues(List<ContextItem> values)
        {
            if (this.Values != null)
            {
                List<ContextItem> merged = values;
                //this.values is inferred from the logic set, so the items in it are defacto.  
                //if the callers values are in the defacto set, use the callers, otherwise, it is superfluous, so get rid of it.
                foreach(ContextItem i in this.Values)
                {
                    var match = (from g in values where g.Key == i.Key select g).FirstOrDefault();
                    if (!string.IsNullOrEmpty(match.Key))
                    {
                        merged.Add(match);
                    } else
                    {
                        merged.Add(i);
                    }

                }
                this.Values = merged;
            } else
            {
                this.Values = values;
            }
            return this;

  
        }

        /// <summary>
        /// Expects that the test value can only be one of the items in the set of context, and that the 
        /// set of context is finite.
        /// </summary>
        /// <example>when an equation tests for multiple cases - a,b,c - this means that only one
        /// of those cases can be true, so we only need to test three cases.  Additionally, because
        /// its exclusive, one of these must be true</example>
        public void OneOfExcusive()
        {
            this.Exclusive = true;
            this.ListType = ValidateOptions.OneOf;
        }
        /// <summary>
        /// Expects that the test value can only be one of the items in the set of context, and that the 
        /// set of context is not finite, but the caller is only providing the test cases.
        /// </summary>
        /// <example>when an equation tests for multiple cases - a,b,c - this means that only one
        /// of those cases can be true, so we only need to test three cases.  Additionally, because
        /// its inclusive, there may be a case where all are false</example>
        public void OneOfInclusive()
        {
            this.Exclusive = false;
            this.ListType = ValidateOptions.OneOf;
        }
        /// <summary>
        /// Expects that the test value can be any of the items in the set of context, and that the 
        /// set of context is finite.
        /// </summary>
        /// <example>when an equation tests for multiple cases - a,b,c - this means that any
        /// of those cases can be true, so we need to test all combinations (n^n).  Additionally, because
        /// its exclusive, one of these must be true</example>
        public void AnyOfExclusive()
        {
            this.Exclusive = true;
            this.ListType = ValidateOptions.AnyOf;
        }

        /// <summary>
        /// Expects that the test value can be any of the items in the set of context, and that the 
        /// set of context is not finite, but the caller is only providing the test cases.
        /// </summary>
        /// <example>when an equation tests for multiple cases - a,b,c - this means that any
        /// of those cases can be true, so we need to test all combinations (n^n).  Additionally, because
        /// its inclusive, there may be a case where all are false</example>
        public void AnyOfInclusive()
        {
            this.Exclusive = false;
            this.ListType = ValidateOptions.AnyOf;
        }

    }
    public enum ValidateOptions
    {
        OneOf,
        AnyOf
    }


}
