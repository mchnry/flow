using System;
using Mchnry.Flow;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkDefine.Workflow sampleWorkflow = new WorkDefine.Workflow()
            {
                Evaluators = new System.Collections.Generic.List<LogicDefine.Evaluator>()
                {
                    new LogicDefine.Evaluator() { Id = ""}
                }
                Equations = new System.Collections.Generic.List<LogicDefine.Equation>()
                {
                    new LogicDefine.Equation() { }
                }
            };


        }
    }
}
