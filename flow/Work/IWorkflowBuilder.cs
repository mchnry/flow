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
}
