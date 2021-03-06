﻿namespace Mchnry.Flow
{
    public enum ActivityStatusOptions
    {
        
        Action_NotRun = 100,
        Action_Completed = 101,
        Action_Failed = 102,
        Action_Running = 103,
        Action_Reacting = 104,
        Action_Executing = 105,

        Activity_Running = 107,

        WorkflowEngine_Begin = 110,
        WorkflowEngine_Stop = 111,

        
        Rule_NotRun_ShortCircuit = 201,
        Rule_NotRun_Cached = 202,
        Rule_Evaluated = 203,
        Rule_Failed = 204,
        Rule_Evaluating = 205,
        Rule_Executing = 206,

        Expression_Evaluating = 207,

        RuleEngine_Begin = 210,
        RuleEngine_Stop = 211,

        Engine_Loading = 300,
        Engine_Begin = 301,
        Engine_Finalizing = 302
        
    }
}
