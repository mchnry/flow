using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mchnry.Flow.Test
{
    public class Lint
    {
        //context.oneof, anyof
        //

    }

    public class Intent
    {
        private readonly string evaluatorId;

        public Intent(string evaluatorId)
        {
            this.evaluatorId = evaluatorId;
        }
        public Context<T> HasContext<T>()
        {

        }
    }

    public class Context<T>
    {
        internal ContextOptions ListType { get; set; } = ContextOptions.OneOf;
        internal List<ContextItem<T>> Values { get; set; } = new List<ContextItem<T>>();
        public void OneOf(List<T> values)
        {
            Values = (from v in values select new ContextItem<T>(v)).ToList();
            
        }
        public void OneOf(List<ContextItem<T>> values)
        {
            Values = values;
        }
        
        public void AnyOf(List<T> values)
        {
            Values = (from v in values select new ContextItem<T>(v)).ToList();
            this.ListType = ContextOptions.AnyOf;
        }
        public void AnyOf(List<ContextItem<T>> values)
        {
            Values = values;
            this.ListType = ContextOptions.AnyOf;
        }
    }
    public enum ContextOptions
    {
        OneOf,
        AnyOf
    }
    public enum ContextItemOptions
    {
        Any,
        Null,
        Value
    }

    public class ContextItem<T>
    {
        internal ContextItemOptions ItemType { get; set; }
        internal T Value { get; set; }

        internal ContextItem(ContextItemOptions itemType) {
            Value = default(T);
            this.ItemType = itemType;
        }
        internal ContextItem(T value)
        {
            this.Value = value;
            this.ItemType = ContextItemOptions.Value;
        }

        public static ContextItem<T> Any {
            get {
                return new ContextItem<T>(ContextItemOptions.Any);
            }
        }
        public static ContextItem<T> Null {
            get {
                return new ContextItem<T>(ContextItemOptions.Null);
            }
        }


        

        

    }
}
