using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public interface IEngineLoader
    {
        IEngineLoader SetModel<T>(string key, T model);
        IEngineLoader OverrideValidations(ValidationContainer overrides);

        IEngineLoader SetEvaluatorFactory(IRuleEvaluatorFactory factory);
        IEngineLoader SetActionFactory(IActionFactory factory);

        IEngineRunner Start();
    }
}
