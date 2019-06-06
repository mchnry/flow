namespace Mchnry.Flow.Work
{


    public interface IWorkflowBuilderFactory
    {

        IWorkflowBuilder<T> GetWorkflow<T>(string workflowId);

    }


}
