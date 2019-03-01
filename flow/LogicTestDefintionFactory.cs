using Mchnry.Flow.Work;
using Mchnry.Flow.Work.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    internal class LogicTestDefintionFactory : IWorkflowDefinitionFactory
    {
        private readonly Configuration.Config config;

        internal LogicTestDefintionFactory(Configuration.Config config)
        {
            this.config = config;
        }
        public Workflow GetWorkflow(string workflowId)
        {
            Workflow toReturn = Builder.CreateBuilder(workflowId, c =>
            {
                c.Cache = this.config.Cache;
                c.Convention = this.config.Convention;

            }).Build(Todo => Todo
                .Do(string.Format("{0}{1}{2}", workflowId, this.config.Convention.Delimeter, "fake"))
            );

            return toReturn;
                
                
        }
    }
}
