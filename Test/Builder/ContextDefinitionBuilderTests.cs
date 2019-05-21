using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Mchnry.Flow;

namespace Test.Builder
{
    public class ContextDefinitionBuilderTests
    {

        public enum testEnum
        {
            one = 1,
            two = 2,
            three = 3
        }

        [Fact]
        public void FromEnumTestNominal()
        {

            ContextDefinitionBuilder toTest = new ContextDefinitionBuilder();
            toTest.AnyOf("test", "test", typeof(testEnum), true);
            ContextDefinition ctx = toTest.definition;

            Assert.Equal(3, ctx.Items.Count);
            Assert.Contains("1", (from i in ctx.Items select i.Key));
            Assert.Contains("one", (from i in ctx.Items select i.Literal));
        }

    }
}
