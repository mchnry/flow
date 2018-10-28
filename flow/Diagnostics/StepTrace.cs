namespace Mchnry.Flow.Diagnostics
{

    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class StepTrace<T>
    {

        public StepTrace(T value)
        {

            this.Value = value;
        }

        public T Value { get; }
    }

    public class StepTraceNode<T>
    {

        public StepTraceNode<T> Parent { get; }

        internal StepTraceNode(StepTraceNode<T> parent, T value)
        {
            this.Node = new StepTrace<T>(value);
            this.Parent = parent;
        }
        internal StepTraceNode<T> AddChild(T value)
        {
            StepTraceNode<T> toAdd = new StepTraceNode<T>(this, value);
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
