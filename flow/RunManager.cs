using Mchnry.Flow.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    internal class RunManager
    {
        //store known evaluator results to avoid rerunning already run evaluators
        private Dictionary<string, bool?> results = new Dictionary<string, bool?>();
        internal virtual LogicDefine.Rule CurrentRuleDefinition { get; set; } = null;
        
        internal virtual WorkDefine.ActionRef CurrentAction { get; set; }
        internal virtual WorkDefine.Activity CurrentActivity { get; set; }
        //current status of running engine
        internal virtual EngineStatusOptions EngineStatus { get; set; } = EngineStatusOptions.NotStarted;
        internal Configuration.Convention config;
        
        internal Dictionary<string, int> deferrals = new Dictionary<string, int>();

        internal int Ordinal { get; set; } = 0;

        internal RunManager(Configuration.Convention config, string workflowId)
        {
            this.config = config;
            this.WorkflowId = workflowId;
        }

        internal string WorkflowId { get; set; }
        

        internal virtual bool? GetResult(LogicDefine.Rule rule)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = rule.Id, Context = rule.Context };
            if (results.ContainsKey(key.ToString()))
            {
                return results[key.ToString()];
            }
            else
            {
                return null;
            }
        }

        internal virtual void SetResult(LogicDefine.Rule rule, bool result)
        {
            EvaluatorKey key = new EvaluatorKey() { Context = rule.Context, Id = rule.Id };
            if (this.results.ContainsKey(key.ToString()))
            {
                this.results.Remove(key.ToString());

            }
            this.results.Add(key.ToString(), result);

        }

        internal virtual void Reset()
        {
            this.EngineStatus = EngineStatusOptions.NotStarted;
            this.CurrentAction = null;
            this.CurrentRuleDefinition = null;
            this.CurrentActivity = null;
            this.results = new Dictionary<string, bool?>();
        }

        internal string GetDeferralId()
        {
            string key = this.CurrentAction.Id;
            int cnt = 1;
            if (this.deferrals.ContainsKey(key))
            {
                cnt = this.deferrals[key];
                cnt++;
                this.deferrals[key] = cnt;
            } else
            {
                this.deferrals.Add(key, cnt);
            }

            return string.Format("{0}{1}{2}{3}{4}", key, this.config.Delimeter, this.WorkflowId, this.config.Delimeter, cnt);

        }
    }
}
