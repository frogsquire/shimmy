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
        internal List<ShimmedMethod> _shimmedMethods;

        internal ParameterInfo[] _entryPointParameters;

        /*
         * In PoseWrapper, the entry point is assumed to be an Action.
         * In PoseWrapper<T>, it's assumed to be a function with a return type of T.
         */
        internal Delegate _entryPoint { get; set; }

        public PoseWrapper(Action entryPoint)
        {
            Init(entryPoint);
        }

        public PoseWrapper(Delegate entryPoint)
        {
            Init(entryPoint);
        }

        private void Init(Delegate entryPoint)
        {
            _entryPoint = entryPoint ?? throw new ArgumentException("Cannot convert entryPoint to Action. Did you mean to use PoseWrapper<>?");
            _entryPointParameters = entryPoint.Method.GetParameters();
            GenerateShimmedMethods();
        }

        protected PoseWrapper() { }

        public void Execute(params object[] args)
        {
            Execute(true, args);
        }

        public void Execute(bool clearLastCallResults = true, params object[] args)
        {
            if (clearLastCallResults)
                ClearLastCallResults();

            VerifyArguments(args);

            PoseContext.IsolateExistingDelegate(_entryPoint, typeof(Action), GetShims());
        }

        protected void VerifyArguments(params object[] args)
        {
            if (_entryPointParameters.Length != args.Length)
                throw new ArgumentException("Argument list provided does not match entry point parameters.");

            for (var i = 0; i < _entryPointParameters.Length; i++)
            {
                var relevantArgument = args[i];
                var relevantParameter = _entryPointParameters[i];

                if (relevantArgument == null)
                {
                    if (ParameterCanBeNull(_entryPointParameters[i]))
                        continue;
                    else
                        throw new ArgumentException("Argument " + i + "is null, but parameters of type " + relevantParameter.ParameterType + " cannot be null.");
                }

                var relevantArgumentType = relevantArgument.GetType();
                if (relevantArgument.GetType() != _entryPointParameters[i].GetType())
                    throw new ArgumentException("Argument list is invalid: parameter " + i + " is of type " + relevantArgumentType + "; expected " + relevantParameter.ParameterType);
            }
        }

        // Based on StackOverflow #1770181
        private bool ParameterCanBeNull(ParameterInfo parameter)
        {
            return !parameter.ParameterType.IsValueType || (Nullable.GetUnderlyingType(parameter.ParameterType) != null);
        }

        public void ClearLastCallResults()
        {
            _shimmedMethods.ForEach(sm => sm.CallResults.Clear());
        }

        public Dictionary<MethodInfo, List<ShimmedMethodCall>> LastExecutionResults =>
             _shimmedMethods.ToDictionary(sm => sm.Method, sm => sm.CallResults.ToList());

        private void GenerateShimmedMethods()
        {
            var methods = GetMethodCallsInEntryPoint(_entryPoint);
            _shimmedMethods = methods.Select(m => {
                return GetShimmedMethod(m);
            }).ToList();
        }

        private Shim[] GetShims()
        {
            return _shimmedMethods.Select(sm => sm.Shim).ToArray();
        }

        private static List<MethodInfo> GetMethodCallsInEntryPoint(Delegate entryPoint)
        {
            var instructions = entryPoint.GetMethodInfo().GetInstructions();
            var memberInfos = instructions.Where(i => i.OpCode.OperandType == OperandType.InlineMethod)
                .Select(i => i.Operand as MemberInfo).Distinct();

            // todo: constructors
            return memberInfos.Where(mi => mi.MemberType == MemberTypes.Method)
                .Select(mi => mi as MethodInfo).ToList();
        }

        private ShimmedMethod GetShimmedMethod(MethodInfo m)
        {
            var type = m.ReturnType;

            if (type == typeof(void))
                return new ShimmedMethod(m);

            // todo: pass invoking instance if needed
            var genericShimmedMethod = typeof(ShimmedMethod<>).MakeGenericType(new Type[] { m.ReturnType });
            return (ShimmedMethod)Activator.CreateInstance(genericShimmedMethod, new object[] { m });
        }
    }

    public class PoseWrapper<T> : PoseWrapper
    {
        public PoseWrapper(Delegate entryPoint) : base(entryPoint)
        {
            var returnType = entryPoint.Method.ReturnType;
            if (returnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");
        }

        public new T Execute(params object[] args)
        {
            return Execute(true, args);
        }

        // todo: what if there are params?
        public new T Execute(bool clearLastCallResults = true, params object[] args)
        {
            if (clearLastCallResults)
                ClearLastCallResults();

            VerifyArguments(args);

            T result = default(T); // todo: a way around this?

            // todo
            PoseContext.Isolate(() =>
            {
                result = (T)_entryPoint.DynamicInvoke(args);
            });
            return result;
        }
    }
}
