using System;
using System.Collections.Generic;
using System.Text;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IWorkflowDefinitionFactory
    {

        WorkDefine.Workflow GetWorkflow(string workflowId);

    }
}
