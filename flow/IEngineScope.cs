using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public interface IEngineScope
    {

        T GetModel<T>(string key);
        T SetModel<T>(string key, T value);
        T GetActivityModel<T>(string key);
        T SetActivityModel<T>(string key, T value);

        Logic.Define.Rule? CurrentRuleDefinition { get; }
        Work.Define.Activity CurrentActivity { get; }

        void AddValidation(Validation toAdd);

        void Defer(IAction action, bool onlyIfValidationsResolved);

        StepTraceNode<ActivityProcess> Process { get; }
    }
}
