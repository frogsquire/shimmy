using Pose.IL;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Pose
{
    public static class PoseContext
    {
        internal static Shim[] Shims { private set; get; }
        internal static Dictionary<MethodBase, DynamicMethod> StubCache { private set; get; }

        public static void Isolate(Action entryPoint, params Shim[] shims)
        {
            if (shims == null || shims.Length == 0)
            {
                entryPoint.DynamicInvoke();
                return;
            }

            Type delegateType = typeof(Action<>).MakeGenericType(entryPoint.Target.GetType());
            IsolateDelegate(entryPoint, delegateType, shims);
        }

        public static void IsolateDelegate(Delegate entryPoint, Type delegateType, Shim[] shims, params object[] args)
        {
            if (shims == null || shims.Length == 0)
            {
                entryPoint.DynamicInvoke();
                return;
            }

            Shims = shims;
            StubCache = new Dictionary<MethodBase, DynamicMethod>();

            MethodRewriter rewriter = MethodRewriter.CreateRewriter(entryPoint.Method);
            var delToInvoke = ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType);

            if (IsActionDelegateWithoutEntryPointFirstParam(delToInvoke, entryPoint.Method)
                || entryPoint.Target == null)
            {
                delToInvoke.DynamicInvoke(args);
            }
            else
            {
                delToInvoke.DynamicInvoke(entryPoint.Target);
            }
        }

        public static T IsolateDelegate<T>(Delegate entryPoint, Type delegateType, Shim[] shims, params object[] args)
        {
            var returnType = typeof(T);

            if (entryPoint.Method.ReturnType != returnType)
            {
                throw new InvalidOperationException("Cannot return a type of " + returnType + " when specified method expects " + entryPoint.Method.ReturnType + ".");
            }

            if (shims == null || shims.Length == 0)
            {
                return (T)entryPoint.DynamicInvoke(args);
            }

            Shims = shims;
            StubCache = new Dictionary<MethodBase, DynamicMethod>();

            MethodRewriter rewriter = MethodRewriter.CreateRewriter(entryPoint.Method);
            var delToInvoke = ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType, entryPoint.Target);

            return (T)delToInvoke.DynamicInvoke(args);                     
        }

        // todo: move this?
        private static readonly Type[] _actionTypes = new[] {
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>),
            typeof(Action<,,,,,,,,>),
            typeof(Action<,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,,>)
        };

        // todo: determine this upstream - figure out when such a delegate is created
        private static bool IsActionDelegateWithoutEntryPointFirstParam(Delegate candidate, MethodInfo originalMethod)
        {
            if (candidate == null)
                return false;

            var type = candidate.GetType();

            if (type == typeof(Action))
                return true;

            var isGenericAction = false;
            if (type.IsGenericType)
                isGenericAction = Array.IndexOf(_actionTypes, type.GetGenericTypeDefinition()) >= 0;

            if (!isGenericAction)
                return false;

            var actionParameters = type.GetGenericArguments();
            return actionParameters == null
                || actionParameters.Length == 0
                || actionParameters[0] != originalMethod.DeclaringType;
        }
    }
}