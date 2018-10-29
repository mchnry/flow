using Mchnry.Flow.Logic;
using Mchnry.Flow.Test;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public interface IEngineLoader
    {
        IEngineLoader SetModel<T>(string key, T model);
        IEngineLoader OverrideValidation(ValidationOverride oride);

        IEngineLoader SetEvaluatorFactory(IRuleEvaluatorFactory factory);
        IEngineLoader SetActionFactory(IActionFactory factory);

        IEngineRunner Start();
        List<LogicTest> Lint(Action<Linter> addIntents);
    }
}
