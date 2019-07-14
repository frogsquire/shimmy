using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using Shimmy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy.Tests.Data.ShimmedMethodTests
{
    [TestClass]
    public class ShimmedMethodCustomReturnTypesFixture
    {
        private class TestClass
        {
            public Guid InstanceGuid = Guid.NewGuid();

            public static void VoidMethod()
            {
            }

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

            public static TestClassNoParameterlessConstructor GetTestClassNoParameterlessConstructor()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }

            public static TestClass GetTestClass()
            {
                throw new NotImplementedException("Intentionally unimplemented.");
            }
        }

        private class TestClassNoParameterlessConstructor
        {
            public TestClassNoParameterlessConstructor(int a)
            {
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

            shimmedMethod.SetReturnValue(7);
            var value3 = 0;
            PoseContext.Isolate(() => {
                value3 = a.MethodWithValueReturnType();
            }, new[] { shimmedMethod.Shim });
            Assert.AreEqual(7, value3);
        }

        [TestMethod]
        public void ShimmedMethod_SetReturnValue_Excepts_When_Called_On_ShimmedMethod_With_Void_Return_Type()
        {
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("VoidMethod"));
            try
            {
                shimmedMethod.SetReturnValue(5);
            }
            catch(InvalidOperationException e)
            {
                Assert.AreEqual(ShimmedMethod.CannotSetReturnTypeOnVoidMethodError, e.Message);
            }
        }

        [TestMethod] 
        public void ShimmedMethod_SetReturnValue_Excepts_When_Called_On_ShimmedMethod_With_Wrong_Param_Type()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("StaticMethodWithValueReturnType"));
            try
            {
                shimmedMethod.SetReturnValue("bird");
            }
            catch (InvalidOperationException e)
            {
                var expectedString = string.Format(ShimmedMethod.InvalidReturnTypeError, typeof(string), typeof(int));
                Assert.AreEqual(expectedString, e.Message);
            }
        }

        [TestMethod]
        public void ShimmedMethod_Uses_Default_Return_Type_For_Value_Types_When_No_Return_Value_Specified()
        {
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("StaticMethodWithValueReturnType"));
            Assert.AreEqual(default(int), shimmedMethod.ReturnValue);
        }

        [TestMethod]
        public void ShimmedMethod_Uses_Default_For_Reference_Types_With_No_Parameterless_Constructor_When_No_Return_Value_Specified()
        {
            var shimmedMethod = new ShimmedMethod<TestClassNoParameterlessConstructor>(typeof(TestClass).GetMethod("GetTestClassNoParameterlessConstructor"));
            Assert.AreEqual(default(TestClassNoParameterlessConstructor), shimmedMethod.ReturnValue);
        }

        [TestMethod]
        public void ShimmedMethod_Uses_Empty_Object_For_Reference_Types_With_Parameterless_Constructor_When_No_Return_Value_Specified()
        {
            var shimmedMethod = new ShimmedMethod<TestClass>(typeof(TestClass).GetMethod("GetTestClass"));
            Assert.IsNotNull(shimmedMethod.ReturnValue);
        }
    }
}
