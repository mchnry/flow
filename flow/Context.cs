using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mchnry.Flow
{

    public enum ValidateOptions
    {
        OneOf,
        AnyOf
    }

    public sealed class ContextDefinition: ICloneable
    {
     
        internal ContextDefinition(string name, string literal, IEnumerable<ContextItem> items, ValidateOptions validate, bool exclusive)
        {
            this.Items = new List<ContextItem>(items ?? throw new ArgumentException("Items required"));
            this.Name = name;
            this.Literal = literal;
            this.Validate = validate;
            this.Exclusive = exclusive;
        }

        public List<ContextItem> Items { get; internal set; }
        public string Name { get; internal set; }
        public string Literal { get; internal set; }
        public ValidateOptions Validate { get; internal set; }
        public bool Exclusive { get; internal set; }

        public object Clone()
        {
            return new ContextDefinition(this.Name, this.Literal, (from i in this.Items select i), this.Validate, this.Exclusive);
        }
    }

    public sealed class Context
    {
        public static implicit operator Context(string serialized)
        {
            if (!string.IsNullOrEmpty(serialized))
            {
                string[] parts = serialized.Split('[');
                return new Context(parts[1].Replace("]", string.Empty).Split(','), parts[0]);
            }
            return null;
        }

        internal Context(IEnumerable<string> keys,string name)
        {
            this.Keys = new List<string>(keys);
            this.Name = name;
        }

        public List<string> Keys { get; internal set; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{this.Name}[{string.Join(",", this.Keys)}]";
        }
    }


    public struct ContextItem
    {
        public string Key { get; set; }
        public string Literal { get; set; }
    }


}
