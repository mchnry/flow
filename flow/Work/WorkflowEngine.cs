//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Mchnry.Flow.Diagnostics;
//using Mchnry.Flow.Work.Define;

//namespace Mchnry.Flow.Work
//{


    

//    public class WorkflowEngine
//    {

//        private WorkflowEngineStatusOptions status;
//        private List<IAction> finalize = new List<IAction>();
//        private List<IAction> finalizeAlways = new List<IAction>(); //actions to complete at end regardless of validation state

//        internal Dictionary<string, IAction> Actions { get; set; }
//        private Engine engineRef;

//        internal WorkflowEngine(Engine engineRef)
//        {

//            this.status = EngineStatusOptions.NotStarted;
//            this.engineRef = engineRef;
//        }


        
//        private Activity LoadActivity(string activityId)
//        {

//            Define.Activity definition = this.engineRef.WorkFlow.Activities.FirstOrDefault(a => a.Id == activityId);

//            Activity toReturn = new Activity(this, definition);

//            Action<Activity, Define.Activity> LoadReactions = null;
//            LoadReactions = (a, d) =>
//            {
//                if (d.Reactions != null && d.Reactions.Count > 0)
//                {
//                    d.Reactions.ForEach(r =>
//                    {
//                        Define.Activity toCreatedef = this.engineRef.WorkFlow.Activities.FirstOrDefault(z => z.Id == r.ActivityId);
//                        a.Reactions = new List<Reaction>();
//                        Activity toCreate = new Activity(this, toCreatedef);
//                        LoadReactions(toCreate, toCreatedef);
//                        a.Reactions.Add(new Reaction(r.EquationId, toCreate));
//                    });
//                }

//            };

//            LoadReactions(toReturn, definition);

//            return toReturn;

//        }

//    }
//}
