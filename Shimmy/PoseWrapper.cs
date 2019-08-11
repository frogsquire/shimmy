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

        internal HashSet<ShimmedMember> ShimmedMembers;

        internal ParameterInfo[] EntryPointParameters;

        /*
         * In PoseWrapper, the entry point is assumed to be an Action.
         * In PoseWrapper<T>, it's assumed to be a function with a return type of T.
         */
        internal Delegate EntryPoint;

        internal Type EntryPointType;

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
            EntryPoint = entryPoint ?? throw new ArgumentException("Cannot convert entryPoint to Action. Did you mean to use PoseWrapper<>?");
            EntryPointParameters = entryPointParameters ?? entryPoint.Method.GetParameters();
            EntryPointType = entryPointType
                ?? DelegateTypeHelper.GetTypeForDelegate(EntryPointParameters.Select(epp => epp.ParameterType).ToArray(), returnType);
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

            PoseContext.IsolateDelegate(EntryPoint, EntryPointType, GetShims(), args);
        }

        protected void VerifyArguments(params object[] args)
        {
            if (EntryPointParameters.Length != args.Length)
                throw new ArgumentException("Argument list provided does not match entry point parameters.");

            for (var i = 0; i < EntryPointParameters.Length; i++)
            {
                var relevantArgument = args[i];
                var relevantParameter = EntryPointParameters[i];

                if (relevantArgument == null)
                {
                    if (ParameterCanBeNull(EntryPointParameters[i]))
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
            ShimmedMembers.ToList().ForEach(sm => sm.CallResults.Clear());
        }

        public Dictionary<MemberInfo, List<ShimmedMemberCall>> LastExecutionResults =>
             ShimmedMembers.ToDictionary(sm => sm.Member, sm => sm.CallResults.ToList());

        public List<ShimmedMemberCall> ResultsFor(Expression<Action> expression) =>
            ResultsFor(MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance));

        public List<ShimmedMemberCall> ResultsFor<T>(Expression<Func<T>> expression) =>
            ResultsFor(MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance));

        public List<ShimmedMemberCall> ResultsFor(string memberName) => ParseMemberFromString(memberName)?.CallResults;

        public List<ShimmedMemberCall> ResultsFor(MemberInfo member) =>
            ShimmedMembers.FirstOrDefault(sm => sm.Member == member)?.CallResults;

        private void GenerateShimmedMethods()
        {
            var callInstructions = EntryPoint.GetMethodInfo().GetInstructions()
                .Where(i => i.OpCode.OperandType == OperandType.InlineMethod);

            var methods = GetMethodCallsFromInstructions(callInstructions);

            ShimmedMembers = new HashSet<ShimmedMember>();
            foreach (var method in methods)
            {
                ShimmedMembers.Add(GetShimmedMethod(method));
            }

            if (Options.HasFlag(WrapperOptions.ShimConstructors))
            {
                var constructors = new List<MemberInfo>();
                constructors.AddRange(GetConstructorsFromInstructions(callInstructions));

                foreach (var constructor in constructors)
                {
                    ShimmedMembers.Add(GetShimmedConstructor(constructor, constructor.DeclaringType));
                }
            }
        }

        protected Shim[] GetShims()
        {
            return ShimmedMembers.Select(sm => sm.Shim).ToArray();
        }

        private List<MethodInfo> GetMethodCallsFromInstructions(IEnumerable<Instruction> callInstructions, List<MethodInfo> methodCallsToShim = null)
        {
            // todo: support constructors (which are memberinfos, not methodinfos)
            var methodInfos = callInstructions.Select(i => i.Operand as MethodInfo).Distinct();

            if (methodCallsToShim == null)
                methodCallsToShim = new List<MethodInfo>();

            foreach (var method in methodInfos)
            {
                if (methodCallsToShim.Contains(method) || method == null)
                    continue;

                if (!Exceptions.Contains(method)
                    && (Options.HasFlag(WrapperOptions.ShimPrivateMembers) || !method.IsPrivate)
                    && (Options.HasFlag(WrapperOptions.ShimSpecialNames) || !method.IsSpecialName))
                    methodCallsToShim.Add(method);
                else if (!Options.HasFlag(WrapperOptions.ShimPrivateMembers) && method.IsPrivate)
                    methodCallsToShim = GetMethodCallsFromInstructions(method.GetInstructions(), methodCallsToShim);                
            }

            return methodCallsToShim;
        }

        private List<MemberInfo> GetConstructorsFromInstructions(IEnumerable<Instruction> callInstructions)
            => callInstructions.Select(i => i.Operand as MemberInfo).Where(mi => mi.MemberType == MemberTypes.Constructor).Distinct().ToList();

        private ShimmedMethod GetShimmedMethod(MethodInfo m)
        {
            // if a shim already exists for this method, don't create a duplicate            
            var existingShim = ShimmedMembers.Select(sm => sm as ShimmedMethod).FirstOrDefault(sm => sm.Member == m);
            if (existingShim != null)
                return existingShim;

            // build the shim with appropriate return type
            var type = m.ReturnType;

            if (type == typeof(void))
                return new ShimmedMethod(m);

            var genericShimmedMethod = typeof(ShimmedMethod<>).MakeGenericType(new Type[] { m.ReturnType });
            return (ShimmedMethod)Activator.CreateInstance(genericShimmedMethod, new object[] { m });
        }

        private ShimmedMember GetShimmedConstructor<T>(MemberInfo m, T objectType)
        {
            var existingShim = ShimmedMembers.FirstOrDefault(sc => sc.Member.Equals(m));
            if (existingShim != null)
                return (ShimmedConstructor<T>)existingShim;

            var genericShimmedConstructor = typeof(ShimmedConstructor<>).MakeGenericType(new Type[] { m.DeclaringType });

            // cast back to ShimmedMember because there's no way to cast to ShimmedConstructor<T>
            return (ShimmedMember)Activator.CreateInstance(genericShimmedConstructor, new object[] { m });
        }

        public void SetReturn(Expression<Action> expression, object value)
        {
            var memberInfo = MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            SetReturn(memberInfo, value);
        }

        public void SetReturn<T>(Expression<Func<T>> expression, object value)
        {
            var memberInfo = MethodHelper.GetMethodFromExpression(expression.Body, false, out object instance);
            SetReturn(memberInfo, value);
        }

        public void SetReturn(string memberName, object value)
        {
            var shimmedMethod = ParseMemberFromString(memberName);

            if (shimmedMethod == null)
                throw new InvalidOperationException(string.Format(CouldNotFindMatchingShimError, memberName));

            shimmedMethod.SetReturnValue(value);
        }

        public void SetReturn(MemberInfo member, object value)
        {
            var shimmedMethod = ShimmedMembers.FirstOrDefault(sm => sm.Member.Equals(member));

            if (shimmedMethod == null)
                throw new InvalidOperationException(string.Format(CouldNotFindMatchingShimError, member));

            shimmedMethod.SetReturnValue(value);
        }

        /*
         * Accepts methods in the form:
         *  "methodName"
         *  "className.methodName"
         */
        private ShimmedMember ParseMemberFromString(string memberName)
        {
            var className = string.Empty;
            if (memberName.Contains("."))
            {
                var splitMemberName = memberName.Split('.');
                className = splitMemberName[0];
                memberName = splitMemberName[1];
            }
            return ShimmedMembers.FirstOrDefault(sm => sm.Member.Name.Equals(memberName)
                    && (string.IsNullOrEmpty(className) || sm.Member.DeclaringType.Name.Equals(className)));            
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

            return PoseContext.IsolateDelegate<T>(EntryPoint, EntryPointType, GetShims(), args);
        }
    }
}
