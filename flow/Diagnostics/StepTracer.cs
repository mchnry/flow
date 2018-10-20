using System;

namespace Mchnry.Flow.Diagnostics
{

    public interface IStepTracer<T>
    {
        void TraceStep(string step, T value);
    }

    public class StepTracer<T>
    {

        public StepTraceNode<T> Root { get; private set; }

        public StepTracer() { }

        internal StepTraceNode<T> TraceFirst(string step, T value)
        {
            if (this.Root != null)
            {
                throw new InvalidOperationException("StepTracer already initialized");
            }
            this.Root = new StepTraceNode<T>(null, step, value);
            return this.Root;
        }


        public StepTraceNode<T> TraceNext(StepTraceNode<T> parent, string step, T value)
        {
            StepTraceNode<T> toAdd = new StepTraceNode<T>(parent, step, value);
            parent.Children.Add(toAdd);
            return toAdd;
        }
    }

}
