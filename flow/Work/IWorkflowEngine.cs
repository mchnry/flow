using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    public interface IWorkflowEngine
    {

        StepTraceNode<ActivityProcess> Process { get; }
        StepTraceNode<ActivityProcess> CurrentProcess { get; }

        IActionFactory ActionFactory { get; }


        void SetStateObject<T>(ActivityProcess currentProcess, string key, T toSave);
        void SetStateObject<T>(string key, T toSave);
        T GetStateObject<T>(ActivityProcess currentProcess, string key);
        T GetStateObject<T>(string key);

        void Defer(IAction action, bool onlyIfValidationsResolved = true);
        //void Inject(Define.Activity activityDefinition, object model);

        
    }
}