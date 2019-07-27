using Mono.Reflection;
using Pose;
using Pose.Helpers;
using Shimmy.Data;
using Shimmy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shimmy
{
    public class PoseWrapper
    {
        public const string CouldNotFindMatchingShimError = "No shim matching {0} could be found.";

        public WrapperOptions Options { get; private set; }

        internal HashSet<ShimmedMethod> _shimmedMethods;

        internal ParameterInfo[] _entryPointParameters;

        /*
         * In PoseWrapper, the entry point is assumed to be an Action.
         * In PoseWrapper<T>, it's assumed to be a function with a return type of T.
         */
        internal Delegate _entryPoint;

        internal Type _entryPointType;

        public PoseWrapper(Action entryPoint)
        {
            Init(entryPoint, WrapperOptions.None);
        }

        public PoseWrapper(Action entryPoint, WrapperOptions options)
        {
            Init(entryPoint, options);
        }

        public PoseWrapper(Delegate entryPoint)
        {
            Init(entryPoint, WrapperOptions.None);
        }

        public PoseWrapper(Delegate entryPoint, WrapperOptions options)
        {
            Init(entryPoint, options);
        }

        public PoseWrapper(Delegate entryPoint, Type returnType = null, Type entryPointType = null, ParameterInfo[] entryPointParameters = null, WrapperOptions options = WrapperOptions.None)
        {
            Init(entryPoint, options, returnType, entryPointType, entryPointParameters);
        }

        private void Init(Delegate entryPoint, WrapperOptions options, Type returnType = null, Type entryPointType = null, ParameterInfo[] entryPointParameters = null)
        {
            Options = options;
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
            _shimmedMethods.ToList().ForEach(sm => sm.CallResults.Clear());
        }

        public Dictionary<MethodInfo, List<ShimmedMethodCall>> LastExecutionResults =>
             _shimmedMethods.ToDictionary(sm => sm.Method, sm => sm.CallResults.ToList());

        public List<ShimmedMethodCall> ResultsFor(Expression<Action> expression) =>
            ResultsFor((MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance));

        public List<ShimmedMethodCall> ResultsFor<T>(Expression<Func<T>> expression) =>
            ResultsFor((MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance));

        public List<ShimmedMethodCall> ResultsFor(string methodName) => ParseMethodFromString(methodName)?.CallResults;

        public List<ShimmedMethodCall> ResultsFor(MethodInfo method) =>
            _shimmedMethods.FirstOrDefault(sm => sm.Method == method)?.CallResults;

        private void GenerateShimmedMethods()
        {
            var methods = GetMethodCallsInEntryPoint(_entryPoint);
            _shimmedMethods = new HashSet<ShimmedMethod>();
            foreach (var method in methods)
            {
                _shimmedMethods.Add(GetShimmedMethod(method));
            }
        }

        protected Shim[] GetShims()
        {
            return _shimmedMethods.Select(sm => sm.Shim).ToArray();
        }

        private List<MethodInfo> GetMethodCallsInEntryPoint(Delegate entryPoint)
        {
            var instructions = entryPoint.GetMethodInfo().GetInstructions();
            var memberInfos = instructions.Where(i => i.OpCode.OperandType == OperandType.InlineMethod)
                .Select(i => i.Operand as MemberInfo).Distinct();

            // todo: constructors
            return memberInfos.Where(mi => mi.MemberType == MemberTypes.Method)
                .Select(mi => mi as MethodInfo)
                .Where(mi => !Exceptions.Contains(mi)
                            && (Options.HasFlag(WrapperOptions.ShimSpecialNames) || !mi.IsSpecialName)
                            && (Options.HasFlag(WrapperOptions.ShimPrivateMembers) || !mi.IsPrivate))
                .ToList();
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

        public void SetReturn(Expression<Action> expression, object value)
        {
            var methodInfo = (MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            SetReturn(methodInfo, value);
        }

        public void SetReturn<T>(Expression<Func<T>> expression, object value)
        {
            var methodInfo = (MethodInfo)MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            SetReturn(methodInfo, value);
        }

        public void SetReturn(string methodName, object value)
        {
            var shimmedMethod = ParseMethodFromString(methodName);

            if (shimmedMethod == null)
                throw new InvalidOperationException(string.Format(CouldNotFindMatchingShimError, methodName));

            shimmedMethod.SetReturnValue(value);
        }

        public void SetReturn(MethodInfo method, object value)
        {
            var shimmedMethod = _shimmedMethods.FirstOrDefault(sm => sm.Method.Equals(method));

            if (shimmedMethod == null)
                throw new InvalidOperationException(string.Format(CouldNotFindMatchingShimError, method));

            shimmedMethod.SetReturnValue(value);
        }

        /*
         * Accepts methods in the form:
         *  "methodName"
         *  "className.methodName"
         */
        private ShimmedMethod ParseMethodFromString(string methodName)
        {
            var className = string.Empty;
            if (methodName.Contains("."))
            {
                var splitMethodName = methodName.Split('.');
                className = splitMethodName[0];
                methodName = splitMethodName[1];
            }
            return _shimmedMethods.FirstOrDefault(sm => sm.Method.Name.Equals(methodName)
                    && (string.IsNullOrEmpty(className) || sm.Method.DeclaringType.Name.Equals(className)));            
        }

        // never shim string.concat(); it breaks the + operator
        // todo: are there other methods in a similar situation?
        // todo: what if someone wishes to override this?
        private static List<MethodInfo> MethodsToNeverShim =>
            typeof(string).GetMethods().Where(m => m.Name.Equals("Concat")).ToList();

        // todo: add custom exceptions here
        private static List<MethodInfo> Exceptions => MethodsToNeverShim;
    }

    public class PoseWrapper<T> : PoseWrapper
    {
        public PoseWrapper(Delegate entryPoint)
            : base(entryPoint, 
                  entryPoint.Method.ReturnType, 
                  DelegateTypeHelper.GetTypeForDelegate(entryPoint.Method.GetParameters(), entryPoint.Method.ReturnType),
                  options: WrapperOptions.None)
        {
            if (entryPoint.Method.ReturnType == null || entryPoint.Method.ReturnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");
        }

        public PoseWrapper(Delegate entryPoint, Type entryPointType, WrapperOptions options = WrapperOptions.None) 
            : base(entryPoint, entryPoint.Method.ReturnType, entryPointType, options: options)
        {
            if (entryPoint.Method.ReturnType == null || entryPoint.Method.ReturnType != typeof(T))
                throw new ArgumentException("Return type of entry point and generic type must match.");
        }

        public PoseWrapper(Delegate entryPoint, Type returnType = null, Type entryPointType = null, 
                ParameterInfo[] entryPointParameters = null, WrapperOptions options = WrapperOptions.None) 
            : base(entryPoint, returnType, entryPointType, entryPointParameters, options)
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
