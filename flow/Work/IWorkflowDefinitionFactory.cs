using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{

    public interface IWorkflowBuilder<T>
    {
        IBuilderWorkflow<T> GetBuilder();
    }

    public class WorkflowBuilder<T> : IWorkflowBuilder<T>
    {
        private readonly IBuilderWorkflow<T> workflow;

        public WorkflowBuilder(IBuilderWorkflow<T> workflow)
        {
            this.workflow = workflow;
        }
        public IBuilderWorkflow<T> GetBuilder()
        {
            return this.workflow;
        }
    }

    public interface IWorkflowBuilderFactory
    {

        IWorkflowBuilder<T> GetWorkflow<T>(string workflowId);

    }

    internal class NoWorkflowDefinitionFactory: IWorkflowBuilderFactory
    {
        IWorkflowBuilder<T> IWorkflowBuilderFactory.GetWorkflow<T>(string workflowId)
        {
            return new WorkflowBuilder<T>(new Builder<T>(workflowId));
        }
    }
}
