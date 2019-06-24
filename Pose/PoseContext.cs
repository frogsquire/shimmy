using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Pose.IL;

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
            if (entryPoint.Target != null)
            {
                ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke(entryPoint.Target);
            }
            else
            {
                ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke(args);
            }
        }

        public static T IsolateDelegate<T>(Delegate entryPoint, Type delegateType, Shim[] shims, params object[] args)
        {
            var returnType = typeof(T);

            if (entryPoint.Method.ReturnType != returnType)
                throw new InvalidOperationException("Cannot return a type of " + returnType + " when specified method expects " + entryPoint.Method.ReturnType + ".");

            if (shims == null || shims.Length == 0)
            {
                return (T)entryPoint.DynamicInvoke();
            }

            Shims = shims;
            StubCache = new Dictionary<MethodBase, DynamicMethod>();

            MethodRewriter rewriter = MethodRewriter.CreateRewriter(entryPoint.Method);
            if (entryPoint.Target != null)
            {
                return (T)((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke(entryPoint.Target);
            }
            else
            {
                return (T)((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke(args);
            }
        }
    }
}