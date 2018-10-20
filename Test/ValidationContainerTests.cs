namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Mchnry.Flow;
    using Moq;
    using Xunit;

    public class ValidationContainerTests
    {

        [Fact]
        public void RootSharesWithContainer()
        {
            //there's not really anything i can do with a container..
            //i need the scope
            ValidationContainer toTest = new ValidationContainer();
            IValidationContainer scoped = toTest.Scope("abc");

            scoped.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));
            Assert.Contains(toTest.Validations, t => t.Key.Equals("abc.test"));

        }

        //scopes don't collide (same sub key)
        //resolve = true add validation, then override
        //resolve = true add override, then validation
        //resolve = false 
        //resolve = false even though one is confirmed
        //cannot confirm a fatal validation (exception)

    }
}
