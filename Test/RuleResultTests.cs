﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using Mchnry.Flow;

namespace Test
{
    public class RuleResultTests
    {
        //fail calls delegate with false
        [Fact]
        public void FailDelegatesFalse()
        {
            bool testResult = true;
            Action<bool, Validation> testDelegate = (b, v) =>
             {
                 testResult = b;
             };
            IRuleResult toTest = new RuleResult(testDelegate);
            toTest.Fail();

            Assert.False(testResult);
        }
        //pass calls delegate with status
        [Fact]
        public void PassDelegatesTrue()
        {
            bool? testResult = null;

            Action<bool, Validation> testDelegate = (b, v) =>
            {
                testResult = b;
            };
            IRuleResult toTest = new RuleResult(testDelegate);
            toTest.Pass();
            Assert.True(testResult);
        }



    }
}