using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow.Work
{
    internal class ProxyWorkflowBuilderFactory<T>
    {
        private readonly Configuration.Config config;

        public IWorkflowBuilderFactory Proxy { get;set; }
        public Dictionary<string, IWorkflowBuilder<T>> Builders { get; set; }

        public ProxyWorkflowBuilderFactory(Configuration.Config config)
        {
            this.Proxy = new NoWorkflowDefinitionFactory();
            this.config = config;
        }
        
        public IWorkflowBuilder<T> GetBuilder(string workflowId)
        {

            var toReturn = this.Builders.FirstOrDefault(g => g.Key == workflowId).Value;
            if (toReturn == null)
            {
                toReturn = Proxy.GetWorkflow<T>(workflowId);

            }
            return toReturn;
        }

    }

    internal class NoWorkflowDefinitionFactory : IWorkflowBuilderFactory
    {
        IWorkflowBuilder<T> IWorkflowBuilderFactory.GetWorkflow<T>(string workflowId)
        {
            return null;
            //return new WorkflowBuilder<T>(new Builder<T>(workflowId));
        }
    }
}
