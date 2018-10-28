using System;

namespace Mchnry.Flow.Diagnostics
{

    public class WorkflowEngineTrace
    {
        internal WorkflowEngineTrace(EngineStepTracer tracer)
        {
            this.tracer = tracer;
        }

        private EngineStepTracer tracer;

        public void TraceStep(string toTrace)
        {
            string currentStep = tracer.CurrentStep.Node.Value.ActivityId;
            tracer.TraceStep(new ActivityProcess(currentStep, ActivityStatusOptions.Action_Running, DateTimeOffset.UtcNow));
        }
    }

    public class LogicEngineTrace
    {
        internal LogicEngineTrace(EngineStepTracer tracer)
        {
            this.tracer = tracer;
        }

        private EngineStepTracer tracer;

        public void TraceStep(string toTrace)
        {
            string currentStep = tracer.CurrentStep.Node.Value.ActivityId;
            tracer.TraceStep(new ActivityProcess(currentStep, ActivityStatusOptions.Rule_Evaluating, DateTimeOffset.UtcNow));
        }
    }

    internal class EngineStepTracer
    {

        private StepTracer<ActivityProcess> tracer;

        public StepTraceNode<ActivityProcess> Root { get; private set; }
        public StepTraceNode<ActivityProcess> CurrentStep { get; set; }

        private EngineStepTracer(ActivityProcess process)
        {
            this.tracer = new StepTracer<ActivityProcess>();
            this.Root = this.CurrentStep = this.tracer.TraceFirst(process);
        }

        public static EngineStepTracer StartRuleEngine()
        {
            return new EngineStepTracer(new ActivityProcess("Start", ActivityStatusOptions.RuleEngine_Begin, DateTimeOffset.UtcNow));
        }
        public static EngineStepTracer StartWorkflowEngine()
        {
            return new EngineStepTracer( new ActivityProcess("Start", ActivityStatusOptions.WorkflowEngine_Begin, DateTimeOffset.UtcNow));
        }


        public StepTraceNode<ActivityProcess> TraceStep(ActivityProcess value)
        {
            return this.tracer.TraceNext(this.CurrentStep, value);
            
        }
        public StepTraceNode<ActivityProcess> TraceStep(StepTraceNode<ActivityProcess> parent, ActivityProcess value)
        {
            return this.tracer.TraceNext(parent, value);
            
        }

    }


    public class StepTracer<T>
    {

        public StepTraceNode<T> Root { get; private set; }

        public StepTracer() { }

        internal StepTraceNode<T> TraceFirst(T value)
        {
            if (this.Root != null)
            {
                throw new InvalidOperationException("StepTracer already initialized");
            }
            this.Root = new StepTraceNode<T>(null, value);
            return this.Root;
        }


        public StepTraceNode<T> TraceNext(StepTraceNode<T> parent, T value)
        {
            StepTraceNode<T> toAdd = new StepTraceNode<T>(parent, value);
            parent.Children.Add(toAdd);
            return toAdd;
        }
    }

}
