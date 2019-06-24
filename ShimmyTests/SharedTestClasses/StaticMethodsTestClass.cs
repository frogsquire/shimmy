using System;
using System.Collections.Generic;
using System.Text;

namespace Shimmy.Tests.SharedTestClasses
{
    public class StaticMethodsTestClass
    {
        public static void EmptyMethod()
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static void MethodWithValueTypeParam(int a)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static void MethodWithStringParam(string b)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static void MethodWithObjectParam(List<bool> l)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static void MethodWithMultiParams(int a, int b, string c, List<bool> d)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static int MethodWithReturn()
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static int MethodWithParamAndReturn(int param1)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static int MethodWithParamsAndReturn(int param1, int param2)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static List<int> MethodWithParamsAndReferenceTypeReturn(int param1, int param2)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static List<int> MethodWithReferenceTypeParamsAndReturn(List<int> args)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }

        public static List<int> MethodWithMultiReferenceTypeParamsAndReturn(List<int> a, string b, DateTime c)
        {
            throw new NotImplementedException("Intentionally unimplemented!");
        }
    }
}
