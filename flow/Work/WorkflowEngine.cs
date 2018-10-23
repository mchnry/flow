using System;
using System.Collections.Generic;
using System.Text;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public class WorkflowEngine : IWorkflowEngine
    {
        public StepTraceNode<ActivityProcess> ProcessRoot => throw new NotImplementedException();

        StepTraceNode<ActivityProcess> IWorkflowEngine.CurrentProcess => throw new NotImplementedException();

        IActionFactory IWorkflowEngine.ActionFactory => throw new NotImplementedException();

        public StepTracer<string> Trace => throw new NotImplementedException();

        void IWorkflowEngine.Defer(IAction action, bool onlyIfValidationsResolved)
        {
            throw new NotImplementedException();
        }

        T IWorkflowEngine.GetStateObject<T>(ActivityProcess currentProcess, string key)
        {
            throw new NotImplementedException();
        }

        public T GetStateObject<T>(string key)
        {
            throw new NotImplementedException();
        }

        void IWorkflowEngine.Inject(Activity activityDefinition, object model)
        {
            throw new NotImplementedException();
        }

        void IWorkflowEngine.SetStateObject<T>(ActivityProcess currentProcess, string key, T toSave)
        {
            throw new NotImplementedException();
        }

        public void SetStateObject<T>(string key, T toSave)
        {
            throw new NotImplementedException();
        }
    }
}
