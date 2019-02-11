using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public interface IEngineScope<TModel>
    {



        TModel GetModel();
        void SetModel(TModel value);
        T GetActivityModel<T>(string key);
        void SetActivityModel<T>(string key, T value);

        Logic.Define.Rule CurrentRuleDefinition { get; }
        Work.Define.Activity CurrentActivity { get; }

        void AddValidation(Validation toAdd);

        void Defer(IDeferredAction<TModel> action, bool onlyIfValidationsResolved);

        StepTraceNode<ActivityProcess> Process { get; }
    }
}
