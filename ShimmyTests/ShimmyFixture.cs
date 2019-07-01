using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shimmy.Tests.SharedTestClasses;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Shimmy.Tests
{
    [TestClass]
    public class ShimmyFixture
    {
        private static class StaticTestClass
        {
            public static void VoidMethod()
            {
                var result = StaticMethodsTestClass.MethodWithParamAndReturn(2);
            }

            public static int MethodWithReturn()
            {
                return StaticMethodsTestClass.MethodWithParamAndReturn(2);
            }

            public static void MethodWithValueTypeParam(int a)
            {
                var result = StaticMethodsTestClass.MethodWithParamAndReturn(a);
            }

            public static int MethodWithParamAndReturn(int a)
            {
                return StaticMethodsTestClass.MethodWithParamAndReturn(a);
            }
        }

        private class InstanceTestClass
        {
            public void VoidMethod()
            {
                var result = StaticMethodsTestClass.MethodWithParamAndReturn(2);
            }

            public int MethodWithReturn()
            {
                return StaticMethodsTestClass.MethodWithParamAndReturn(2);
            }

            public void MethodWithValueTypeParam(int a)
            {
                var result = StaticMethodsTestClass.MethodWithParamAndReturn(a);
            }

            public int MethodWithParamAndReturn(int a)
            {
                return StaticMethodsTestClass.MethodWithParamAndReturn(a);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_From_Delegate_Throws_InvalidOperationException_On_Non_Void_Return()
        {
            try
            {
                var badDelegate = typeof(StaticTestClass).GetMethod("MethodWithReturn").CreateDelegate(typeof(Func<int>));
                var wrapper = Shimmy.GetPoseWrapper(badDelegate);
                Assert.Fail("Expected ArgumentException.");
            }
            catch(ArgumentException e)
            {
                Assert.AreEqual(Shimmy.ReturnlessWrapperInvalidDelegate, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_T_From_Delegate_Throws_InvalidOperationException_On_Non_Matching_Return_Type()
        {
            try
            {
                var badDelegate = typeof(StaticTestClass).GetMethod("MethodWithReturn").CreateDelegate(typeof(Func<int>));
                var wrapper = Shimmy.GetPoseWrapper<string>(badDelegate);
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Shimmy.NonMatchingReturnType, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_From_Expression_Throws_InvalidOperationException_On_Non_Void_Return()
        {
            try
            {
                var wrapper = Shimmy.GetPoseWrapper(() => StaticTestClass.MethodWithReturn());
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Shimmy.ReturnlessWrapperInvalidDelegate, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_T_From_Expression_Throws_InvalidOperationException_On_Non_Matching_Return_Type()
        {
            try
            {
                var wrapper = Shimmy.GetPoseWrapper<string>(() => StaticTestClass.VoidMethod());
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Shimmy.NonMatchingReturnType, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_From_Method_Throws_InvalidOperationException_On_Non_Void_Return()
        {
            try
            {
                var methodInfo = typeof(StaticTestClass).GetMethod("MethodWithReturn");
                var wrapper = Shimmy.GetPoseWrapper(methodInfo);
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Shimmy.ReturnlessWrapperInvalidDelegate, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_T_From_Method_Throws_InvalidOperationException_On_Non_Matching_Return_Type()
        {
            try
            {
                var methodInfo = typeof(StaticTestClass).GetMethod("VoidMethod");
                var wrapper = Shimmy.GetPoseWrapper<string>(methodInfo);
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Shimmy.NonMatchingReturnType, e.Message);
            }
        }

        [TestMethod]
        public void GetPoseWrapper_From_Delegate_Returns_New_PoseWrapper_For_Parameterless_Method()
        {
            var wrapper = Shimmy.GetPoseWrapper((Action)StaticTestClass.VoidMethod);
            Assert.IsNotNull(wrapper);
            wrapper.Execute();
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_From_Delegate_Returns_New_PoseWrapper_For_Method_With_Parameters()
        {
            var wrapper = Shimmy.GetPoseWrapper((Action<int>)StaticTestClass.MethodWithValueTypeParam);
            Assert.IsNotNull(wrapper);
            wrapper.Execute(5);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_T_From_Delegate_Returns_New_Pose_Wrapper_For_Method_With_Appropriate_Return_Type()
        {
            var wrapper = Shimmy.GetPoseWrapper<int>((Func<int>)StaticTestClass.MethodWithReturn);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute();
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_T_From_Delegate_Returns_New_Pose_Wrapper_For_Method_With_Appropriate_Return_Type_And_Parameters()
        {
            var wrapper = Shimmy.GetPoseWrapper<int>((Func<int, int>)StaticTestClass.MethodWithParamAndReturn);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute(5);
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Expression()
        {
            var wrapper = Shimmy.GetPoseWrapper(() => StaticTestClass.VoidMethod());
            Assert.IsNotNull(wrapper);
            wrapper.Execute();
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Expression_With_Return()
        {
            var wrapper = Shimmy.GetPoseWrapper<int>(() => StaticTestClass.MethodWithReturn());
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute();
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Expression_With_Parameters()
        {
            var wrapper = Shimmy.GetPoseWrapper(() => StaticTestClass.MethodWithValueTypeParam(default(int)));
            Assert.IsNotNull(wrapper);
            wrapper.Execute(5);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Expression_With_Parameters_And_Return()
        {
            var wrapper = Shimmy.GetPoseWrapper<int>(() => StaticTestClass.MethodWithParamAndReturn(default(int)));
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute(5);
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Method()
        {
            var methodInfo = typeof(StaticTestClass).GetMethod("VoidMethod");
            var wrapper = Shimmy.GetPoseWrapper(methodInfo);
            Assert.IsNotNull(wrapper);
            wrapper.Execute();
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Method_With_Return()
        {
            var methodInfo = typeof(StaticTestClass).GetMethod("MethodWithReturn");
            var wrapper = Shimmy.GetPoseWrapper<int>(methodInfo);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute();
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Method_With_Parameters()
        {
            var methodInfo = typeof(StaticTestClass).GetMethod("MethodWithValueTypeParam");
            var wrapper = Shimmy.GetPoseWrapper(methodInfo);
            Assert.IsNotNull(wrapper);
            wrapper.Execute(5);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Method_With_Parameters_And_Return()
        {
            var methodInfo = typeof(StaticTestClass).GetMethod("MethodWithParamAndReturn");
            var wrapper = Shimmy.GetPoseWrapper<int>(methodInfo);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute(5);
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Expression()
        {
            var a = new InstanceTestClass();
            var wrapper = Shimmy.GetPoseWrapper(() => a.VoidMethod());
            Assert.IsNotNull(wrapper);
            wrapper.Execute();
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Expression_With_Return()
        {
            var a = new InstanceTestClass();
            var wrapper = Shimmy.GetPoseWrapper<int>(() => a.MethodWithReturn());
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute();
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Expression_With_Parameters()
        {
            var a = new InstanceTestClass();
            var wrapper = Shimmy.GetPoseWrapper(() => a.MethodWithValueTypeParam(default(int)));
            Assert.IsNotNull(wrapper);
            wrapper.Execute(5);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Expression_With_Parameters_And_Return()
        {
            var a = new InstanceTestClass();
            var wrapper = Shimmy.GetPoseWrapper<int>(() => a.MethodWithParamAndReturn(5));
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute(5);
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Method()
        {
            var a = new InstanceTestClass();
            var methodInfo = a.GetType().GetMethod("VoidMethod");
            var wrapper = Shimmy.GetPoseWrapper(methodInfo, a);
            Assert.IsNotNull(wrapper);
            wrapper.Execute();
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Method_With_Return()
        {
            var a = new InstanceTestClass();
            var methodInfo = a.GetType().GetMethod("MethodWithReturn");
            var wrapper = Shimmy.GetPoseWrapper<int>(methodInfo, a);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute();
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Method_With_Parameters()
        {
            var a = new InstanceTestClass();
            var methodInfo = a.GetType().GetMethod("MethodWithValueTypeParam");
            var wrapper = Shimmy.GetPoseWrapper(methodInfo, a);
            Assert.IsNotNull(wrapper);
            wrapper.Execute(5);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);
        }

        [TestMethod]
        public void GetPoseWrapper_Generates_Wrapper_From_Instance_Method_With_Parameters_And_Return()
        {
            var a = new InstanceTestClass();
            var methodInfo = a.GetType().GetMethod("MethodWithParamAndReturn");
            var wrapper = Shimmy.GetPoseWrapper<int>(methodInfo, a);
            Assert.IsNotNull(wrapper);
            var result = wrapper.Execute(5);
            Assert.AreEqual(0, result);
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);
            Assert.AreEqual(5, wrapper.LastExecutionResults.First().Value[0].Parameters[0]);

        }
    }
}
