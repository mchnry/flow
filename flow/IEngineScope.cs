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
        void SetModel<T>(string key, T value);
        T GetActivityModel<T>(string key);
        void SetActivityModel<T>(string key, T value);

        Logic.Define.Rule CurrentRuleDefinition { get; }
        Work.Define.Activity CurrentActivity { get; }

        void AddValidation(Validation toAdd);

        void Defer(IDeferredAction action, bool onlyIfValidationsResolved);

        StepTraceNode<ActivityProcess> Process { get; }
    }
}
