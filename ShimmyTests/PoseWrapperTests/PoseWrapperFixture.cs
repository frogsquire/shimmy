using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shimmy.Tests.SharedTestClasses;
using System;
using System.Collections.Generic;
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
            public void CallTheSameMethodTwice()
            {
                StaticMethodsTestClass.EmptyMethod();
                StaticMethodsTestClass.EmptyMethod();
            }

            public bool CallTwoDifferentMethods()
            {
                var result1 = StaticMethodsTestClass.MethodWithParamAndReturn(0);
                var result2 = StaticMethodsTestClass.MethodWithParamsAndReturn(0, 1);
                return result1 < result2;
            }

            public bool CallTwoDifferentMethodsWithSameName()
            {
                var result1 = TestStaticMethodSameNameOne.MethodWithSameName();
                var result2 = TestStaticMethodSameNameTwo.MethodWithSameName();
                return result1 < result2;
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
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn(methodInfo1, 3);
            wrapper.SetReturn(methodInfo2, 7);

            var result = wrapper.Execute();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_Method_Name()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn("MethodWithParamAndReturn", 3);
            wrapper.SetReturn("MethodWithParamsAndReturn", 7);

            var result = wrapper.Execute();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_Method_Name_With_Class_Specifier()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethodsWithSameName, null);
            var methodInfo1 = typeof(TestStaticMethodSameNameOne).GetMethod("MethodWithSameName");
            var methodInfo2 = typeof(TestStaticMethodSameNameTwo).GetMethod("MethodWithSameName");
            wrapper.SetReturn("TestStaticMethodSameNameOne.MethodWithSameName", 3);
            wrapper.SetReturn("TestStaticMethodSameNameTwo.MethodWithSameName", 7);

            var result = wrapper.Execute();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PoseWrapper_SetReturn_Changes_Value_Of_Correct_Shim_Via_Expression()
        {
            var a = new TestClass();
            var wrapper = new PoseWrapper<bool>((Func<bool>)a.CallTwoDifferentMethods, null);
            var methodInfo1 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn");
            var methodInfo2 = typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn");
            wrapper.SetReturn(() => StaticMethodsTestClass.MethodWithParamAndReturn(Pose.Is.A<int>()), 3);
            wrapper.SetReturn(() => StaticMethodsTestClass.MethodWithParamsAndReturn(Pose.Is.A<int>(), Pose.Is.A<int>()), 7);

            var result = wrapper.Execute();
            Assert.IsTrue(result);
        }
    }
}
