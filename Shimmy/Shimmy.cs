using Shimmy.Helpers;
using System;

namespace Shimmy
{
    public static class Shimmy
    {
        public static PoseWrapper GenerateReturnlessWrapper(Delegate entryPoint)
        {
            var returnType = entryPoint.Method.ReturnType;
            var parameters = entryPoint.Method.GetParameters();

            if (returnType != typeof(void))
                throw new InvalidOperationException("Cannot generate a returnless PoseWrapper for an entry point with a non-void return type. Use GenerateWrapper<T> instead.");

            if (parameters.Length == 0)
            {
                return new PoseWrapper(entryPoint);
            }

            var delegateType = DelegateTypeHelper.GetTypeForDelegate(parameters);

            return new PoseWrapper(entryPoint, null, delegateType, parameters);
        }

        public static PoseWrapper<T> GenerateWrapper<T>(Delegate entryPoint)
        {
            var returnType = entryPoint.Method.ReturnType;
            if (returnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");

            var parameters = entryPoint.Method.GetParameters();

            var delegateType = DelegateTypeHelper.GetTypeForDelegate(parameters, returnType);

            return new PoseWrapper<T>(entryPoint, returnType, delegateType, parameters);
        }
    }
}
