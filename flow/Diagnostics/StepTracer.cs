using System;
using System.Collections.Generic;

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
            string currentStep = tracer.CurrentStep.Node.Value.ProcessId;
            tracer.TraceStep(new ActivityProcess(currentStep, ActivityStatusOptions.Action_Executing, toTrace));
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
            string currentStep = tracer.CurrentStep.Node.Value.ProcessId;
            tracer.TraceStep(new ActivityProcess(currentStep, ActivityStatusOptions.Rule_Executing, toTrace));
        }
    }

    internal class EngineStepTracer
    {

        internal StepTracer<ActivityProcess> tracer;

        public StepTraceNode<ActivityProcess> Root { get; private set; }
        public StepTraceNode<ActivityProcess> CurrentStep { get; set; }

        internal EngineStepTracer(ActivityProcess process)
        {
            this.tracer = new StepTracer<ActivityProcess>();
            this.Root = this.CurrentStep = this.tracer.TraceFirst(process);
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

        [Newtonsoft.Json.JsonIgnore]
        internal List<StepTraceNode<T>> AllNodes { get; set; }

        public StepTracer() { }

        internal StepTraceNode<T> TraceFirst(T value)
        {

            if (this.Root != null)
            {
                throw new InvalidOperationException("StepTracer already initialized");
            }
            AllNodes = new List<StepTraceNode<T>>();
            this.Root = new StepTraceNode<T>(null, value);
            AllNodes.Add(this.Root);
            return this.Root;
        }


        public StepTraceNode<T> TraceNext(StepTraceNode<T> parent, T value)
        {
            StepTraceNode<T> toAdd = new StepTraceNode<T>(parent, value);
            parent.Children.Add(toAdd);
            AllNodes.Add(toAdd);
            return toAdd;
        }
    }

}
