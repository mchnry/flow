using System;

namespace Mchnry.Flow.Diagnostics
{

    public interface IStepTracer<T>
    {
        void TraceStep(string step, T value);
    }

    internal class EngineStepTracer : IStepTracer<ActivityProcess>
    {

        private StepTracer<ActivityProcess> tracer;

        public StepTraceNode<ActivityProcess> Root { get; private set; }
        public StepTraceNode<ActivityProcess> CurrentStep { get; set; }

        private EngineStepTracer(string step, ActivityProcess process)
        {
            this.tracer = new StepTracer<ActivityProcess>();
            this.Root = this.CurrentStep = this.tracer.TraceFirst(step, process);
        }

        public static EngineStepTracer StartRuleEngine()
        {
            return new EngineStepTracer("StartRuleEngine", new ActivityProcess("Start", ActivityStatusOptions.RuleEngine_Begin, DateTimeOffset.UtcNow));
        }
        public static EngineStepTracer StartWorkflowEngine()
        {
            return new EngineStepTracer("StartWorkflowEngine", new ActivityProcess("Start", ActivityStatusOptions.WorkflowEngine_Begin, DateTimeOffset.UtcNow));
        }

        void IStepTracer<ActivityProcess>.TraceStep(string step, ActivityProcess value)
        {
            this.tracer.TraceNext(this.CurrentStep, step, value);
        }

        public StepTraceNode<ActivityProcess> TraceStep(string step, ActivityProcess value)
        {
            return this.tracer.TraceNext(this.CurrentStep, step, value);
            
        }
        public StepTraceNode<ActivityProcess> TraceStep(StepTraceNode<ActivityProcess> parent, string step, ActivityProcess value)
        {
            return this.tracer.TraceNext(parent, step, value);
            
        }

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
