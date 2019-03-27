using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IWorkflowDefinitionFactory
    {

        IBuilderWorkflow<T> GetWorkflow<T>(string workflowId);

    }

    internal class NoWorkflowDefinitionFactory: IWorkflowDefinitionFactory
    {
        IBuilderWorkflow<T> IWorkflowDefinitionFactory.GetWorkflow<T>(string workflowId)
        {
            return new Builder<T>(workflowId);
        }
    }
}
