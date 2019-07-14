using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shimmy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy.Tests.Data
{
    [TestClass]
    public class ShimmedMethodLibraryFixture
    {
        private Guid _currentReferenceGuid;
        private ShimmedMethod _currentShimmedMethod;

        [TestInitialize]
        public void SetUp()
        {
            ShimmedMethodLibrary.ClearRunningMethod();
            var methodInfo = typeof(SharedTestClasses.StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            _currentShimmedMethod = new ShimmedMethod<int>(methodInfo);
            _currentReferenceGuid = ShimmedMethodLibrary.Add(_currentShimmedMethod);
        }

        [TestMethod]
        public void SetRunningMethod_Sets_Method_From_Guid()
        {
            // At first, a call result should except, as no method is running
            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
                Assert.Fail("Expected NullReferenceException - no method should be running.");
            }
            catch(NullReferenceException)
            {
                // do nothing
            }

            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            // Now, it should pass, because a method is running
            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
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
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            ShimmedMethodLibrary.AddCallResultToShim(new object[] { 5 });
            Assert.AreEqual(1, _currentShimmedMethod.CallResults.Count);
            Assert.AreEqual(5, _currentShimmedMethod.CallResults.First().Parameters[0]);
        }

        [TestMethod]
        public void ClearRunningMethod_Sets_Running_Method_Null()
        {
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
            }
            catch (Exception)
            {
                Assert.Fail("Method should now be running.");
            }

            ShimmedMethodLibrary.ClearRunningMethod();

            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
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
                ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<int>();
                Assert.Fail("Expected InvalidOperationException - no method should be running.");
            }
            catch(InvalidOperationException e)
            {
                Assert.AreEqual(ShimmedMethodLibrary.CannotGetReturnValueNoMethodRunningError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Execpts_When_Running_Method_Has_Void_Return_Type()
        {
            // setup a void return type method to run
            ShimmedMethodLibrary.ClearRunningMethod();
            var methodInfo = typeof(SharedTestClasses.StaticMethodsTestClass).GetMethod("EmptyMethod");
            _currentShimmedMethod = new ShimmedMethod(methodInfo);
            _currentReferenceGuid = ShimmedMethodLibrary.Add(_currentShimmedMethod);
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<int>();
                Assert.Fail("Expected InvalidOperationException - running method is of type void.");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ShimmedMethodLibrary.CannotGetReturnValueNonMatchingTypeError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Execpts_When_Running_Method_Has_Wrong_Return_Type()
        {
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            try
            {
                ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<string>();
                Assert.Fail("Expected InvalidOperationException - method type doesn't match generic type.");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(ShimmedMethodLibrary.CannotGetReturnValueNonMatchingTypeError, e.Message);
            }
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Default_Return_Value()
        {
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            var result = ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<int>();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Returns_Correct_Custom_Return_Value()
        {
            _currentShimmedMethod.SetReturnValue(6);
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());
            var result = ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<int>();
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void GetReturnValueAndClearRunningMethod_Clears_Running_Method()
        {
            ShimmedMethodLibrary.SetRunningMethod(_currentReferenceGuid.ToString());

            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
            }
            catch (Exception)
            {
                Assert.Fail("Method should now be running.");
            }

            ShimmedMethodLibrary.GetReturnValueAndClearRunningMethod<int>();

            try
            {
                ShimmedMethodLibrary.AddCallResultToShim(new object[] { });
                Assert.Fail("Expected NullReferenceException - no method should be running.");
            }
            catch (NullReferenceException)
            {
                // do nothing
            }
        }
    }
}
