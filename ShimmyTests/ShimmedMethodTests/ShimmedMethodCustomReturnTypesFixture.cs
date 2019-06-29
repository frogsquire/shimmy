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
    public class ShimmedMethodCustomReturnTypesFixture
    {
        private class TestClass
        {
            public Guid InstanceGuid = Guid.NewGuid();

            public int MethodWithValueReturnType()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }

            public List<int> MethodWithReferenceReturnType()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }

            public static int StaticMethodWithValueReturnType()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }

            public static List<int> StaticMethodWithReferenceReturnType()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }
        }

        [TestMethod]
        public void ShimmedMethod_Call_Returns_Custom_Return_Value_For_Instance_Method_Value_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithValueReturnType"), 5);
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            var value = 0;
            PoseContext.Isolate(() => {
                value = a.MethodWithValueReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.AreEqual(5, value);
        }

        [TestMethod]
        public void ShimmedMethod_Call_Returns_Custom_Return_Value_For_Static_Method_Value_Type()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("StaticMethodWithValueReturnType"), 5);
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            var value = 0;
            PoseContext.Isolate(() => {
                value = TestClass.StaticMethodWithValueReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.AreEqual(5, value);
        }

        [TestMethod]
        public void ShimmedMethod_Call_Returns_Custom_Return_Value_For_Instance_Method_Reference_Type()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("MethodWithReferenceReturnType"), new List<int> { 1, 2, 3 });
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            var value = new List<int>();
            PoseContext.Isolate(() => {
                value = a.MethodWithReferenceReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsTrue(value.SequenceEqual(new List<int> { 1, 2, 3 }));
        }

        [TestMethod]
        public void ShimmedMethod_Call_Returns_Custom_Return_Value_For_Static_Method_Reference_Type()
        {
            var shimmedMethod = new ShimmedMethod<List<int>>(typeof(TestClass).GetMethod("StaticMethodWithReferenceReturnType"), new List<int> { 1, 2, 3 });
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            var value = new List<int>();
            PoseContext.Isolate(() => {
                value = TestClass.StaticMethodWithReferenceReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(1, shimmedMethod.CallResults.Count);
            var callResult = shimmedMethod.CallResults.First();
            Assert.IsNotNull(callResult.Parameters);
            var afterDateTime = DateTime.Now;
            Assert.IsNotNull(callResult.CalledAt);
            Assert.IsTrue(beforeDateTime < callResult.CalledAt && callResult.CalledAt < afterDateTime);
            Assert.IsTrue(value.SequenceEqual(new List<int> { 1, 2, 3 }));
        }

        [TestMethod]
        public void ShimmedMethod_Call_Returns_New_Return_Value_When_Return_Value_Changed()
        {
            var a = new TestClass();
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithValueReturnType"), 5);
            var value = 0;
            PoseContext.Isolate(() => {
                value = a.MethodWithValueReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(5, value);
            
            shimmedMethod.ReturnValue = 6;
            var value2 = 0;
            PoseContext.Isolate(() => {
                value2 = a.MethodWithValueReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(6, value2);
        }
    }
}
