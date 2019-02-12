using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;
using Mchnry.Flow;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Diagnostics;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Test.Logic
{
    public class RuleTests
    {
        

        [Fact]
        public async void DoesNotReevaluate()
        {
            AlwaysTrueEvaluator<string> trueEvaluator = new AlwaysTrueEvaluator<string>();
            Mock<EngineStepTracer> mkTracer = new Mock<EngineStepTracer>(new ActivityProcess());
            
            Mock<RunManager> mkRunMgr = new Mock<RunManager>();
            Mock<ImplementationManager<string>> mkImplMgr = new Mock<ImplementationManager<string>>();
            mkRunMgr.Setup(g => g.GetResult(It.IsAny<LogicDefine.Rule>())).Returns(true);
            mkImplMgr.Setup(g => g.GetEvaluator(It.IsAny<string>())).Returns(trueEvaluator);
            Mock<Engine<string>> mkEngine = new Mock<Engine<string>>(new WorkDefine.Workflow());
            mkEngine.Setup(g => g.Tracer).Returns(mkTracer.Object);
            mkEngine.Setup(g => g.ImplementationManager).Returns(mkImplMgr.Object);
            mkEngine.Setup(g => g.RunManager).Returns(mkRunMgr.Object);
            Rule<string> toTest = new Rule<string>("test", mkEngine.Object);

            await (toTest.EvaluateAsync(false, new System.Threading.CancellationToken()));

            mkRunMgr.Verify(g => g.SetResult(It.IsAny<LogicDefine.Rule>(), It.IsAny<bool>()), Times.Never);


            


        }

        [Fact]
        public async void EvaluatesFirstTime()
        {
            AlwaysTrueEvaluator<string> trueEvaluator = new AlwaysTrueEvaluator<string>();
            Mock<EngineStepTracer> mkTracer = new Mock<EngineStepTracer>(new ActivityProcess());

            Mock<RunManager> mkRunMgr = new Mock<RunManager>();
            Mock<ImplementationManager<string>> mkImplMgr = new Mock<ImplementationManager<string>>();
            bool? nullBool = null;
            mkRunMgr.Setup(g => g.GetResult(It.IsAny<LogicDefine.Rule>())).Returns(nullBool);
            mkImplMgr.Setup(g => g.GetEvaluator(It.IsAny<string>())).Returns(trueEvaluator);
            Mock<Engine<string>> mkEngine = new Mock<Engine<string>>(new WorkDefine.Workflow());
            mkEngine.Setup(g => g.Tracer).Returns(mkTracer.Object);
            mkEngine.Setup(g => g.ImplementationManager).Returns(mkImplMgr.Object);
            mkEngine.Setup(g => g.RunManager).Returns(mkRunMgr.Object);
            Rule<string> toTest = new Rule<string>("test", mkEngine.Object);

            await (toTest.EvaluateAsync(false, new System.Threading.CancellationToken()));

            mkRunMgr.Verify(g => g.SetResult(It.IsAny<LogicDefine.Rule>(), It.IsAny<bool>()), Times.Once);





        }

        [Fact]
        public async void DoesReevaluateWhenForced()
        {
            AlwaysTrueEvaluator<string> trueEvaluator = new AlwaysTrueEvaluator<string>();
            Mock<EngineStepTracer> mkTracer = new Mock<EngineStepTracer>(new ActivityProcess());

            Mock<RunManager> mkRunMgr = new Mock<RunManager>();
            Mock<ImplementationManager<string>> mkImplMgr = new Mock<ImplementationManager<string>>();
            
            //indicates that it was evaluated previously
            mkRunMgr.Setup(g => g.GetResult(It.IsAny<LogicDefine.Rule>())).Returns(true);
            mkImplMgr.Setup(g => g.GetEvaluator(It.IsAny<string>())).Returns(trueEvaluator);
            Mock<Engine<string>> mkEngine = new Mock<Engine<string>>(new WorkDefine.Workflow());
            mkEngine.Setup(g => g.Tracer).Returns(mkTracer.Object);
            mkEngine.Setup(g => g.ImplementationManager).Returns(mkImplMgr.Object);
            mkEngine.Setup(g => g.RunManager).Returns(mkRunMgr.Object);
            Rule<string> toTest = new Rule<string>("test", mkEngine.Object);

            //true to reevaluate
            await (toTest.EvaluateAsync(true, new System.Threading.CancellationToken()));

            mkRunMgr.Verify(g => g.SetResult(It.IsAny<LogicDefine.Rule>(), It.IsAny<bool>()), Times.Once);





        }
    }
}
