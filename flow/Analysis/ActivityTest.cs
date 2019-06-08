using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{

    /// <summary>
    /// Container for the lint results of an activity test
    /// </summary>
    public class ActivityTest
    {
        /// <summary>
        /// Constructs an activity test for the given activity
        /// </summary>
        /// <param name="activityId">Id of the activity represented by this test</param>
        internal ActivityTest(string activityId)
        {
            this.ActivityId = activityId ?? throw new ArgumentNullException(activityId);

        }

        /// <summary>
        /// An activity test will have test cases for each logical permutation of all rules
        /// involved in the activity (with context factored in)
        /// </summary>
        public List<Case> TestCases { get; set; } = new List<Case>();

        /// <summary>
        /// Activity Id
        /// </summary>
        public string ActivityId { get; internal set; }
   
    }
}
