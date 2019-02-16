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
        IEngineLoader<TModel> SetModel(TModel model);
        IEngineLoader<TModel> OverrideValidation(ValidationOverride oride);

        IEngineLoader<TModel> SetEvaluatorFactory(IRuleEvaluatorFactory factory);
        IEngineLoader<TModel> SetActionFactory(IActionFactory factory);

        IEngineLoader<TModel> AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator);
        IEngineLoader<TModel> AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action);


             //Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token);

        IEngineRunner Start();
        Task<LintResult> LintAsync(Action<INeedIntent> addIntents, CancellationToken token);
        WorkDefine.Workflow Workflow { get; }
    }
}
