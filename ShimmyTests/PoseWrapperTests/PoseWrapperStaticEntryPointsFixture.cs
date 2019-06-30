using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Shimmy.Tests.SharedTestClasses;

namespace Shimmy.Tests.PoseWrapperTests
{
    [TestClass]
    public class PoseWrapperStaticEntryPointsFixture
    {
        #region Static calling Static Methods

        private class TestClassCallingStatic
        {
            public static void EmptyMethod()
            {
                StaticMethodsTestClass.EmptyMethod();
            }

            public static void MethodWithValueTypeParam(int a)
            {
                StaticMethodsTestClass.MethodWithValueTypeParam(a);
            }

            public static void MethodWithStringParam(string b)
            {
                StaticMethodsTestClass.MethodWithStringParam(b);
            }

            public static void MethodWithObjectParam(List<bool> l)
            {
                StaticMethodsTestClass.MethodWithObjectParam(l);
            }

            public static void MethodWithMultiParams(int a, int b, string c, List<bool> d)
            {
                StaticMethodsTestClass.MethodWithMultiParams(a, b, c, d);
            }

            public static int MethodWithReturn()
            {
                return StaticMethodsTestClass.MethodWithReturn();
            }

            public static int MethodWithParamAndReturn(int param1)
            {
                return StaticMethodsTestClass.MethodWithParamAndReturn(param1);
            }

            public static int MethodWithParamsAndReturn(int param1, int param2)
            {
                return StaticMethodsTestClass.MethodWithParamsAndReturn(param1, param2);
            }

            public static List<int> MethodWithParamsAndReferenceTypeReturn(int param1, int param2)
            {
                return StaticMethodsTestClass.MethodWithParamsAndReferenceTypeReturn(param1, param2);
            }

            public static List<int> MethodWithReferenceTypeParamsAndReturn(List<int> args)
            {
                return StaticMethodsTestClass.MethodWithReferenceTypeParamsAndReturn(args);
            }

            public static List<int> MethodWithMultiReferenceTypeParamsAndReturn(List<int> a, string b, DateTime c)
            {
                return StaticMethodsTestClass.MethodWithMultiReferenceTypeParamsAndReturn(a, b, c);
            }
        }

        // todo: test clearing of lastexecutionresults

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Empty_Static_Method_Call_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper(TestClassCallingStatic.EmptyMethod);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute();
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);            

