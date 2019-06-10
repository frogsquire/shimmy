using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using Pose.Extensions;

namespace Pose.Helpers
{
    internal static class StubHelper
    {
        // this class is the true type of a Shimmy dynamic method, but it's internal
        private const string RTDynamicMethodType = "System.Reflection.Emit.DynamicMethod+RTDynamicMethod";

        public static IntPtr GetMethodPointer(MethodBase methodBase)
        {
            if (methodBase is DynamicMethod)
            {
                return GetDynamicMethodFunctionPointer(methodBase as DynamicMethod);
            }

            if (MethodBaseIsShimmyDynamic(methodBase))
            {
                var dynamicMethod = GetDynamicMethodFromDynamicMethodBase(methodBase);
                return GetDynamicMethodFunctionPointer(dynamicMethod);
            }

            return methodBase.MethodHandle.GetFunctionPointer();
        }

        public static DynamicMethod GetDynamicMethodFromDynamicMethodBase(MethodBase methodBase)
        {
            var fieldInfo = methodBase.GetType().GetField("m_owner", BindingFlags.NonPublic | BindingFlags.Instance);
            return ((DynamicMethod)fieldInfo.GetValue(methodBase));            
        }

        // Workaround for Shimmy's dynamic methods, whose methodbases do not cast back to dynamicmethod cleanly
        // (because they're sent to Pose as delegates?)
        public static bool MethodBaseIsShimmyDynamic(MethodBase methodBase)
        {
            return methodBase.GetType().ToString().Equals(RTDynamicMethodType);
        }

        public static IntPtr GetDynamicMethodFunctionPointer(DynamicMethod methodBase)
        {
            var methodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.Instance | BindingFlags.NonPublic);
            return ((RuntimeMethodHandle)methodDescriptor.Invoke(methodBase, null)).GetFunctionPointer();
        }

        public static object GetShimDelegateTarget(int index)
            => PoseContext.Shims[index].Replacement.Target;

        public static MethodInfo GetShimReplacementMethod(int index)
            => PoseContext.Shims[index].Replacement.Method;

        public static int GetIndexOfMatchingShim(MethodBase methodBase, Type type, object obj)
        {
            if (methodBase.IsStatic || obj == null)
                return Array.FindIndex(PoseContext.Shims, s => s.Original == methodBase);

            int index = Array.FindIndex(PoseContext.Shims,
                s => Object.ReferenceEquals(obj, s.Instance) && s.Original == methodBase);

            if (index == -1)
                return Array.FindIndex(PoseContext.Shims,
                            s => SignatureEquals(s, type, methodBase) && s.Instance == null);

            return index;
        }

        public static int GetIndexOfMatchingShim(MethodBase methodBase, object obj)
            => GetIndexOfMatchingShim(methodBase, methodBase.DeclaringType, obj);

        public static MethodInfo GetRuntimeMethodForVirtual(Type type, MethodInfo methodInfo)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | (methodInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);
            Type[] types = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            return type.GetMethod(methodInfo.Name, bindingFlags, null, types, null);
        }

        public static Module GetOwningModule() => typeof(StubHelper).Module;

        private static bool SignatureEquals(Shim shim, Type type, MethodBase method)
        {
            if (shim.Type == null || type == shim.Type)
                return $"{shim.Type}::{shim.Original.ToString()}" == $"{type}::{method.ToString()}";

            if (type.IsSubclassOf(shim.Type))
            {
                if ((shim.Original.IsAbstract || !shim.Original.IsVirtual)
                        || (shim.Original.IsVirtual && !method.IsOverride()))
                {
                    return $"{shim.Original.ToString()}" == $"{method.ToString()}";
                }
            }

            return false;
        }
    }
}