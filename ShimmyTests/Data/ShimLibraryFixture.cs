using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shimmy.Data;
using Shimmy.Tests.SharedTestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy.Tests.Data
{
    [TestClass]
    public class ShimLibraryFixture
    {
        private Guid _currentReferenceGuid;
        private ShimmedMember _currentShimmedMethod;

        [TestInitialize]
        public void SetUp()
        {
            ShimLibrary.ClearRunningMethod();
            var methodInfo = typeof(SharedTestClasses.StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            _currentShimmedMethod = new ShimmedMethod<int>(methodInfo);
            _currentReferenceGuid = ShimLibrary.Add(_currentShimmedMethod);
        }

        [TestMethod]
        public void SetRunningMethod_Sets_Method_From_Guid()
        {
            // At first, a call result should except, as no method is running
            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
                Assert.Fail("Expected NullReferenceException - no method should be running.");
            }
            catch(NullReferenceException)
            {
                // do nothing
            }

            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            // Now, it should pass, because a method is running
            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
            }
            catch (Exception)
            {
                Assert.Fail("Method should now be running.");
            }
        }

        [TestMethod]
        public void AddCallResultToShim_Adds_Call_Result_With_Parameters()
        {
            Assert.IsFalse(_currentShimmedMethod.CallResults.Any());
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            ShimLibrary.AddCallResultToShim(new object[] { 5 });
            Assert.AreEqual(1, _currentShimmedMethod.CallResults.Count);
            Assert.AreEqual(5, _currentShimmedMethod.CallResults.First().Parameters[0]);
        }

        [TestMethod]
        public void ClearRunningMethod_Sets_Running_Method_Null()
        {
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
            }
            catch (Exception)
            {
                Assert.Fail("Method should now be running.");
            }

            ShimLibrary.ClearRunningMethod();

            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
                Assert.Fail("Expected NullReferenceException - no method should be running.");
            }
            catch (NullReferenceException)
            {
                // do nothing
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Execpts_On_No_Running_Method()
        {
            try
            {
                ShimLibrary.GetReturnValueAndClearRunningMethod<int>();
                Assert.Fail("Expected InvalidOperationException - no method should be running.");
            }
            catch(InvalidOperationException e)
            {
                Assert.AreEqual(ShimLibrary.CannotGetReturnValueNoMethodRunningError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Execpts_When_Running_Method_Has_Void_Return_Type()
        {
            // setup a void return type method to run
            ShimLibrary.ClearRunningMethod();
            var methodInfo = typeof(SharedTestClasses.StaticMethodsTestClass).GetMethod("EmptyMethod");
            _currentShimmedMethod = new ShimmedMethod(methodInfo);
            _currentReferenceGuid = ShimLibrary.Add(_currentShimmedMethod);
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimLibrary.GetReturnValueAndClearRunningMethod<int>();
                Assert.Fail("Expected InvalidOperationException - running method is of type void.");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ShimLibrary.CannotGetReturnValueNonMatchingTypeError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Execpts_When_Running_Method_Has_Wrong_Return_Type()
        {
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            try
            {
                ShimLibrary.GetReturnValueAndClearRunningMethod<string>();
                Assert.Fail("Expected InvalidOperationException - method type doesn't match generic type.");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ShimLibrary.CannotGetReturnValueNonMatchingTypeError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Default_Return_Value()
        {
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            var result = ShimLibrary.GetReturnValueAndClearRunningMethod<int>();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Custom_Return_Value()
        {
            _currentShimmedMethod.ReturnValue = 6;
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            var result = ShimLibrary.GetReturnValueAndClearRunningMethod<int>();
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Default_Value_for_Constructor()
        {
            ShimLibrary.ClearRunningMethod();
            var constructorInfo = typeof(InstanceMethodsTestClass).GetConstructor(Type.EmptyTypes);
            _currentShimmedMethod = new ShimmedConstructor<InstanceMethodsTestClass>(constructorInfo);
            _currentReferenceGuid = ShimLibrary.Add(_currentShimmedMethod);
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
           
            var result = ShimLibrary.GetReturnValueAndClearRunningMethod<InstanceMethodsTestClass>();
            Assert.IsNotNull(result); // would be null if there was no parameterless constructor
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Custom_Value_for_Constructor()
        {
            var a = new InstanceMethodsTestClass();

            ShimLibrary.ClearRunningMethod();
            var constructorInfo = typeof(InstanceMethodsTestClass).GetConstructor(Type.EmptyTypes);
            _currentShimmedMethod = new ShimmedConstructor<InstanceMethodsTestClass>(constructorInfo);
            _currentReferenceGuid = ShimLibrary.Add(_currentShimmedMethod);
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            _currentShimmedMethod.ReturnValue = a;

            var result = ShimLibrary.GetReturnValueAndClearRunningMethod<InstanceMethodsTestClass>();
            Assert.AreEqual(a.InstanceGuid, result.InstanceGuid);
            Assert.AreEqual(a, result);
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Clears_Running_Method()
        {
            ShimLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
            }
            catch (Exception)
            {
                Assert.Fail("Method should now be running.");
            }

            ShimLibrary.GetReturnValueAndClearRunningMethod<int>();

            try
            {
                ShimLibrary.AddCallResultToShim(new object[] { });
                Assert.Fail("Expected NullReferenceException - no method should be running.");
            }
            catch (NullReferenceException)
            {
                // do nothing
            }
        }
    }
}
