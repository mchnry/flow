﻿using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{

    public enum CacheScopeOptions
    {
        /// <summary>
        /// items in global scope are available to all workflows/actions/evaluators
        /// </summary>
        Global,
        /// <summary>
        /// items in activity scope are only available to actions/evaluators within the current running activity
        /// </summary>
        Activity,
        /// <summary>
        /// items in workflow scope are available to all actions/evaluators in the workflow
        /// </summary>
        Workflow
    }

    public interface IEngineScopeDefer<TModel>
    {

        TModel GetModel();
        void SetModel(TModel value);
        T GetModel<T>(CacheScopeOptions scope, string key);
        DateTime TimeStamp { get; }
        void SetModel<T>(CacheScopeOptions scope, string key, T value);
        StepTraceNode<ActivityProcess> Process { get; }

    }

    public interface IEngineScope<TModel>
    {



        TModel GetModel();
        void SetModel(TModel value);
        T GetModel<T>(CacheScopeOptions scope, string key);
        void SetModel<T>(CacheScopeOptions scope, string key, T value);
        Task<T> GetModelAsync<T>(CacheScopeOptions scope, string key, Func<Task<T>> Get, bool cacheBeforeReturn);

        Logic.Define.Rule CurrentRuleDefinition { get; }
        Work.Define.ActionRef CurrentAction { get; }
        Work.Define.Activity CurrentActivity { get; }

        DateTime TimeStamp { get; }

        IValidationContainer MyValidations { get; }
        void Defer(IDeferredAction<TModel> action, bool onlyIfValidationsResolved);

        StepTraceNode<ActivityProcess> Process { get; }

        //Task RunWorkflowAsync<T>(string workflowId, T model, CancellationToken token);
        Task RunWorkflowAsync<T>(IWorkflowBuilder<T> builder, T model, CancellationToken token);
    }
}
