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
            IsolateExistingDelegate(entryPoint, delegateType, shims);
        }

        public static void IsolateExistingDelegate(Delegate entryPoint, Type delegateType, params Shim[] shims)
        {
            if (shims == null || shims.Length == 0)
            {
                entryPoint.DynamicInvoke();
                return;
            }

            RewriteAndExecute(entryPoint, delegateType, shims);
        }

        private static void RewriteAndExecute(Delegate entryPoint, Type delegateType, params Shim[] shims)
        {
            Shims = shims;
            StubCache = new Dictionary<MethodBase, DynamicMethod>();        

            MethodRewriter rewriter = MethodRewriter.CreateRewriter(entryPoint.Method);
            if (entryPoint.Target != null)
            {
                ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke(entryPoint.Target);
            }
            else
            {
                ((MethodInfo)(rewriter.Rewrite())).CreateDelegate(delegateType).DynamicInvoke();
            }
        }        
    }
}