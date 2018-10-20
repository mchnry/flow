namespace Mchnry.Flow.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class StepTrace<T>
    {

        public StepTrace(string step, T value)
        {
            this.Step = step;
            this.Value = value;
        }

        public string Step { get; }
        public T Value { get; }
    }

    public class StepTraceNode<T>
    {

        public StepTraceNode<T> Parent { get; }

        internal StepTraceNode(StepTraceNode<T> parent, string step, T value)
        {
            this.Node = new StepTrace<T>(step, value);
            this.Parent = parent;
        }
        internal StepTraceNode<T> AddChild(string step, T value)
        {
            StepTraceNode<T> toAdd = new StepTraceNode<T>(this, step, value);
            this.Children.Add(toAdd);
            return toAdd;
        }

        public StepTrace<T> Node { get; private set; }
        internal List<StepTraceNode<T>> Children { get; set; } = new List<StepTraceNode<T>>();

        public ReadOnlyCollection<StepTraceNode<T>> ChildNodes { get {
                return this.Children.AsReadOnly();
            }
        }
        

        
    }
}
