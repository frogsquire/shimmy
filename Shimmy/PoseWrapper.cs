using Pose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Reflection;

namespace Shimmy
{
    public class PoseWrapper
    {
        private List<ShimmedMethod> _shimmedMethods;

        private Action _entryPoint { get; set; }

        public PoseWrapper(Action entryPoint)
        {
            _entryPoint = entryPoint ?? throw new ArgumentNullException(nameof(entryPoint));
            GenerateShimmedMethods();
        }

        public Dictionary<MethodInfo, List<ShimmedMethodCall>> Execute()
        {
            PoseContext.Isolate(_entryPoint, GetShims());
            return _shimmedMethods.ToDictionary(sm => sm.Method, sm => sm.CallResults.ToList());
        }

        private void GenerateShimmedMethods()
        {
            var methods = GetMethodCallsInEntryPoint(_entryPoint);
            _shimmedMethods = methods.Select(m => new ShimmedMethod(m)).ToList();
        }

        private Shim[] GetShims()
        {
            return _shimmedMethods.Select(sm => sm.Shim).ToArray();
        }

        private static List<MethodInfo> GetMethodCallsInEntryPoint(Action entryPoint)
        {
            var instructions = entryPoint.GetMethodInfo().GetInstructions();
            var memberInfos = instructions.Where(i => i.OpCode.OperandType == OperandType.InlineMethod)
                .Select(i => i.Operand as MemberInfo);
            // todo: constructors
            return memberInfos.Where(mi => mi.MemberType == MemberTypes.Method)
                .Select(mi => mi as MethodInfo).ToList();
        }
    }
}
