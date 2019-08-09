using Pose.Helpers;
using Shimmy.Helpers;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Shimmy
{
    public static class Shimmer
    {
        public const string ReturnlessWrapperInvalidDelegate 
            = "Cannot generate a returnless PoseWrapper for an entry point with a non-void return type. Use GetPoseWrapper<T> instead.";
        public const string NonMatchingReturnType 
            = "Return type of entry point and generic type parameter must match, or return type was null.";

        // todo: support constructors, getters vs. setters, etc. from here
        // and from the equivalent for GetPoseWrapper<T>
        public static PoseWrapper GetPoseWrapper(Expression<Action> expression, WrapperOptions options = WrapperOptions.None)
        {
            var method = (MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            if (instance is Type)
                return GetPoseWrapper(method, options);
            else
                return GetPoseWrapper(method, instance, options);
        }

        public static PoseWrapper GetPoseWrapper(MethodInfo method, object instance = null, WrapperOptions options = WrapperOptions.None)
        {
            var methodDelegate = GetDelegateFromMethodInfo(method, instance, out Type delegateType);
            return GetPoseWrapper(methodDelegate, delegateType, options);
        }

        public static PoseWrapper GetPoseWrapper(Delegate entryPoint, Type delegateType = null, WrapperOptions options = WrapperOptions.None)
        {
            var returnType = entryPoint.Method.ReturnType;
            var parameters = entryPoint.Method.GetParameters();

            if (returnType == null || returnType != typeof(void))
                throw new ArgumentException(ReturnlessWrapperInvalidDelegate);

            if (parameters.Length == 0)
            {
                return new PoseWrapper(entryPoint, options);
            }

            delegateType = delegateType
                ?? DelegateTypeHelper.GetTypeForDelegate(parameters, returnType);

            return new PoseWrapper(entryPoint, null, delegateType, parameters, options);
        }

        public static PoseWrapper<T> GetPoseWrapper<T>(Expression<Action> expression, WrapperOptions options = WrapperOptions.None)
        {
            var method = (MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            if (instance is Type)
                return GetPoseWrapper<T>(method, null, options);
            else
                return GetPoseWrapper<T>(method, instance, options);
        }

        public static PoseWrapper<T> GetPoseWrapper<T>(MethodInfo method, object instance = null, WrapperOptions options = WrapperOptions.None)
        {
            var methodDelegate = GetDelegateFromMethodInfo(method, instance, out Type delegateType);
            return GetPoseWrapper<T>(methodDelegate, delegateType, options);
        }

        public static PoseWrapper<T> GetPoseWrapper<T>(Delegate entryPoint, Type delegateType = null, WrapperOptions options = WrapperOptions.None)
        {
            var returnType = entryPoint.Method.ReturnType;
            if (returnType == null || returnType != typeof(T))
                throw new ArgumentException(NonMatchingReturnType);

            var parameters = entryPoint.Method.GetParameters();

            delegateType = delegateType 
                ?? DelegateTypeHelper.GetTypeForDelegate(parameters, returnType);

            return new PoseWrapper<T>(entryPoint, returnType, delegateType, parameters, options);
        }

        // todo: get parameters here to further specify method
        private static Delegate GetDelegateFromMethodInfo(MethodInfo method, object instance, out Type delegateType)
        {
            if (!method.IsStatic && instance == null)
                throw new ArgumentException("An instance must be provided for a non-static method.");

            var instanceType = instance?.GetType();

            if (instance != null && instanceType != method.DeclaringType && !instanceType.IsSubclassOf(method.DeclaringType))
                throw new ArgumentException("Provided instance must be an instance or child of " + method.DeclaringType);

            delegateType = DelegateTypeHelper.GetTypeForDelegate(method.GetParameters(), method.ReturnType);

            Delegate methodDelegate;
            if (method.IsStatic)
            {
                methodDelegate = method.CreateDelegate(delegateType);
            }
            else
            {
                methodDelegate = method.CreateDelegate(delegateType, instance);
            }

            return methodDelegate;
        }
    }
}
