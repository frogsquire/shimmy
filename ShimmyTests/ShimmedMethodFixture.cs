using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pose;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shimmy.Tests
{
    [TestClass]
    public class ShimmedMethodFixture
    {
        private class TestClass
        {
            public static void EmptyMethod()
            {

            }

            public static void MethodWithParam(int a)
            {
                a = 1;
                return;
            }

            public static void MethodWithMultiParams(int a, int b, string c, List<bool> d)
            {
                throw new NotImplementedException("Intentionally unimplemented!");
            }

            public static int MethodWithReturn()
            {
                return 1;
            }

            public static int MethodWithParamAndReturn(int param1)
            {
                return 1 + param1;
            }

            public static int MethodWithParamsAndReturn(int param1, int param2)
            {
                return 1 + param1 + param2;
            }

            public static List<int> MethodWithParamsAndReferenceTypeReturn(int param1, int param2)
            {
                return new List<int> { param1, param2 };
            }

            public static int MethodWithReferenceTypeParams(List<int> args)
            {
                return args.Count;
            }

        }

        [TestMethod]
        public void ShimmedMethod_Generates_From_Empty_Static_Method_Call()
        {
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("EmptyMethod"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            try
            {
                var beforeDateTime = DateTime.Now;
                PoseContext.Isolate(() => {
                    TestClass.EmptyMethod();
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
            var shimmedMethod = new ShimmedMethod<int>(typeof(TestClass).GetMethod("MethodWithReturn"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            int value = -1;
            PoseContext.Isolate(() => {
                value = TestClass.MethodWithReturn();
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
        public void ShimmedMethod_Generates_From_Static_Call_Records_Parameters()
        {
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("MethodWithParam"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);

            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                TestClass.MethodWithParam(5);
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
        public void ShimmedMethod_Generates_From_Static_Call_Records_Multi_Parameters()
        {
            var shimmedMethod = new ShimmedMethod(typeof(TestClass).GetMethod("MethodWithMultiParams"));
            Assert.IsNotNull(shimmedMethod);
            Assert.IsNotNull(shimmedMethod.Method);
            Assert.IsNotNull(shimmedMethod.Shim);
            
            var beforeDateTime = DateTime.Now;
            PoseContext.Isolate(() => {
                TestClass.MethodWithMultiParams(5, 6, "bird", new List<bool> { true, false, true });
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
    }
}
