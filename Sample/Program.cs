using Mchnry.Flow;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Sample
{
    #region Services

    public class ServiceFactory
    {
        public static InventoryService inventoryService = new InventoryService();
        public static PaymentService paymentService = new PaymentService();
        public static InventoryService InventoryService {
            get {
                return inventoryService;
            }
        }
        public static PaymentService PaymentService {
            get {
                return paymentService;
            }
        }
    }


    public class InventoryService
    {
        public static int Inventory = 1;
        public async Task<int> GetInventoryCount(string SKU)
        {
            return Inventory;
        }
        public async Task<int> DecrementInventory(string SKU)
        {
            if (Inventory > 0)
            {
                Inventory -= 1;
            }
            return Inventory;
        }
    }

    public class PaymentService
    {
        public async Task<bool> IsAuthorizied(string payment)
        {
            return payment.Contains("good");
        }
    }
    #endregion

    #region Actions
    public class CompletePurchaseAction : IAction
    {
        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            //the key name for getmodel is a user defined convention.  You can call it what you want, just be consistent.
            ShoppingCart model = scope.GetModel<ShoppingCart>("model");
            string sku = model.ItemSKU;
            InventoryService svc = ServiceFactory.InventoryService;
            int remaining = await svc.DecrementInventory(sku);
            trace.TraceStep(string.Format("Completing Purchase of {0}.  {1} left in inventory", sku, remaining));
            return true;
        }
    }
    public class NotifyPurchasingAction : IAction
    {
        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            ShoppingCart model = scope.GetModel<ShoppingCart>("model");
            string sku = model.ItemSKU;
            trace.TraceStep(string.Format("Notifying Purchasing that we're out of stock of {0}", sku));
            return true;
        }
    }
    #endregion

    #region Evaluators
    public class IsInInventoryEvaluator : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {
            ShoppingCart model = scope.GetModel<ShoppingCart>("model");
            //check inventory to see if item is still in stock
            string sku = model.ItemSKU;
            InventoryService svc = ServiceFactory.InventoryService;

            int inventoryCount = await svc.GetInventoryCount(sku);

            return inventoryCount > 0;


        }
    }
    public class IsPaymentAuthorizedEvaluator : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(IEngineScope scope, LogicEngineTrace trace, CancellationToken token)
        {
            ShoppingCart model = scope.GetModel<ShoppingCart>("model");
            //check if payment method is good
            PaymentService svc = ServiceFactory.PaymentService;

            bool isGood = await svc.IsAuthorizied(model.Payment);

            return isGood;
        }
    }

    #endregion

    #region Factories

    public class ActionFactory : Mchnry.Flow.Work.IActionFactory
    {
        public IAction GetAction(WorkDefine.ActionDefinition definition)
        {
            //note.. one would hopefully use di for this... but for this example, we'll brute force it

            switch(definition.Id)
            {
                case "action.completePurchase":
                    return new CompletePurchaseAction();

                case "action.notifyPurchasing":
                    return new NotifyPurchasingAction();
                default:
                    return null;
            }

            
        }
    }

    public class EvaluatorFactory : Mchnry.Flow.Logic.IRuleEvaluatorFactory
    {
        public IRuleEvaluator GetRuleEvaluator(LogicDefine.Evaluator definition)
        {

            //note.. one would hopefully use di for this... but for this example, we'll brute force it

            switch (definition.Id)
            {
                case "evaluator.isInInventory":
                    return new IsInInventoryEvaluator();

                case "evaluator.isPaymentAuthorized":
                    return new IsPaymentAuthorizedEvaluator();
                default:
                    return null;
            }
        }
    }
    #endregion

    #region Models 
    public class ShoppingCart
    {
        public string ItemSKU { get; set; }
        public string Payment { get; set; }
    }
    #endregion

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

            //lint
            IEngineLoader workflowEngine = Engine.CreateEngine(ShoppingCartPurchase);
            var result = workflowEngine.Lint((a) => { });
            var sanitizedWorkflow = workflowEngine.Workflow;
            string s = JsonConvert.SerializeObject(sanitizedWorkflow, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });
            Console.WriteLine(s);

            //IEngineLoader wfeLoader = Engine.CreateEngine(ShoppingCartPurchase);
            //IEngineRunner wfeRunner = wfeLoader
            //    .SetActionFactory(new ActionFactory())
            //    .SetEvaluatorFactory(new EvaluatorFactory())
            //    .SetModel<ShoppingCart>("model", new ShoppingCart() { ItemSKU = "1234", Payment = "goodpayment" })
            //    .Start();
            //IEngineFinalize f = await wfeRunner.ExecuteAsync("activity.main", new CancellationToken());
            //IEngineComplete c = await f.FinalizeAsync(new CancellationToken());

            //string process = JsonConvert.SerializeObject(c.Process, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });
            //Console.WriteLine(process);


            Console.ReadLine();

        }


    }
}
