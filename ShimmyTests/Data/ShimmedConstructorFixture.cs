using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using Shimmy.Data;
using Shimmy.Tests.SharedTestClasses;
using System;
using System.Linq;

namespace Shimmy.Tests.Data
{
    [TestClass]
    public class ShimmedConstructorFixture
    {
        private class TestClassNoParameterlessConstructor
        {
            public int TestValue; 

            public TestClassNoParameterlessConstructor(int a)
            {
                TestValue = a;
            }
        }

        [TestMethod]
        public void ShimmedConstructor_Returns_New_Instance_If_Parameterless_Constructor_Available()
        {
            var constructorInfo = typeof(InstanceMethodsTestClass).GetConstructor(Type.EmptyTypes);
            var shimmedConstructor = new ShimmedConstructor<InstanceMethodsTestClass>(constructorInfo);
            Assert.IsNotNull(shimmedConstructor);
            Assert.IsNotNull(shimmedConstructor.Constructor);
            Assert.IsNotNull(shimmedConstructor.Member);
            Assert.IsNotNull(shimmedConstructor.Shim);

            InstanceMethodsTestClass a = null;
            PoseContext.Isolate(() =>
            {
                a = new InstanceMethodsTestClass();
            }, shimmedConstructor.Shim);

            Assert.AreEqual(1, shimmedConstructor.CallResults.Count);
            Assert.IsNotNull(shimmedConstructor.CallResults.FirstOrDefault());
            Assert.IsNotNull(a);
        }

        [TestMethod]
        public void ShimmedContructor_Returns_Null_If_No_Parameterless_Constructor()
        {
            var constructorInfo = typeof(TestClassNoParameterlessConstructor).GetConstructor(new [] { typeof(int) });
            var shimmedConstructor = new ShimmedConstructor<TestClassNoParameterlessConstructor>(constructorInfo);
            Assert.IsNotNull(shimmedConstructor);
            Assert.IsNotNull(shimmedConstructor.Constructor);
            Assert.IsNotNull(shimmedConstructor.Member);
            Assert.IsNotNull(shimmedConstructor.Shim);

            TestClassNoParameterlessConstructor a = null;
            PoseContext.Isolate(() =>
            {
                a = new TestClassNoParameterlessConstructor(1);
            }, shimmedConstructor.Shim);

            Assert.AreEqual(1, shimmedConstructor.CallResults.Count);
            Assert.IsNotNull(shimmedConstructor.CallResults.FirstOrDefault());
            Assert.IsNull(a);
        }

        [TestMethod]
        public void ShimmedConstructor_Returns_Custom_Value_If_Set()
        {
            var constructorInfo = typeof(TestClassNoParameterlessConstructor).GetConstructor(new[] { typeof(int) });
            var shimmedConstructor = new ShimmedConstructor<TestClassNoParameterlessConstructor>(constructorInfo);
            var b = new TestClassNoParameterlessConstructor(1);
            shimmedConstructor.ReturnValue = b;

            Assert.IsNotNull(shimmedConstructor);
            Assert.IsNotNull(shimmedConstructor.Constructor);
            Assert.IsNotNull(shimmedConstructor.Member);
            Assert.IsNotNull(shimmedConstructor.Shim);

            TestClassNoParameterlessConstructor a = null;
            PoseContext.Isolate(() =>
            {
                a = new TestClassNoParameterlessConstructor(1);
            }, shimmedConstructor.Shim);

            Assert.AreEqual(1, shimmedConstructor.CallResults.Count);
            Assert.IsNotNull(shimmedConstructor.CallResults.FirstOrDefault());
            Assert.IsNotNull(a);
            Assert.AreEqual(a.TestValue, b.TestValue);
            Assert.AreEqual(a, b);
        }
    }
}
