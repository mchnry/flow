using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Work
{
    internal class Activity
    {
        private readonly IWorkflowEngine engineRef;
        private readonly Define.Activity activityDefinition;

        public Activity(IWorkflowEngine engineRef, Define.Activity activityDefinition)
        {
            this.engineRef = engineRef;
            this.activityDefinition = activityDefinition;
        }

        public bool Executed { get; set; } = false;
        public List<Reaction> Reactions { get; set; }


    }
}
