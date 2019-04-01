using Mchnry.Flow.Work;
using Mchnry.Flow.Work.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    internal class LogicTestDefintionFactory : IWorkflowBuilderFactory
    {
        private readonly Configuration.Config config;

        internal LogicTestDefintionFactory(Configuration.Config config)
        {
            this.config = config;
        }
        public IWorkflowBuilder<T> GetWorkflow<T>(string workflowId)
        {

            ActionDefinition def = new ActionDefinition()
            {
                Id = string.Format("{0}{1}{2}", workflowId, this.config.Convention.Delimeter, "fake"),
                Description = "Fake Action"
            };

            var wf = Builder<T>.CreateBuilder(workflowId, c =>
            {
                c.Cache = this.config.Cache;
                c.Convention = this.config.Convention;

            }).BuildFluent(Todo => Todo
                .Do(b => b.Do(new FakeAction<T>(def)))
                
            );

            return new WorkflowBuilder<T>(wf);
                
                
        }
    }
}
