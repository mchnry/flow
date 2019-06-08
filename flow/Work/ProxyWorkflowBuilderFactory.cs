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

        public Dictionary<string, IWorkflowBuilder<T>> Builders { get; set; }

        public ProxyWorkflowBuilderFactory(Configuration.Config config)
        {
  
            this.config = config;
        }
        
        public IWorkflowBuilder<T> GetBuilder(string workflowId)
        {

            var toReturn = this.Builders.FirstOrDefault(g => g.Key == workflowId).Value;
       
            return toReturn;
        }

    }


}
