using System;
using System.Collections.Generic;

namespace Shimmy.Tests.SharedTestClasses
{
    public static class InstanceMethodsTestClassTracker
    {
        public static InstanceMethodsTestClass LastCreated { get; set; }
    }

    public class InstanceMethodsTestClass
    {
        public Guid InstanceGuid = Guid.NewGuid();

        public InstanceMethodsTestClass()
        {
            InstanceMethodsTestClassTracker.LastCreated = this;
        }

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
}
