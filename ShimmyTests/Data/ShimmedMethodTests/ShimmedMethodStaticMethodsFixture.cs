using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using System;
using System.Collections.Generic;
using System.Linq;
using Shimmy.Tests.SharedTestClasses;
using Shimmy.Data;

namespace Shimmy.Tests.Data.ShimmedMethodTests
{
    [TestClass]
    public class ShimmedMethodStaticMethodsFixture
    {
        [TestMethod]
        public void ShimmedMethod_Generates_From_Empty_Static_Method_Call()
        {
            var shimmedMethod = new ShimmedMethod(typeof(StaticMethodsTestClass).GetMethod("EmptyMethod"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            try
            {
                var beforeDateTime = DateTime.Now;
                PoseContext.Isolate(() => {
                    StaticMethodsTestClass.EmptyMethod();
                }, new[] { shimmedMethod.Shim });
                Assert.AreEqual(1, shimmedMethod.CallResults.Count);
                var callResult = shimmedMethod.CallResults.First();
                Assert.IsNotNull(callResult.Parameters);
                var afterDateTime = DateTime.Now;
                Assert.IsNotNull(callResult.CalledAt);
                Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_And_Returns_Value()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(StaticMethodsTestClass).GetMethod("MethodWithReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithReturn();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_Records_Value_Type_Parameters()
        {
            var shimmedMethod = new ShimmedMethod(typeof(StaticMethodsTestClass).GetMethod("MethodWithValueTypeParam"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                StaticMethodsTestClass.MethodWithValueTypeParam(5);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            var expectedParam = callResult.Parameters[0];
            Assert.AreEqual(5, (int)expectedParam);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_Records_String_Parameters()
        {
            var shimmedMethod = new ShimmedMethod(typeof(StaticMethodsTestClass).GetMethod("MethodWithStringParam"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                StaticMethodsTestClass.MethodWithStringParam("bird");
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            var expectedParam = callResult.Parameters[0];
            Assert.AreEqual("bird", (string)expectedParam);
        }
        
        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_Records_Multi_Parameters()
        {
            var shimmedMethod = new ShimmedMethod(typeof(StaticMethodsTestClass).GetMethod("MethodWithMultiParams"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);
            
            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                StaticMethodsTestClass.MethodWithMultiParams(5, 6, "bird", new List<bool> { true, false, true });
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);

            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);

            Assert.AreEqual(5, (int)callResult.Parameters[0]);
            Assert.AreEqual(6, (int)callResult.Parameters[1]);
            Assert.AreEqual("bird", (string)callResult.Parameters[2]);
            Assert.IsTrue(new List<bool> { true, false, true }.SequenceEqual((List<bool>)callResult.Parameters[3]));
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_With_Param_And_Returns_Value()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(StaticMethodsTestClass).GetMethod("MethodWithParamAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithParamAndReturn(2);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.AreEqual(2, (int)callResult.Parameters[0]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_With_Multi_Params_And_Returns_Value()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithParamsAndReturn(2, 4);
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(0, value); // Shimmy will set to default for that value type
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.AreEqual(2, (int)callResult.Parameters[0]);
            Assert.AreEqual(4, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_With_Params_And_Returns_Reference_Type()
        {
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(StaticMethodsTestClass).GetMethod("MethodWithParamsAndReferenceTypeReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithParamsAndReferenceTypeReturn(2, 4);
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.AreEqual(2, (int)callResult.Parameters[0]);
            Assert.AreEqual(4, (int)callResult.Parameters[1]);
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_With_Reference_Type_Param_And_Returns_Reference_Type()
        {
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(StaticMethodsTestClass).GetMethod("MethodWithReferenceTypeParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithReferenceTypeParamsAndReturn(new List<int> { 3, 2, 1 });
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsTrue(((List<int>)callResult.Parameters[0]).SequenceEqual(new List<int> { 3, 2, 1 }));
        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Static_Call_With_Multi_Params_And_Returns_Reference_Type()
        {
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(StaticMethodsTestClass).GetMethod("MethodWithMultiReferenceTypeParamsAndReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            List<int> value = null;
            PoseContext.Isolate(() => {
                value = StaticMethodsTestClass.MethodWithMultiReferenceTypeParamsAndReturn(new List<int> { 3, 2, 1 }, "bird", DateTime.Today);
            }, new[] { shimmedMethod.Shim });
            Assert.IsNotNull(value);
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsTrue(((List<int>)callResult.Parameters[0]).SequenceEqual(new List<int> { 3, 2, 1 }));
            Assert.AreEqual("bird", (string)callResult.Parameters[1]);
            Assert.AreEqual(DateTime.Today, (DateTime)callResult.Parameters[2]);
        }
    }
}
