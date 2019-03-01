using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{
    public class ActivityTest
    {
        public ActivityTest(string activityId)
        {
            this.ActivityId = activityId ?? throw new ArgumentNullException(activityId);

        }

        public List<Case> TestCases { get; set; }

        public string ActivityId { get; internal set; }
   
    }
}
