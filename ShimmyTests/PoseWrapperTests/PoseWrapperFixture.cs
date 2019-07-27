using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using Shimmy.Tests.SharedTestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy.Tests.PoseWrapperTests
{
    /*
     * Contains tests for non-method-type-specific behavior
     * of PoseWrapper.
     */
    [TestClass]
    public class PoseWrapperFixture
    {
        private class TestStaticMethodSameNameOne
        {
            public static int MethodWithSameName()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }
        }

        private class TestStaticMethodSameNameTwo
        {
            public static int MethodWithSameName()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }
        }

        private class TestClass
        {
            public Guid ReferenceGuid = Guid.NewGuid();

            public int GetterSetter { get; set; }

            public int InstanceMethodWithParamAndReturn(int a)
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }

            public void CallTheSameMethodTwice()
            {
                StaticMethodsTestClass.EmptyMethod();
                StaticMethodsTestClass.EmptyMethod();
            }

            public bool CallTwoDifferentMethods()
            {
                var result1 = StaticMethodsTestClass.MethodWithParamAndReturn(5);
                var result2 = StaticMethodsTestClass.MethodWithParamsAndReturn(7, 1);
                return result1 < result2;
            }

            public bool CallTwoDifferentMethodsWithSameName()
            {
                var result1 = TestStaticMethodSameNameOne.MethodWithSameName();
                var result2 = TestStaticMethodSameNameTwo.MethodWithSameName();
                return result1 < result2;
            }

            public int CallSameMethodMultipleTimes()
            {
                var result = 0;
                result += StaticMethodsTestClass.MethodWithParamAndReturn(10);
                result += StaticMethodsTestClass.MethodWithParamAndReturn(9);
                result += StaticMethodsTestClass.MethodWithParamAndReturn(8);
                result += StaticMethodsTestClass.MethodWithParamAndReturn(7);
                result += StaticMethodsTestClass.MethodWithParamAndReturn(6);
                return result;
            }

            public int CallDifferentInstancesOfSameMethod()
            {
                var instance1 = new TestClass();
                var instance2 = new TestClass();
                return instance1.InstanceMethodWithParamAndReturn(1) + instance2.InstanceMethodWithParamAndReturn(2);
            }

            public int MethodCallingGetterSetter()
            {
                var result = InstanceMethodWithParamAndReturn(1);
                return GetterSetter;
            }

            public string MethodWithStringConcat()
            {
                var string1 = "bird";
                string1 += "stone";
                return string1;
            }

            public int MethodThatCallsPrivateMethod()
            {
                return APrivateMethod();
            }

            private int APrivateMethod()
            {
                return 5;
            }
        }

        [TestMethod]
        public void PoseWrapper_Creates_One_Shim_Per_Unique_Method_Called_Multiple_Times()
        {
            var wrapper = new PoseWrapper(new TestClass().CallTheSameMethodTwice);
            Assert.AreEqual(1, wrapper._shimmedMethods.Count);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_MethodInfo()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn(methodInfo1, 3);
            wrapper.SetReturn(methodInfo2, 7);

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual(2, wrapper.LastExecutionResults.Count);
            var resultsMethodWithParamAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo1)).Value;
            var resultsMethodWithParamsAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo2)).Value;
            Assert.AreEqual(1, resultsMethodWithParamAndReturn.Count);
            Assert.AreEqual(1, resultsMethodWithParamsAndReturn.Count);
            var resultsMethodWithParamAndReturnData = resultsMethodWithParamAndReturn[0];
            var resultsMethodWithParamsAndReturnData = resultsMethodWithParamsAndReturn[0];

            Assert.IsTrue(resultsMethodWithParamAndReturnData.CalledAt > preCallDateTime);
            Assert.IsTrue(resultsMethodWithParamsAndReturnData.CalledAt > resultsMethodWithParamAndReturnData.CalledAt);
            Assert.AreEqual(5, (int)resultsMethodWithParamAndReturnData.Parameters[0]);
            Assert.AreEqual(7, (int)resultsMethodWithParamsAndReturnData.Parameters[0]);
            Assert.AreEqual(1, (int)resultsMethodWithParamsAndReturnData.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_Method_Name()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn("MethodWithParamAndReturn", 3);
            wrapper.SetReturn("MethodWithParamsAndReturn", 7);

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual(2, wrapper.LastExecutionResults.Count);
            var resultsMethodWithParamAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo1)).Value;
            var resultsMethodWithParamsAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo2)).Value;
            Assert.AreEqual(1, resultsMethodWithParamAndReturn.Count);
            Assert.AreEqual(1, resultsMethodWithParamsAndReturn.Count);
            var resultsMethodWithParamAndReturnData = resultsMethodWithParamAndReturn[0];
            var resultsMethodWithParamsAndReturnData = resultsMethodWithParamsAndReturn[0];

            Assert.IsTrue(resultsMethodWithParamAndReturnData.CalledAt > preCallDateTime);
            Assert.IsTrue(resultsMethodWithParamsAndReturnData.CalledAt > resultsMethodWithParamAndReturnData.CalledAt);
            Assert.AreEqual(5, (int)resultsMethodWithParamAndReturnData.Parameters[0]);
            Assert.AreEqual(7, (int)resultsMethodWithParamsAndReturnData.Parameters[0]);
            Assert.AreEqual(1, (int)resultsMethodWithParamsAndReturnData.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_Method_Name_With_Class_Specifier()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethodsWithSameName, null, WrapperOptions.None);
            var methodInfo1 = typeof(TestStaticMethodSameNameOne).GetMethod("MethodWithSameName");
            var methodInfo2 = typeof(TestStaticMethodSameNameTwo).GetMethod("MethodWithSameName");
            wrapper.SetReturn("TestStaticMethodSameNameOne.MethodWithSameName", 3);
            wrapper.SetReturn("TestStaticMethodSameNameTwo.MethodWithSameName", 7);

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual(2, wrapper.LastExecutionResults.Count);
            var resultsMethodWithSameNameOne = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo1)).Value;
            var resultsMethodWithSameNameTwo = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo2)).Value;
            Assert.AreEqual(1, resultsMethodWithSameNameOne.Count);
            Assert.AreEqual(1, resultsMethodWithSameNameTwo.Count);
            var resultsMethodWithSameNameOneData = resultsMethodWithSameNameOne[0];
            var resultsMethodWithSameNameTwoData = resultsMethodWithSameNameTwo[0];

            Assert.IsTrue(resultsMethodWithSameNameOneData.CalledAt > preCallDateTime);
            Assert.IsTrue(resultsMethodWithSameNameTwoData.CalledAt > resultsMethodWithSameNameOneData.CalledAt);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_On_Function_Call()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn(() => StaticMethodsTestClass.MethodWithParamAndReturn(Pose.Is.A<int>()), 3);
            wrapper.SetReturn(() => StaticMethodsTestClass.MethodWithParamsAndReturn(Pose.Is.A<int>(), Pose.Is.A<int>()), 7);

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.IsTrue(result);

            Assert.AreEqual(2, wrapper.LastExecutionResults.Count);
            var resultsMethodWithParamAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo1)).Value;
            var resultsMethodWithParamsAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Equals(methodInfo2)).Value;
            Assert.AreEqual(1, resultsMethodWithParamAndReturn.Count);
            Assert.AreEqual(1, resultsMethodWithParamsAndReturn.Count);
            var resultsMethodWithParamAndReturnData = resultsMethodWithParamAndReturn[0];
            var resultsMethodWithParamsAndReturnData = resultsMethodWithParamsAndReturn[0];

            Assert.IsTrue(resultsMethodWithParamAndReturnData.CalledAt > preCallDateTime);
            Assert.IsTrue(resultsMethodWithParamsAndReturnData.CalledAt > resultsMethodWithParamAndReturnData.CalledAt);
            Assert.AreEqual(5, (int)resultsMethodWithParamAndReturnData.Parameters[0]);
            Assert.AreEqual(7, (int)resultsMethodWithParamsAndReturnData.Parameters[0]);
            Assert.AreEqual(1, (int)resultsMethodWithParamsAndReturnData.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Does_Not_Shim_Getter_Setter_When_Option_Not_Set()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.MethodCallingGetterSetter);

            try
            {
                wrapper.SetReturn(() => a.GetterSetter, 5);
                Assert.Fail("Expected exception.");
            }
            catch (InvalidOperationException e)
            {
                var methodInfo = a.GetType().GetMethod("get_GetterSetter");
                Assert.AreEqual(e.Message, string.Format(PoseWrapper.CouldNotFindMatchingShimError, methodInfo));
            }
        }

        [TestMethod]
        public void PoseWrapper_Works_When_Getter_Setter_Not_Shimmed()
        {
            var a = new TestClass();
            a.GetterSetter = 9;
            var wrapper = Shimmer.GetPoseWrapper<int>(() => a.MethodCallingGetterSetter(), WrapperOptions.None);
            var result = wrapper.Execute();
            Assert.AreEqual(9, result);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count());
            Assert.AreEqual("InstanceMethodWithParamAndReturn", wrapper.LastExecutionResults.First().Key.Name);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_On_Getter_Setter_When_Option_Is_Set()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.MethodCallingGetterSetter, null, WrapperOptions.ShimSpecialNames);
            wrapper.SetReturn(() => a.GetterSetter, 5);

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.AreEqual(result, 5);

            Assert.AreEqual(2, wrapper.LastExecutionResults.Count);
            var resultsInstanceMethodWithParamAndReturn = wrapper.LastExecutionResults.First(ler => ler.Key.Name.Equals("InstanceMethodWithParamAndReturn")).Value;
            var resultsGetterSetter = wrapper.LastExecutionResults.First(ler => ler.Key.Name.Equals("get_GetterSetter")).Value;
            Assert.AreEqual(1, resultsGetterSetter.Count);
            Assert.IsTrue(resultsGetterSetter.First().CalledAt > preCallDateTime);
            Assert.AreEqual(1, resultsInstanceMethodWithParamAndReturn.Count);
            Assert.IsTrue(resultsGetterSetter.First().CalledAt > resultsInstanceMethodWithParamAndReturn.First().CalledAt);
        }

        [TestMethod]
        public void PoseWrapper_Multiple_Calls_To_Same_Method_Record_Separate_Call_Results()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.CallSameMethodMultipleTimes, null, WrapperOptions.None);
            wrapper.SetReturn("StaticMethodsTestClass.MethodWithParamAndReturn", 1);
            var currentCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.AreEqual(5, result); // method is called 5 times, returning 1 each time

            // verify all call results are correct
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count); // one method was called
            var methodInfo = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            Assert.AreEqual(methodInfo, wrapper.LastExecutionResults.First().Key);

            var callResults = wrapper.LastExecutionResults.First().Value;
            Assert.AreEqual(5, callResults.Count); // that method was called five times
            for(var i = 0; i < 5; i++)
            {
                var callResult = callResults[i];
                Assert.IsTrue(currentCallDateTime < callResult.CalledAt);
                // params count backwards from 10
                Assert.AreEqual(10 - i, (int)callResult.Parameters[0]);
                currentCallDateTime = callResult.CalledAt;
            }
        }

        [TestMethod]
        public void PoseWrapper_Multiple_Calls_To_Different_Instances_Of_Method_Record_Separate_Call_Results()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.CallDifferentInstancesOfSameMethod, null, WrapperOptions.None);
            wrapper.SetReturn("TestClass.InstanceMethodWithParamAndReturn", 1);
            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();
            Assert.AreEqual(2, result);

            // verify all call results are correct
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count); // one method was called
            var methodInfo = typeof(TestClass).GetMethod("InstanceMethodWithParamAndReturn");
            Assert.AreEqual(methodInfo, wrapper.LastExecutionResults.First().Key);

            var callResults = wrapper.LastExecutionResults.First().Value;
            Assert.AreEqual(2, callResults.Count);
            var call1 = callResults[0];
            var call2 = callResults[1];
            Assert.IsTrue(call1.CalledAt > preCallDateTime);
            Assert.IsTrue(call2.CalledAt > call1.CalledAt);
            Assert.IsInstanceOfType(call1.Parameters[0], typeof(TestClass));
            Assert.IsInstanceOfType(call2.Parameters[0], typeof(TestClass));
            Assert.AreNotEqual((TestClass)call1.Parameters[0], (TestClass)call2.Parameters[0]);
            Assert.AreNotEqual(((TestClass)call1.Parameters[0]).ReferenceGuid, ((TestClass)call2.Parameters[0]).ReferenceGuid);
            Assert.AreEqual(1, (int)call1.Parameters[1]);
            Assert.AreEqual(2, (int)call2.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Does_Not_Shim_String_Concat()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<string>((Func<string>)a.MethodWithStringConcat, null, WrapperOptions.None);
            var result = wrapper.Execute();
            Assert.AreEqual("birdstone", result);
            Assert.IsFalse(wrapper.LastExecutionResults.Any());
        }

        [TestMethod]
        public void PoseWrapper_Shims_Private_Methods_When_Option_Is_Set()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.MethodThatCallsPrivateMethod, null, WrapperOptions.ShimPrivateMembers);
            wrapper.SetReturn("APrivateMethod", 6);
            var result = wrapper.Execute();
            Assert.AreEqual(6, result);
            var callResults = wrapper.LastExecutionResults.First(l => l.Key.Name.Equals("APrivateMethod")).Value;
            Assert.IsNotNull(callResults);
            Assert.AreEqual(1, callResults.Count);
        }

        [TestMethod]
        public void PoseWrapper_Does_Not_Shim_Private_Methods_When_Option_Not_Set()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<int>((Func<int>)a.MethodThatCallsPrivateMethod, null, WrapperOptions.None);

            try
            {
                wrapper.SetReturn("APrivateMethod", 6);
                Assert.Fail("Expected exception.");
            }
            catch (InvalidOperationException)
            {
                // expected
            }

            var result = wrapper.Execute();
            Assert.AreEqual(5, result);
            Assert.IsFalse(wrapper.LastExecutionResults.Any(l => l.Key.Name.Equals("APrivateMethod")));
        }

        [TestMethod]
        public void PoseWrapper_Returns_Results_Per_Method_By_MethodInfo()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();

            var callResults = wrapper.ResultsFor(methodInfo);
            Assert.IsNotNull(callResults);
            Assert.AreEqual(1, callResults.Count);
            Assert.IsTrue(callResults.SequenceEqual(wrapper.LastExecutionResults.FirstOrDefault(m => m.Key == methodInfo).Value));
        }

        [TestMethod]
        public void PoseWrapper_Returns_Results_Per_Method_By_Name()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();

            var callResults = wrapper.ResultsFor("MethodWithParamsAndReturn");
            Assert.IsNotNull(callResults);
            Assert.AreEqual(1, callResults.Count);
            Assert.IsTrue(callResults.SequenceEqual(wrapper.LastExecutionResults.FirstOrDefault(m => m.Key == methodInfo).Value));
        }

        [TestMethod]
        public void PoseWrapper_Returns_Results_Per_Method_By_Expression()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null, WrapperOptions.None);
            var methodInfo = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");

            var preCallDateTime = DateTime.Now;
            var result = wrapper.Execute();

            var callResults = wrapper.ResultsFor(() => StaticMethodsTestClass.MethodWithParamsAndReturn(Pose.Is.A<int>(), Pose.Is.A<int>()));
            Assert.IsNotNull(callResults);
            Assert.AreEqual(1, callResults.Count);
            Assert.IsTrue(callResults.SequenceEqual(wrapper.LastExecutionResults.FirstOrDefault(m => m.Key == methodInfo).Value));
        }
    }
}
