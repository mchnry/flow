﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Analysis;
using Mchnry.Flow.Cache;
using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using Newtonsoft.Json;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    public class Engine<TModel> :
        IEngineLoader<TModel>, IEngineRunner<TModel>, IEngineScope<TModel>, IEngineFinalize<TModel>, IEngineComplete<TModel>, IEngineLinter<TModel>, IEngineScopeDefer<TModel>
    {

        internal Config Configuration;
        internal bool Sanitized = false;

        private List<ValidationOverride> preOverrides = new List<ValidationOverride>();

        private StepTracer<LintTrace> lintTracer = default(StepTracer<LintTrace>);
        internal virtual EngineStepTracer Tracer { get; set; }

        internal virtual IImplementationManager<TModel> ImplementationManager { get; set; }
        internal virtual WorkflowManager WorkflowManager { get; set; }
        internal virtual RunManager RunManager { get; set; }

        

        //store reference to all actions to run during finalize (if all validations succeed)
        private Dictionary<string, IDeferredAction<TModel>> finalize = new Dictionary<string, IDeferredAction<TModel>>();
        //store reference to all actions to run during finalize always
        private Dictionary<string, IDeferredAction<TModel>> finalizeAlways = new Dictionary<string, IDeferredAction<TModel>>(); //actions to complete at end regardless of validation state

        private ICacheManager GlobalCache;
        private ICacheManager WorkflowCache;


        /// <summary>
        /// engine construcor
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// internal so that caller follows fluent construction 
        /// starting with <see cref="CreateEngine(WorkDefine.Workflow)"/>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="workFlow">workflow definition</param>
        internal Engine(Configuration.Config config)
        {

            this.Configuration = config;
            this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));
            this.ImplementationManager = new ImplementationManager<TModel>(this.Configuration);
            
          


            this.GlobalCache = config.Cache;

            this.Sanitized = false;
        }
        internal Engine(Config config, RunManager runManager) : this(config)
        {
            this.RunManager = runManager;
        }

        
        public static IEngineLoader<TModel> CreateEngine()
        {

            
            return new Engine<TModel>(new Config());


        }
        public static IEngineLoader<TModel> CreateEngine(Action<Config> Configure)
        {
        
            Config config = new Config();
            Configure?.Invoke(config);
            Engine<TModel> toReturn = new Engine<TModel>(config);


            

            

            return toReturn;

        }



        internal virtual ActivityStatusOptions CurrentActivityStatus { get; set; } = ActivityStatusOptions.Engine_Loading;


        LogicDefine.Rule IEngineScope<TModel>.CurrentRuleDefinition => RunManager.CurrentRuleDefinition;

        WorkDefine.ActionRef IEngineScope<TModel>.CurrentAction => RunManager.CurrentAction;

        WorkDefine.Activity IEngineScope<TModel>.CurrentActivity => RunManager.CurrentActivity;


        StepTraceNode<ActivityProcess> IEngineScopeDefer<TModel>.Process => this.Tracer.Root;
        StepTraceNode<ActivityProcess> IEngineScope<TModel>.Process => this.Tracer.Root;

        StepTraceNode<ActivityProcess> IEngineComplete<TModel>.Process => this.Tracer.Root;
        StepTraceNode<ActivityProcess> IEngineFinalize<TModel>.Process => this.Tracer.Root;

        EngineStatusOptions IEngineComplete<TModel>.Status => RunManager.EngineStatus;
        EngineStatusOptions IEngineFinalize<TModel>.Status => RunManager.EngineStatus;

        //store all validations created by validators/evaluators
        internal virtual ValidationContainer ValidationContainer { get; set; } 
        IValidationContainer IEngineComplete<TModel>.Validations => this.ValidationContainer.ScopeToRoot();
        IValidationContainer IEngineFinalize<TModel>.Validations => this.ValidationContainer.ScopeToRoot();


        IValidationContainer IEngineScope<TModel>.MyValidations {  get {

                if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
                {
                    return this.ValidationContainer.ScopeToRoot().Scope(RunManager.CurrentActivity.Id);
                }
                else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
                {
                    string scope = RunManager.CurrentRuleDefinition.Id;
                    if (null != RunManager.CurrentRuleDefinition.Context)
                    {
                        scope = string.Format("{0}.{1}", scope, RunManager.CurrentRuleDefinition.Context.GetHashCode().ToString());
                    }
                    return this.ValidationContainer.ScopeToRoot().Scope(scope);
                } else
                {
                    return this.ValidationContainer.ScopeToRoot();
                }
                

            }
        }

        internal void AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
            {
                string scope = RunManager.CurrentActivity.Id;
                this.ValidationContainer.ScopeToRoot().Scope(scope).AddValidation(toAdd);


            }
            else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
            {
                string scope = RunManager.CurrentRuleDefinition.Id;
                if (null != RunManager.CurrentRuleDefinition.Context)
                {
                    scope = string.Format("{0}.{1}", scope, RunManager.CurrentRuleDefinition.Context.GetHashCode().ToString());
                }
                this.ValidationContainer.ScopeToRoot().Scope(scope).AddValidation(toAdd);

            }
            else
            {
                ((IValidationContainer)this.ValidationContainer.ScopeToRoot()).AddValidation(toAdd);
            }

        }

        //defer is called during the execution of an IAction
        void IEngineScope<TModel>.Defer(IDeferredAction<TModel> action, bool onlyIfValidationsResolved)
        {
            string key = RunManager.GetDeferralId();
            if (onlyIfValidationsResolved)
            {
                this.finalize.Add(key,action);
            }
            else
            {
                this.finalizeAlways.Add(key,action);
            }
        }

        async Task<IEngineFinalize<TModel>> IEngineRunner<TModel>.ExecuteAsync(CancellationToken token)
        {
            if (!this.Sanitized)
            {
                this.Sanitize();
            }

            string workflowId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, this.WorkflowManager.WorkFlow.Id, this.Configuration.Convention);
            workflowId = workflowId + this.Configuration.Convention.Delimeter + "Main";

            return await this.ExecuteAsync(workflowId, token);

        }

        internal async Task<Engine<TModel>> ExecuteAsync(string activityId, CancellationToken token)
        {


            this.GlobalCache.Insert("global_timestamp", DateTime.UtcNow);

            Activity<TModel> toLoad = this.LoadActivity(activityId);

            this.Tracer.CurrentStep = this.Tracer.TraceStep(this.Tracer.Root, new ActivityProcess("Execute", ActivityStatusOptions.Engine_Begin, null));
            await(toLoad.Execute(this.Tracer, token));

            return this;

        }

        async Task<IEngineComplete<TModel>> IEngineRunner<TModel>.ExecuteAutoFinalizeAsync( CancellationToken token)
        {

            IEngineFinalize<TModel> finalizer = await ((IEngineRunner<TModel>)this).ExecuteAsync( token);
            return await finalizer.FinalizeAsync(token);

        }

        async Task<IEngineComplete<TModel>> IEngineFinalize<TModel>.FinalizeAsync(CancellationToken token)
        {
            StepTraceNode<ActivityProcess> mark = this.Tracer.CurrentStep = this.Tracer.TraceStep(this.Tracer.Root, new ActivityProcess("Finalize", ActivityStatusOptions.Engine_Finalizing, null));
            if (this.ValidationContainer.ResolveValidations())
            {


                foreach (KeyValuePair<string, IDeferredAction<TModel>> toFinalize in this.finalize)
                {
                    this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Key, ActivityStatusOptions.Action_Running, null));

                    await toFinalize.Value.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);

                }


            }

            foreach (KeyValuePair<string, IDeferredAction<TModel>> toFinalize in this.finalizeAlways)
            {
                this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Key, ActivityStatusOptions.Action_Running, null));

                await toFinalize.Value.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);
            }

            return this;
        }

        T IEngineScopeDefer<TModel>.GetModel<T>(CacheScopeOptions scope, string key) { return ((IEngineScope<TModel>)this).GetModel<T>(scope, key); }
        T IEngineScope<TModel>.GetModel<T>(CacheScopeOptions scope, string key)
        {
            T toReturn = default(T);
            switch(scope)
            {
                case CacheScopeOptions.Activity:
                    toReturn = this.WorkflowCache.Spawn(RunManager.CurrentActivity.Id).Read<T>(key);
                    break;
                case CacheScopeOptions.Global:
                    toReturn = this.GlobalCache.Read<T>(key);
                    break;
                case CacheScopeOptions.Workflow:
                    toReturn = this.WorkflowCache.Read<T>(key);
                    break;
                
            }
            
            return toReturn;
        }

        async Task<T> IEngineScope<TModel>.GetModelAsync<T>(CacheScopeOptions scope, string key, Func<Task<T>> Get, bool cacheBeforeReturn)
        {

            ICacheManager cache = default;
            switch(scope)
            {
                case CacheScopeOptions.Activity:
                    cache = this.WorkflowCache.Spawn(RunManager.CurrentActivity.Id); break;
                case CacheScopeOptions.Global:
                    cache = this.GlobalCache; break;
                case CacheScopeOptions.Workflow:
                    cache = this.WorkflowCache; break;

            }

            T toReturn = default(T);
            toReturn = await cache.ReadAsync(key, Get, cacheBeforeReturn);

            return toReturn;

        }


        TModel IEngineScopeDefer<TModel>.GetModel() { return ((IEngineScope<TModel>)this).GetModel(); }
        TModel IEngineScope<TModel>.GetModel()
        {
            return this.WorkflowCache.Read<TModel>("workflowmodel");
        }

        TModel IEngineComplete<TModel>.GetModel(string key)
        {
            return this.WorkflowCache.Read<TModel>(key);
        }
        TModel IEngineFinalize<TModel>.GetModel(string key)
        {
            return this.WorkflowCache.Read<TModel>(key);
        }

        TModel IEngineComplete<TModel>.GetModel()
        {
            return ((IEngineScope<TModel>)this).GetModel(); 
        }
        TModel IEngineFinalize<TModel>.GetModel()
        {
            return ((IEngineScope<TModel>)this).GetModel();
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.OverrideValidation(ValidationOverride oride)
        {

            this.preOverrides.Add(oride);
            return this;
        }
        IEngineLoader<TModel> IEngineLoader<TModel>.PreemptValidation<T>(string key, string context, string comment, string auditCode) 
        {
            string typeOfT = typeof(T).Name;
            LogicDefine.Rule rule = new LogicDefine.Rule() { Id = typeOfT, Context = context };

            string normalized = ConventionHelper.ApplyConvention(NamePrefixOptions.Evaluator, rule.RuleIdWithContext, this.Configuration.Convention) + ".";
            key = key.ToLower().Replace(normalized.ToLower(), "");

            string orideKey = ConventionHelper.ApplyConvention(NamePrefixOptions.Evaluator, rule.RuleIdWithContext + "." + key, this.Configuration.Convention);
            ValidationOverride oride = new ValidationOverride(orideKey.ToLower(), comment, auditCode);
            this.preOverrides.Add(oride);
            return this;
        }


        void IEngineScopeDefer<TModel>.SetModel<T>(CacheScopeOptions scope, string key, T value) { ((IEngineScope<TModel>)this).SetModel<T>(scope, key, value); }
        void IEngineScope<TModel>.SetModel<T>(CacheScopeOptions scope, string key, T value)
        {
            switch (scope)
            {
                case CacheScopeOptions.Workflow:
                    this.WorkflowCache.Insert<T>(key, value);
                    break;
                case CacheScopeOptions.Activity:
                    this.WorkflowCache.Spawn(RunManager.CurrentActivity.Id).Insert<T>(key, value);
                    break;
                case CacheScopeOptions.Global:
                    this.GlobalCache.Insert<T>(key, value);
                    break;
            }

  
        }


        void IEngineScopeDefer<TModel>.SetModel(TModel value) { ((IEngineScope<TModel>)this).SetModel(value); }
        void IEngineScope<TModel>.SetModel(TModel value)
        {
            this.WorkflowCache.Insert<TModel>("workflowmodel", value);



        }



        private void LoadWorkflow(IWorkflowBuilder<TModel> builder)
        {
            WorkDefine.Workflow wf = null;
            if (this.WorkflowManager != null)
            {
                wf = this.WorkflowManager.WorkFlow;
            }
            else
            {
                wf = this.ImplementationManager.GetWorkflow(builder);
                this.WorkflowManager = new WorkflowManager(wf, this.Configuration);

            }

            if (RunManager == null)
            {
                RunManager = new RunManager(this.Configuration.Convention, wf.Id);
            } else
            {
                this.RunManager.WorkflowId = wf.Id;
            }

            this.ValidationContainer = ValidationContainer.CreateValidationContainer();

            if (this.preOverrides.Count() > 0)
            {
                this.preOverrides.ForEach(o =>
                {
                    ((IValidationContainer)this.ValidationContainer).AddOverride(o.Key, o.Comment, o.AuditCode);

                });
            }

            this.WorkflowCache = this.Configuration.Cache.Spawn(wf.Id);
        }

        IEngineRunner<TModel> IEngineLoader<TModel>.StartFluent(IWorkflowBuilder<TModel> builder, TModel model)
        {
            LoadWorkflow(builder);
            ((IEngineScope<TModel>)this).SetModel(model);

            return this;
        }







        internal async Task<bool> Evaluate(string equationId, CancellationToken token)
        {
            IRule<TModel> toEval = this.LoadLogic(equationId);

            bool result = await toEval.EvaluateAsync(false, token);

            return result;


        }




        private Activity<TModel> LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.WorkflowManager.GetActivity(activityId);



            Activity<TModel> toReturn = new Activity<TModel>(this, definition);

            Action<Activity<TModel>, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {


                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.ActionRef work = r.Work;
                        WorkDefine.Activity toCreatedef = this.WorkflowManager.GetActivity(work.Id);
                        LoadLogic(r.Logic);

                        if (null == a.Reactions)
                        {
                            a.Reactions = new List<Reaction<TModel>>();
                        }

                        if (toCreatedef != null) { 
                            Activity<TModel> toCreate = new Activity<TModel>(this, toCreatedef);
                            LoadReactions(toCreate, toCreatedef);
                            a.Reactions.Add(new Reaction<TModel>(r.Logic, toCreate));                

                        } else
                        {
                            
                            a.Reactions.Add(new Reaction<TModel>(r.Logic, r.Work));
                        }
                        





                    });
                }

            };

            LoadReactions(toReturn, definition);

            return toReturn;

        }

        private IRule<TModel> LoadLogic(string equationId)
        {
            StepTracer<LintTrace> trace = new StepTracer<LintTrace>();
            StepTraceNode<LintTrace> root = trace.TraceFirst(new LintTrace(LintStatusOptions.Loading, "Loading Logic", equationId));


            //Lint.... make sure we have everything we need first.
            Func<LogicDefine.Rule, StepTraceNode<LintTrace>, bool, IRule<TModel>> LoadRule = null;
            LoadRule = (rule, parentStep, inner) =>
            {
                StepTraceNode<LintTrace> step = trace.TraceNext(parentStep, new LintTrace(LintStatusOptions.Inspecting, "Inspecting Rule", rule.Id));
                IRule<TModel> toReturn = null;
                //if id is an equation, we are creating an expression

                //since we've formalized convention, we can just check that
                if (ConventionHelper.MatchesConvention(NamePrefixOptions.Equation, rule.Id, this.Configuration.Convention))
                {
                    LogicDefine.Equation eq = this.WorkflowManager.GetEquation(rule.Id);

                    IRule<TModel> first = LoadRule(eq.First, step, true);
                    IRule<TModel> second = LoadRule(eq.Second, step, true);
                    toReturn = new Expression<TModel>(rule, eq.Condition, first, second, this, inner);
                }
                else
                {
                    LogicDefine.Evaluator ev = this.WorkflowManager.GetEvaluator(rule.Id);
                    toReturn = new Rule<TModel>(rule, this, inner);
                }




                return toReturn;
            };




            LogicDefine.Rule eqRule = equationId;
            IRule<TModel> loaded = LoadRule(eqRule, root, false);

            return loaded;

        }

        IEngineLinter<TModel> IEngineLoader<TModel>.LintFluent(IWorkflowBuilder<TModel> builder)
        {
            LoadWorkflow(builder);
            return this;
        }


        public async Task<LintInspector> LintAsync(Action<Case> mockCase, CancellationToken token)
        {
            if (!this.Sanitized)
            {
                this.Sanitize();
            }


            Linter linter = new Linter(this.WorkflowManager, this.Configuration);
 

            List<ActivityTest> activityTests = linter.AcvityLint();
            List<ActivityTest> mockTests = null;

            //temporarily supplant implementationmanager with fake
            IImplementationManager<TModel> holdIM = this.ImplementationManager;

            //loop through each activity in activityTests and run them
            foreach (ActivityTest at in activityTests)
            {
                foreach (Case tc in at.TestCases)
                {
                    string activityId = ConventionHelper.EnsureConvention(NamePrefixOptions.Activity, at.ActivityId, this.Configuration.Convention);
                    this.ImplementationManager = new FakeImplementationManager<TModel>(tc, this.WorkflowManager.WorkFlow, this.Configuration);
                    await this.ExecuteAsync(activityId, token);

                    tc.Trace = this.Tracer.tracer.AllNodes;
                    this.Reset(true);

                }
                
            }

            //restore original implementation manager
            this.ImplementationManager = holdIM;

            //if the caller provided a mock callback, then we'll
            //run through it through again, but this time using the
            //existing implementation manager. 
            if (mockCase != null)
            {
                mockTests = (from t in activityTests
                 select new ActivityTest(t.ActivityId)
                 {
                     TestCases = (from z in t.TestCases select (Case)z.Clone()).ToList()
                 }).ToList();

                foreach (ActivityTest at in mockTests)
                {
                    foreach (Case tc in at.TestCases)
                    {
                        mockCase(tc);
                        await this.ExecuteAsync(at.ActivityId, token);

                        tc.Trace = this.Tracer.tracer.AllNodes;
                        this.Reset(true);

                    }

                }

            }

            CaseAnalyzer analyzer = new CaseAnalyzer(this.WorkflowManager, activityTests, mockTests, this.Configuration);
            List<Audit> auditResults = analyzer.Analyze();




            int lintHash = this.WorkflowManager.WorkFlow.GetHashCode();
            return new LintInspector(new LintResult(this.lintTracer, activityTests, null, auditResults, lintHash.ToString()), this.Workflow, this.Configuration);
            //return new LintResult(this.lintTracer, activityTests, null, auditResults, lintHash.ToString());
        }

        public WorkDefine.Workflow Workflow { get => this.WorkflowManager.WorkFlow; }

        WorkDefine.Workflow IEngineLoader<TModel>.Workflow => this.WorkflowManager.WorkFlow;

        public DateTime TimeStamp
        {
            get
            {
                DateTime stamp = this.GlobalCache.Read<DateTime>("global_timestamp", () => DateTime.UtcNow, true);
                return stamp;
                

            }
        }

        internal void Sanitize()
        {
            StepTracer<LintTrace> lintTrace = new StepTracer<LintTrace>();
            lintTrace.TraceFirst(new LintTrace(LintStatusOptions.Linting, "Starting Lint"));

            //before sanitize
            //follows convention

            //after sanitize
            //any unused evaluators or actions

            //identify main activities

            //get test cases
            //look for irrelevant evaluators (i.e., doesnt matter in any case if true or false)

            Sanitizer sanitizer = new Sanitizer(lintTrace, this.Configuration);
            WorkDefine.Workflow sanitized = sanitizer.Sanitize(this.WorkflowManager.WorkFlow);

            string s = JsonConvert.SerializeObject(sanitized, new JsonSerializerSettings() { Formatting = Formatting.Indented });

            this.WorkflowManager.WorkFlow = sanitized;
            this.lintTracer = lintTrace;
            this.Sanitized = true;
        }

        internal void Reset(bool hard)
        {
            if (hard)
            {
    
                RunManager.Reset();
                this.CurrentActivityStatus = ActivityStatusOptions.Engine_Loading;
                this.finalize = new Dictionary<string, IDeferredAction<TModel>>();
                this.finalizeAlways = new Dictionary<string, IDeferredAction<TModel>>();
                this.Configuration.Ordinal = 0;
                this.Configuration.Cache.Flush();
                this.Tracer = this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));
            }
        }


        async Task IEngineScope<TModel>.RunWorkflowAsync<T>(IWorkflowBuilder<T> builder, T model, CancellationToken token)
        {
            string currentWorkflowId = this.RunManager.WorkflowId;

            this.RunManager.Ordinal++;
            IEngineLoader<T> subEngine = new Engine<T>(this.Configuration, this.RunManager);

            //subEngine
            //    .SetActionFactory(this.ImplementationManager.ActionFactory.proxy)
            //    .SetEvaluatorFactory(this.ImplementationManager.EvaluatorFactory.proxy)
            //    .SetWorkflowDefinitionFactory(this.ImplementationManager.BuilderFactory.Proxy);

            //this is only called by the chainactivity, which always uses the same model as the originating engine.
            //so we can safely assume T == TModel
            Engine<T> asEngine = (Engine<T>)subEngine;
            

            foreach (var v in this.ValidationContainer.Overrides)
            {
                subEngine.OverrideValidation(v);
            }

            var runner = subEngine.StartFluent(builder, model);
            var finalizer = await runner.ExecuteAsync(token);




            //append all finalize to mine
            foreach (var f in asEngine.finalize)
            {
                this.finalize.Add(f.Key, new DeferProxy<TModel, T>(f.Value, asEngine));
            }
            foreach (var f in asEngine.finalizeAlways)
            {
                this.finalize.Add(f.Key, new DeferProxy<TModel, T>(f.Value, asEngine));
            }
            //append validations to mine
            foreach (var v in asEngine.ValidationContainer.Validations)
            {
                this.AddValidation(v);
            }

            this.RunManager.WorkflowId = currentWorkflowId;

        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetGlobalModel<T>(string key, T model)
        {
            ((IEngineScope<TModel>)this).SetModel(CacheScopeOptions.Global, key, model);
            return this;
        }


    }



}
