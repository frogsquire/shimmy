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
        private class TestClass
        {
            public void CallTheSameMethodTwice()
            {
                StaticMethodsTestClass.EmptyMethod();
                StaticMethodsTestClass.EmptyMethod();
            }
        }

        [TestMethod]
        public void PoseWrapper_Creates_One_Shim_Per_Unique_Method_Called_Multiple_Times()
        {
            var wrapper = new PoseWrapper(new TestClass().CallTheSameMethodTwice);
            Assert.AreEqual(1, wrapper._shimmedMethods.Count);
        }
    }
}