            var result = wrapper.LastExecutionResults.First();
            Assert.AreEqual("EmptyMethod", result.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), result.Key.DeclaringType);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count);

            var callResult = result.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(0, callResult.Parameters.Count());
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_And_Returns_Value_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int>)TestClassCallingStatic.MethodWithReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute();
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result); // todo: custom values return

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(0, callResult.Parameters.Count());
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_Value_Type_Parameters_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper((Action<int>)TestClassCallingStatic.MethodWithValueTypeParam);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { 5 });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithValueTypeParam", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());

            var parameter = callResult.Parameters.First();
            Assert.AreEqual(5, (int)parameter);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_String_Parameters_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper((Action<string>)TestClassCallingStatic.MethodWithStringParam);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { "bird" });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithStringParam", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());

            var parameter = callResult.Parameters.First();
            Assert.AreEqual("bird", (string)parameter);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_Multi_Parameters_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper((Action<int, int, string, List<bool>>)TestClassCallingStatic.MethodWithMultiParams);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { 1, 2, "bird", new List<bool> { true, false, true } });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithMultiParams", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(4, callResult.Parameters.Count());

            Assert.AreEqual(1, (int)callResult.Parameters[0]);
            Assert.AreEqual(2, (int)callResult.Parameters[1]);
            Assert.AreEqual("bird", (string)callResult.Parameters[2]);
            Assert.IsTrue(new List<bool> { true, false, true }.SequenceEqual((List<bool>)callResult.Parameters[3]));
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Param_And_Returns_Value_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int, int>)TestClassCallingStatic.MethodWithParamAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5 });
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result);

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());

            Assert.AreEqual(5, (int)callResult.Parameters[0]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Multi_Params_And_Returns_Value_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int, int, int>)TestClassCallingStatic.MethodWithParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5, 6 });
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result);

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            Assert.AreEqual(5, (int)callResult.Parameters[0]);
            Assert.AreEqual(6, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Params_And_Returns_Reference_Type_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<int, int, List<int>>)TestClassCallingStatic.MethodWithParamsAndReferenceTypeReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5, 6 });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual((List<int>)result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamsAndReferenceTypeReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            Assert.AreEqual(5, (int)callResult.Parameters[0]);
            Assert.AreEqual(6, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Reference_Type_Param_And_Returns_Reference_Type_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<List<int>, List<int>>)TestClassCallingStatic.MethodWithReferenceTypeParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { new List<int> { 4, 3, 2 } });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual(result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithReferenceTypeParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());

            Assert.IsTrue(new List<int> { 4, 3, 2 }.SequenceEqual((List<int>)callResult.Parameters[0]));
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Multi_Params_And_Returns_Reference_Type_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<List<int>, string, DateTime, List<int>>)TestClassCallingStatic.MethodWithMultiReferenceTypeParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { new List<int> { 4, 3, 2 }, "bird", DateTime.Today });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual(result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithMultiReferenceTypeParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(StaticMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(3, callResult.Parameters.Count());

            Assert.IsTrue(new List<int> { 4, 3, 2 }.SequenceEqual((List<int>)callResult.Parameters[0]));
            Assert.AreEqual("bird", (string)callResult.Parameters[1]);
            Assert.AreEqual(DateTime.Today, (DateTime)callResult.Parameters[2]);
        }

        #endregion

        #region Static Calling Instance Methods

        private static class TestClassCallingInstance
        {
            public static void EmptyMethod()
            {
                var inst = new InstanceMethodsTestClass();
                inst.EmptyMethod();
            }

            public static void MethodWithValueTypeParam(int a)
            {
                var inst = new InstanceMethodsTestClass();
                inst.MethodWithValueTypeParam(a);
            }

            public static void MethodWithStringParam(string b)
            {
                var inst = new InstanceMethodsTestClass();
                inst.MethodWithStringParam(b);
            }

            public static void MethodWithObjectParam(List<bool> l)
            {
                var inst = new InstanceMethodsTestClass();
                inst.MethodWithObjectParam(l);
            }

            public static void MethodWithMultiParams(int a, int b, string c, List<bool> d)
            {
                var inst = new InstanceMethodsTestClass();
                inst.MethodWithMultiParams(a, b, c, d);
            }

            public static int MethodWithReturn()
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithReturn();
            }

            public static int MethodWithParamAndReturn(int param1)
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithParamAndReturn(param1);
            }

            public static int MethodWithParamsAndReturn(int param1, int param2)
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithParamsAndReturn(param1, param2);
            }

            public static List<int> MethodWithParamsAndReferenceTypeReturn(int param1, int param2)
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithParamsAndReferenceTypeReturn(param1, param2);
            }

            public static List<int> MethodWithReferenceTypeParamsAndReturn(List<int> args)
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithReferenceTypeParamsAndReturn(args);
            }

            public static List<int> MethodWithMultiReferenceTypeParamsAndReturn(List<int> a, string b, DateTime c)
            {
                var inst = new InstanceMethodsTestClass();
                return inst.MethodWithMultiReferenceTypeParamsAndReturn(a, b, c);
            }
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Empty_Static_Method_Call_Calling_Instance_Method_Calling_Static_Method()
        {
            var wrapper = new PoseWrapper(TestClassCallingInstance.EmptyMethod);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute();
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var result = wrapper.LastExecutionResults.First();
            Assert.AreEqual("EmptyMethod", result.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), result.Key.DeclaringType);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count);

            var callResult = result.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());

            // verify this instance was the one most recently created
            // and therefore, was the one created in the method call
            // (which will work until/unless constructors are shimmed)
            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_And_Returns_Value_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int>)TestClassCallingInstance.MethodWithReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute();
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result); // todo: custom values return

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(1, callResult.Parameters.Count());
            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_Value_Type_Parameters_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper((Action<int>)TestClassCallingInstance.MethodWithValueTypeParam);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { 5 });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithValueTypeParam", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual(5, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_String_Parameters_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper((Action<string>)TestClassCallingInstance.MethodWithStringParam);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { "bird" });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithStringParam", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual("bird", (string)callResult.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_Records_Multi_Parameters_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper((Action<int, int, string, List<bool>>)TestClassCallingInstance.MethodWithMultiParams);

            var beforeDateTime = DateTime.Now;
            wrapper.Execute(new object[] { 1, 2, "bird", new List<bool> { true, false, true } });
            var afterDateTime = DateTime.Now;

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithMultiParams", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(5, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual(1, (int)callResult.Parameters[1]);
            Assert.AreEqual(2, (int)callResult.Parameters[2]);
            Assert.AreEqual("bird", (string)callResult.Parameters[3]);
            Assert.IsTrue(new List<bool> { true, false, true }.SequenceEqual((List<bool>)callResult.Parameters[4]));
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Param_And_Returns_Value_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int, int>)TestClassCallingInstance.MethodWithParamAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5 });
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result);

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual(5, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Multi_Params_And_Returns_Value_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<int>((Func<int, int, int>)TestClassCallingInstance.MethodWithParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5, 6 });
            var afterDateTime = DateTime.Now;

            Assert.AreEqual(0, result);

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(3, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual(5, (int)callResult.Parameters[1]);
            Assert.AreEqual(6, (int)callResult.Parameters[2]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Params_And_Returns_Reference_Type_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<int, int, List<int>>)TestClassCallingInstance.MethodWithParamsAndReferenceTypeReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { 5, 6 });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual((List<int>)result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithParamsAndReferenceTypeReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(3, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.AreEqual(5, (int)callResult.Parameters[1]);
            Assert.AreEqual(6, (int)callResult.Parameters[2]);
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Reference_Type_Param_And_Returns_Reference_Type_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<List<int>, List<int>>)TestClassCallingInstance.MethodWithReferenceTypeParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { new List<int> { 4, 3, 2 } });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual(result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithReferenceTypeParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(2, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.IsTrue(new List<int> { 4, 3, 2 }.SequenceEqual((List<int>)callResult.Parameters[1]));
        }

        [TestMethod]
        public void PoseWrapper_Shims_And_Executes_From_Static_Call_With_Multi_Params_And_Returns_Reference_Type_Calling_Instance_Method()
        {
            var wrapper = new PoseWrapper<List<int>>((Func<List<int>, string, DateTime, List<int>>)TestClassCallingInstance.MethodWithMultiReferenceTypeParamsAndReturn, null);

            var beforeDateTime = DateTime.Now;
            var result = wrapper.Execute(new object[] { new List<int> { 4, 3, 2 }, "bird", DateTime.Today });
            var afterDateTime = DateTime.Now;

            Assert.IsTrue(new List<int>().SequenceEqual(result));

            var lastCallResults = wrapper.LastExecutionResults;
            Assert.IsNotNull(wrapper.LastExecutionResults);
            Assert.AreEqual(1, wrapper.LastExecutionResults.Count);

            var executionResult = wrapper.LastExecutionResults.First();
            Assert.AreEqual("MethodWithMultiReferenceTypeParamsAndReturn", executionResult.Key.Name);
            Assert.AreEqual(typeof(InstanceMethodsTestClass), executionResult.Key.DeclaringType);
            Assert.IsNotNull(executionResult.Value);
            Assert.AreEqual(1, executionResult.Value.Count);

            var callResult = executionResult.Value.First();
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsNotNull(callResult.Parameters);
            Assert.AreEqual(4, callResult.Parameters.Count());

            var callResultInstance = (InstanceMethodsTestClass)callResult.Parameters[0];
            Assert.AreEqual(InstanceMethodsTestClassTracker.LastCreated.InstanceGuid, callResultInstance.InstanceGuid);

            Assert.IsTrue(new List<int> { 4, 3, 2 }.SequenceEqual((List<int>)callResult.Parameters[1]));
            Assert.AreEqual("bird", (string)callResult.Parameters[2]);
            Assert.AreEqual(DateTime.Today, (DateTime)callResult.Parameters[3]);
        }

        #endregion
    }
}