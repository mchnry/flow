using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{


    public class LogicIntent
    {

        internal readonly string evaluatorId;

        public IContext Context { get; set; } = default(IContext);

        public LogicIntent(string evaluatorId)
        {
            this.evaluatorId = evaluatorId;
        }



        public Context<T> HasContext<T>()
        {

            //if my context is already valued, it means that this intent was 
            //already inferred.  If T is not string, then we need to convert the existing values
            //and throw an exception if explicit intent does not match inferrred.

            List<string> seed = null;
            if (this.Context != null)
            {
                seed = this.Context.Values;
            }

            Context<T> newContext = new Context<T>();
            if (seed != null)
            {
                newContext = newContext.HasValues(seed);
            }

            this.Context = newContext;
            return newContext;



        }
    }

    public interface IContext
    {
        List<string> Values { get; }
        ValidateOptions ListType { get; }
        bool Exclusive { get; }
    }

    public class Context<T> : IContext
    {



        internal ValidateOptions ListType { get; set; } = ValidateOptions.OneOf;
        ValidateOptions IContext.ListType { get => this.ListType; }
        bool IContext.Exclusive { get => this.Exclusive; }
        internal List<T> Values { get; set; } = new List<T>();
        internal bool Exclusive { get; set; } = false;

        List<string> IContext.Values {
            get {
                return (from x in this.Values
                        select x.ToString()).ToList();
            }
        }

        internal Context<T> HasValues(List<string> values)
        {
            this.Values = new List<T>();
            values.ForEach(s =>
            {
                //box/unbox
                T toAdd = (T)Convert.ChangeType(s, typeof(T));

                this.Values.Add(toAdd);
            });
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
