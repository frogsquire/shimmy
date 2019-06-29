using Pose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Reflection;
using Shimmy.Data;
using Shimmy.Helpers;

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
        internal Delegate _entryPoint;

        internal Type _entryPointType; 

        public PoseWrapper(Action entryPoint)
        {
            Init(entryPoint);
        }

        public PoseWrapper(Delegate entryPoint)
        {
            Init(entryPoint);
        }

        public PoseWrapper(Delegate entryPoint, Type returnType = null, Type entryPointType = null, ParameterInfo[] entryPointParameters = null)
        {
            Init(entryPoint, returnType, entryPointType, entryPointParameters);
        }        

        private void Init(Delegate entryPoint, Type returnType = null, Type entryPointType = null, ParameterInfo[] entryPointParameters = null)
        {
            _entryPoint = entryPoint ?? throw new ArgumentException("Cannot convert entryPoint to Action. Did you mean to use PoseWrapper<>?");
            _entryPointParameters = entryPointParameters ?? entryPoint.Method.GetParameters();
            _entryPointType = entryPointType 
                ?? DelegateTypeHelper.GetTypeForDelegate(_entryPointParameters.Select(epp => epp.ParameterType).ToArray(), returnType);
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

            PoseContext.IsolateDelegate(_entryPoint, _entryPointType, GetShims(), args);
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
                if (relevantArgument.GetType() != relevantParameter.ParameterType)
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
            _shimmedMethods = new List<ShimmedMethod>();
            foreach(var method in methods)
            {
                _shimmedMethods.Add(GetShimmedMethod(method));
            }
        }

        protected Shim[] GetShims()
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
            // if a shim already exists for this method, don't create a duplicate            
            var existingShim = _shimmedMethods.FirstOrDefault(sm => sm.Method == m);
            if (existingShim != null)
                return existingShim;

            // build the shim with appropriate return type
            var type = m.ReturnType;

            if (type == typeof(void))
                return new ShimmedMethod(m);

            var genericShimmedMethod = typeof(ShimmedMethod<>).MakeGenericType(new Type[] { m.ReturnType });
            return (ShimmedMethod)Activator.CreateInstance(genericShimmedMethod, new object[] { m });
        }
    }

    public class PoseWrapper<T> : PoseWrapper
    {
        public PoseWrapper(Delegate entryPoint, Type entryPointType = null) : base(entryPoint, entryPoint.Method.ReturnType, entryPointType)
        {
            if (entryPoint.Method.ReturnType == null || entryPoint.Method.ReturnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");

        }

        public PoseWrapper(Delegate entryPoint, Type returnType = null, Type entryPointType = null, ParameterInfo[] entryPointParameters = null) : base(entryPoint, returnType, entryPointType, entryPointParameters)
        {
            if (returnType == null || returnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");
        }


        public new T Execute(params object[] args)
        {
            return Execute(true, args);
        }

        public new T Execute(bool clearLastCallResults = true, params object[] args)
        {
            if (clearLastCallResults)
                ClearLastCallResults();

            VerifyArguments(args);

            return PoseContext.IsolateDelegate<T>(_entryPoint, _entryPointType, GetShims(), args);
        }
    }
}
