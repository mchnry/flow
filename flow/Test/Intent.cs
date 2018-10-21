using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mchnry.Flow.Test
{


    public class Intent
    {

        internal readonly string evaluatorId;

        public IContext Context { get; set; } = default(IContext);

        public Intent(string evaluatorId)
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
    }

    public class Context<T>: IContext
    {



        internal ValidateOptions ListType { get; set; } = ValidateOptions.OneOf;
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
                object toAdd = s;
                this.Values.Add((T)toAdd);
            });
            return this;
        }
        

        public void OneOfExcusive()
        {
            this.Exclusive = true;
            this.ListType = ValidateOptions.OneOf;
        }
        public void OneOfInclusive()
        {
            this.Exclusive = false;
            this.ListType = ValidateOptions.OneOf;
        }
        public void AnyOfExclusive()
        {
            this.Exclusive = true;
            this.ListType = ValidateOptions.AnyOf;
        }
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

    //public class ContextItem<T>
    //{
    //    private ContextItemOptions ItemType { get; set; }
    //    public T Value { get; set; }

    //    internal ContextItem(ContextItemOptions itemType) {
    //        Value = default(T);
    //        this.ItemType = itemType;
    //    }
    //    internal ContextItem(T value)
    //    {
    //        this.Value = value;
    //        this.ItemType = ContextItemOptions.Value;
    //    }

    //    public static ContextItem<T> Any {
    //        get {
    //            return new ContextItem<T>(ContextItemOptions.Any);
    //        }
    //    }
    //    public static ContextItem<T> Null {
    //        get {
    //            return new ContextItem<T>(ContextItemOptions.Null);
    //        }
    //    }
    //    public static ContextItem<T> Is(T value)
    //    {
    //        return new ContextItem<T>(value);
    //    }
    //    public static ContextItem<T> Is(Func<T> getValue)
    //    {
    //        T value = getValue();
    //        return new ContextItem<T>(value);
    //    }

    //    public override string ToString()
    //    {
    //        string toReturn = string.Empty;
    //        switch (this.ItemType)
    //        {
    //            case ContextItemOptions.Any:
    //                toReturn = "any"; break;
    //            case ContextItemOptions.Null:
    //                toReturn = "null"; break;
    //            default:
    //                toReturn = this.Value.ToString();
    //                break;
    //        }
    //        return toReturn;
    //    }




    //}
}
