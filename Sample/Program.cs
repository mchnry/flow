using Mchnry.Flow;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using System;
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
    public class CompletePurchaseAction : IAction<ShoppingCart>
    {
        public async Task<bool> CompleteAsync(IEngineScope<ShoppingCart> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            //the key name for getmodel is a user defined convention.  You can call it what you want, just be consistent.
            ShoppingCart z = scope.GetModel();
            string sku = z.ItemSKU;
            InventoryService svc = ServiceFactory.InventoryService;
            int remaining = await svc.DecrementInventory(sku);
            trace.TraceStep(string.Format("Completing Purchase of {0}.  {1} left in inventory", sku, remaining));
            return true;
        }
    }
    public class NotifyPurchasingAction : IAction<ShoppingCart>
    {
        public async Task<bool> CompleteAsync(IEngineScope<ShoppingCart> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            ShoppingCart model = scope.GetModel();
            string sku = model.ItemSKU;
            trace.TraceStep(string.Format("Notifying Purchasing that we're out of stock of {0}", sku));
            return true;
        }
    }
    #endregion

    #region Evaluators
    public class IsInInventoryEvaluator : IRuleEvaluator<ShoppingCart>
    {
        public async Task EvaluateAsync(IEngineScope<ShoppingCart> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            ShoppingCart model = scope.GetModel();
            //check inventory to see if item is still in stock
            string sku = model.ItemSKU;
            InventoryService svc = ServiceFactory.InventoryService;

            int inventoryCount = await svc.GetInventoryCount(sku);

            result.SetResult(inventoryCount > 0);


        }
    }
    public class IsPaymentAuthorizedEvaluator : IRuleEvaluator<ShoppingCart>
    {
        public async Task EvaluateAsync(IEngineScope<ShoppingCart> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {


            ShoppingCart model = scope.GetModel();
            //check if payment method is good
            PaymentService svc = ServiceFactory.PaymentService;

            bool isGood = await svc.IsAuthorizied(model.Payment);

            result.SetResult(isGood);
        }
    }

    #endregion

    #region Factories

    public class ActionFactory : Mchnry.Flow.Work.IActionFactory
    {
        public IAction<ShoppingCart> GetAction<ShoppingCart>(WorkDefine.ActionDefinition definition)
        {
            //note.. one would hopefully use di for this... but for this example, we'll brute force it

            switch (definition.Id)
            {
                case "action.completePurchase":
                    return (IAction<ShoppingCart>)new CompletePurchaseAction();

                case "action.notifyPurchasing":
                    return (IAction<ShoppingCart>)new NotifyPurchasingAction();
                default:
                    return null;
            }


        }
    }

    public class EvaluatorFactory : Mchnry.Flow.Logic.IRuleEvaluatorFactory
    {
        public IRuleEvaluator<ShoppingCart> GetRuleEvaluator<ShoppingCart>(LogicDefine.Evaluator definition)
        {

            //note.. one would hopefully use di for this... but for this example, we'll brute force it

            switch (definition.Id)
            {
                case "evaluator.isInInventory":
                    return (IRuleEvaluator<ShoppingCart>)new IsInInventoryEvaluator();

                case "evaluator.isPaymentAuthorized":
                    return (IRuleEvaluator<ShoppingCart>)new IsPaymentAuthorizedEvaluator();
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


        public static async Task Main(string[] args)
        {


            var builder = Builder.CreateBuilder();
            var created = builder.Build("ShoppingCart",
                activity => activity
                    .IfThenDo(
                        If => If.And(
                            First => First.True("isInInventory"),
                            Second => Second.True("paymentAccepted")
                            ),
                        Then => Then
                            .Do("ProcessPaymentMethod")
                            .Do("DecrementInventory")
                            .Do("ShipIt")
                    )
                    .Else(
                        
                        Then => Then
                            .IfThenDo(
                                If => If.True("!isInInventory"),
                                Thenb => Then.Do("NotifyShipping")
                            )

                    )
                );



            string s = JsonConvert.SerializeObject(created, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });
            Console.WriteLine(s);


            //lint
            IEngineLoader<ShoppingCart> workflowEngine = Engine<ShoppingCart>.CreateEngine(created);
            IEngineLinter<ShoppingCart> linter = workflowEngine.Lint();
            var result = await linter.LintAsync((a) =>
            {
                //a.Intent("someotherrule").HasContext("todo").HasValues(new System.Collections.Generic.List<Mchnry.Flow.Analysis.ContextItem>()
                //{
                //    new Mchnry.Flow.Analysis.ContextItem() { Key = "xyz", Literal = "dothis" }
                //});

            }, null, new CancellationToken());
            var sanitizedWorkflow = workflowEngine.Workflow;
            s = JsonConvert.SerializeObject(sanitizedWorkflow, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });
            Console.WriteLine(s);




            var articulated = result.ArticulateActivity("ShoppingCart");
            s = JsonConvert.SerializeObject(articulated, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
            Console.WriteLine(s);


            Console.ReadLine();

        }


    }
}
