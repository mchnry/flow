using Mchnry.Flow.Analysis;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    public interface IEngineLoader<TModel>
    {

        IEngineLoader<TModel> OverrideValidation(ValidationOverride oride);
        IEngineLoader<TModel> PreemptValidation<T>(string key, string context, string comment, string auditCode) where T : IEvaluatorRule<TModel>;


        //IEngineLoader<TModel> LoadWorkflow(WorkDefine.Workflow workflow);


        //IEngineLoader<TModel> SetWorkflowDefinitionFactory(IWorkflowBuilderFactory factory);
        //IEngineLoader<TModel> SetEvaluatorFactory(IRuleEvaluatorFactory factory);
        //IEngineLoader<TModel> SetActionFactory(IActionFactory factory);

        //IEngineLoader<TModel> AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, IRuleResult, CancellationToken, Task> evaluator);
        //IEngineLoader<TModel> AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action);


        //Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token);
        IEngineLoader<TModel> SetGlobalModel<T>(string key, T model);
        //IEngineRunner<TModel> Start(string workflowId, TModel model);
        IEngineRunner<TModel> StartFluent(IWorkflowBuilder<TModel> builder, TModel model);

        //IEngineLinter<TModel> Lint(string workflowId);
        IEngineLinter<TModel> LintFluent(IWorkflowBuilder<TModel> builder);

        WorkDefine.Workflow Workflow { get; }
    }
}
