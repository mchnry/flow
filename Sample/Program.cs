using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow;
using Mchnry.Flow.Analysis;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{

    class Program
    {
        /*
        Phase I
            User is looking at Item on vendor's website, and there appears to be one left in inventory.  But by the time that user clicks purchase, 
            that item has already been purchased. Need to decline the purchase, and let the user know that he/she missed the boat. 
            Logic "plain english"
            If item in inventory and payment authorizied/accepted
                decrement inventory
                record purchase
                send to shipping
                    send confirmation email
            if item not in inventory
                return validation to calling website
                    send notification to purchasing about missed opportunity
        */

        public static async Task Main(string[] args)
        {

            WorkDefine.Workflow ShoppingCartPurchase = new WorkDefine.Workflow()
            {
                Equations = new List<LogicDefine.Equation>()
                {
                    new LogicDefine.Equation() { Id = "equation.canBePurchased", Condition = Operand.And, First = "evaluator.isInInventory", Second = "evaluator.isPaymentAuthorized" }
                },
                Activities = new List<WorkDefine.Activity>()
                {
                    new WorkDefine.Activity() { Id = "activity.main", Reactions = new List<WorkDefine.Reaction>()
                        {
                            new WorkDefine.Reaction() { Logic = "equation.canBePurchased", Work="action.completePurchase" },
                            new WorkDefine.Reaction() { Logic = "!evaluator.isInInventory", Work="action.notifyPurchasing" }

                        }
                    }
                }
            };

            IEngineLoader workflowEngine = Engine.CreateEngine(ShoppingCartPurchase);
            workflowEngine.Lint((a) => { });

            var sanitizedWorkflow = workflowEngine.Workflow;

            string s = JsonConvert.SerializeObject(sanitizedWorkflow, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });
            Console.WriteLine(s);



            Console.ReadLine();

        }


    }
}
