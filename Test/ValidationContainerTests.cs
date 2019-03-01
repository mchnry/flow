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
            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("test");
            IValidationContainer scoped = toTest.Scope("abc");

            scoped.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));
            Assert.Contains(toTest.Validations, t => t.Key.Equals("test.abc.test"));

        }

        [Fact]
        public void ScopesDontCollide()
        {
            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            toTest.Scope("one").AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));
            toTest.ScopeToRoot().Scope("two").AddValidation(new Validation("test", ValidationSeverity.Confirm, "bye"));
            toTest.ScopeToRoot();

            Assert.Contains(toTest.Validations, t => t.Key.Equals("workflow.one.test"));
            Assert.Contains(toTest.Validations, t => t.Key.Equals("workflow.two.test"));

           
        }

        
        //resolve = true add validation, then override
        //if have a validation, then i override, resolve should return true
        [Fact]
        public void OverrideExistingValidation_ResolveTrue()
        {
            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            var c = toTest.Scope("one");
            c.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));

            c.AddOverride("test", "hello", "asdf");
            Assert.True(c.ResolveValidations());
            

            
        }

        //resolve = true add override, then validation
        //if i add an override, then add the validation, resolve should return true
        [Fact]
        public void OverrideValidationBeforeCreated_ResolveTrue()
        {


            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            var c = toTest.Scope("one");
            c.AddOverride("test", "test", "test");

            c.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));

            
            Assert.True(c.ResolveValidations());



        }


        //resolve = false 
        //if i add a validation, but not override, resolve should return false
        [Fact]
        public void ValidationWithNoOverrid_ResolveFalse()
        {
            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            var c = toTest.Scope("one");
            c.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));

           
            Assert.False(c.ResolveValidations());



        }

        //resolve = false even though one is confirmed
        //make sure resolve is false if only one validation is overridden
        [Fact]
        public void TwoValidationsOnlyOneOveride_ResolveFalse()
        {


            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            var c = toTest.Scope("one");
            c.AddOverride("test", "test", "test");



            c.AddValidation(new Validation("test", ValidationSeverity.Confirm, "hello"));
            c.AddValidation(new Validation("another", ValidationSeverity.Confirm, "asdfas"));

            Assert.False(c.ResolveValidations());



        }

        //make sure resolve is true if no validations

        //cannot confirm a fatal validation (exception)
        [Fact]
        public void ConfirmFatalValidationDoesNotPassResolve()
        {
            ValidationContainer toTest = ValidationContainer.CreateValidationContainer("workflow");
            var c = toTest.Scope("one");
           



            c.AddValidation(new Validation("test", ValidationSeverity.Fatal, "hello"));
            c.AddValidation(new Validation("another", ValidationSeverity.Confirm, "asdfas"));
            c.AddOverride("test", "asdf", "asdf");

            Assert.False(c.ResolveValidations());
        }
        //make sure confirmation of fatal validation throws exception



    }
}
