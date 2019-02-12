﻿using Mchnry.Flow.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Mchnry.Flow
{
    public class RunManager
    {
        //store known evaluator results to avoid rerunning already run evaluators
        private Dictionary<string, bool?> results = new Dictionary<string, bool?>();
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
    }
}