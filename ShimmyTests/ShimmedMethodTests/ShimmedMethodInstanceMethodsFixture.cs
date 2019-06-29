using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using Shimmy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy.Tests.ShimmedMethodTests
{
    [TestClass]
    public class ShimmedMethodInstanceMethodsFixture
    {
        private class TestClass
        {
            public Guid InstanceGuid = Guid.NewGuid();

            public void EmptyMethod()
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public void MethodWithValueTypeParam(int a)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public void MethodWithStringParam(string b)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public void MethodWithObjectParam(List<bool> l)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public void MethodWithMultiParams(int a, int b, string c, List<bool> d)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public int MethodWithReturn()
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public int MethodWithParamAndReturn(int param1)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public int MethodWithParamsAndReturn(int param1, int param2)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public List<int> MethodWithParamsAndReferenceTypeReturn(int param1, int param2)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public List<int> MethodWithReferenceTypeParamsAndReturn(List<int> args)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public List<int> MethodWithMultiReferenceTypeParamsAndReturn(List<int> a, string b, DateTime c)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public virtual void EmptyVirtualMethod()
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public virtual List<int> VirtualMethodWithMultiReferenceTypeParamsAndReturn(List<int> a, string b, DateTime c)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }


        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Empty_Instance_Method_Call()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("EmptyMethod"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                a.EmptyMethod();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Empty_Instance_Virtual_Method_Call()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("EmptyVirtualMethod"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                a.EmptyVirtualMethod();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_And_Returns_Value()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = a.MethodWithReturn();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_Records_Value_Type_Parameters()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("MethodWithValueTypeParam"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                a.MethodWithValueTypeParam(5);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            var expectedParam = callResult.Parameters[1];
            Assert.AreEqual(5, (int)expectedParam);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_Records_String_Parameters()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("MethodWithStringParam"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                a.MethodWithStringParam("bird");
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            var expectedParam = callResult.Parameters[1];
            Assert.AreEqual("bird", (string)expectedParam);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_Records_Multi_Parameters()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("MethodWithMultiParams"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                a.MethodWithMultiParams(5, 6, "bird", new List<bool> { true, false, true });
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.AreEqual(5, (int)callResult.Parameters[1]);
            Assert.AreEqual(6, (int)callResult.Parameters[2]);
            Assert.AreEqual("bird", (string)callResult.Parameters[3]);
            Assert.IsTrue(new List<bool> { true, false, true }.SequenceEqual((List<bool>)callResult.Parameters[4]));
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_With_Param_And_Returns_Value()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithParamAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = a.MethodWithParamAndReturn(2);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.AreEqual(2, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_With_Multi_Params_And_Returns_Value()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = a.MethodWithParamsAndReturn(2, 4);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.AreEqual(2, (int)callResult.Parameters[1]);
            Assert.AreEqual(4, (int)callResult.Parameters[2]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_With_Params_And_Returns_Reference_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("MethodWithParamsAndReferenceTypeReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = a.MethodWithParamsAndReferenceTypeReturn(2, 4);
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.AreEqual(2, (int)callResult.Parameters[1]);
            Assert.AreEqual(4, (int)callResult.Parameters[2]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_With_Reference_Type_Param_And_Returns_Reference_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("MethodWithReferenceTypeParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = a.MethodWithReferenceTypeParamsAndReturn(new List<int> { 3, 2, 1 });
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.IsTrue(((List<int>)callResult.Parameters[1]).SequenceEqual(new List<int> { 3, 2, 1 }));
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Instance_Call_With_Multi_Params_And_Returns_Reference_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("MethodWithMultiReferenceTypeParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = a.MethodWithMultiReferenceTypeParamsAndReturn(new List<int> { 3, 2, 1 }, "bird", DateTime.Today);
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.IsTrue(((List<int>)callResult.Parameters[1]).SequenceEqual(new List<int> { 3, 2, 1 }));
            Assert.AreEqual("bird", (string)callResult.Parameters[2]);
            Assert.AreEqual(DateTime.Today, (DateTime)callResult.Parameters[3]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Virtual_Instance_Call_With_Multi_Params_And_Returns_Reference_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("VirtualMethodWithMultiReferenceTypeParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = a.VirtualMethodWithMultiReferenceTypeParamsAndReturn(new List<int> { 3, 2, 1 }, "bird", DateTime.Today);
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            // first parameter should be instance
            var instanceParam = callResult.Parameters[0] as TestClass;
            Assert.IsNotNull(instanceParam);
            Assert.AreEqual(a.InstanceGuid, instanceParam.InstanceGuid);

            Assert.IsTrue(((List<int>)callResult.Parameters[1]).SequenceEqual(new List<int> { 3, 2, 1 }));
            Assert.AreEqual("bird", (string)callResult.Parameters[2]);
            Assert.AreEqual(DateTime.Today, (DateTime)callResult.Parameters[3]);
        }
    }
}
