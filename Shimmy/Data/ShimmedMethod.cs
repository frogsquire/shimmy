using Pose;
using Shimmy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shimmy.Data
{
    internal class ShimmedMethod
    {
        public const string InvokingInstanceNotProvidedMessage = "Cannot generate shim - must provide an invoking instance for non-static members.";

        private Guid _libraryReferenceGuid;
        protected ParameterExpression[] _expressionParameters;

        protected object InvokingInstance;

        public const int MaximumPoseParameters = 10; 

        public MethodInfo Method { get; private set; }

        public List<ShimmedMethodCall> CallResults { get; private set; }

        public Shim Shim { get; private set; }

        public ShimmedMethod(MethodInfo method, object invokingInstance = null)
        {
            Init(method, invokingInstance);
        }

        protected ShimmedMethod() { }

        protected void Init(MethodInfo method, object invokingInstance)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            _expressionParameters = GenerateExpressionParameters();
            _libraryReferenceGuid = ShimmedMethodLibrary.Add(this);
            InvokingInstance = invokingInstance;

            Shim = GenerateShim();
            CallResults = new List<ShimmedMethodCall>();
        }

        protected virtual Shim GenerateShim()
        {
            if (!Method.IsStatic && InvokingInstance == null)
            {
                throw new InvalidOperationException(InvokingInstanceNotProvidedMessage);
            }

            return Shim.Replace(GenerateCallExpression()).With(GetShimAction());
        }

        protected Expression<Action> GenerateCallExpression()
        {
            var constant = InvokingInstance == null ? null : Expression.Constant(InvokingInstance);
            return Expression.Lambda<Action>(Expression.Call(constant, Method, _expressionParameters));
        }

        private Delegate GetShimAction()
        {
            // if it's not necessary, don't build a dynamic method
            // this shortcut bypasses the ShimmedMethodLibrary
            // and the overhead of a dynamicmethod
            // while still logging the call
            if (!_expressionParameters.Any() && Method.IsStatic)
                return (Action)(() => AddCallResult());

            return GenerateDynamicShim();
        }

        protected Delegate GenerateDynamicShim(Type returnType = null)
        {
            var expressionParamsArray = _expressionParameters.Select(p => p.Type).ToArray();

            Type[] paramTypesArray;
            if (InvokingInstance == null)
            {
                paramTypesArray = expressionParamsArray;
            }
            else
            {
                var invokingTypeArray = new Type[] { InvokingInstance.GetType() };
                paramTypesArray = new Type[expressionParamsArray.Length + 1];
                invokingTypeArray.CopyTo(paramTypesArray, 0);
                expressionParamsArray.CopyTo(paramTypesArray, 1);
            }

            var owner = InvokingInstance == null ? typeof(ShimmedMethod) : InvokingInstance.GetType();

            var dynamicMethod = new DynamicMethod("shimmy_" + Method.Name, 
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                Method.ReturnType, 
                paramTypesArray, 
                owner,
                false);

            var ilGenerator = dynamicMethod.GetILGenerator();
            var returnLabel = ilGenerator.DefineLabel();

            var arrayLocal = ilGenerator.DeclareLocal(typeof(object[]));

            // mark the current shimmed method as the one to send call results
            // todo: for now, guid is loaded as a string for convenience of not having to pointerize
            ilGenerator.Emit(OpCodes.Ldstr, _libraryReferenceGuid.ToString());
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimmedMethodLibrary).GetMethod("SetRunningMethod"), null);

            // create a new object array of necessary length
            ilGenerator.Emit(OpCodes.Ldc_I4, paramTypesArray.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            ilGenerator.Emit(OpCodes.Stloc, arrayLocal);

            // load each parameter into the object array
            for (int i = 0; i < paramTypesArray.Length; i++)
            {
                // set current array index
                ilGenerator.Emit(OpCodes.Ldloc, arrayLocal);
                ilGenerator.Emit(OpCodes.Ldc_I4, i);

                // load the parameter
                ilGenerator.Emit(OpCodes.Ldarg, i);

                // if this is a value type, box it
                if (paramTypesArray[i].IsValueType)
                    ilGenerator.Emit(OpCodes.Box, paramTypesArray[i]);

                // save the element into the array
                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }

            // call the method which will save these parameters
            ilGenerator.Emit(OpCodes.Ldloc, arrayLocal);
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimmedMethodLibrary).GetMethod("AddCallResultToShim"), null);

            // return - with default return value if necessary
            // provided via a call so the stack will accomodate it
            if (returnType == null)
            {
                ilGenerator.EmitCall(OpCodes.Call, typeof(ShimmedMethodLibrary).GetMethod("ClearRunningMethod"), null);
            }
            else
            {
                var method = typeof(ShimmedMethodLibrary).GetMethod("GetReturnValueAndClearRunningMethod").MakeGenericMethod(new Type[] { returnType });
                ilGenerator.EmitCall(OpCodes.Call, method, null);
            }

            ilGenerator.MarkLabel(returnLabel);
            ilGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(DelegateTypeHelper.GetTypeForDelegate(paramTypesArray, returnType));
        }

        protected ParameterExpression[] GenerateExpressionParameters()
        {
            var parameters = Method.GetParameters();

            if (parameters.Length > MaximumPoseParameters)
                throw new ArgumentException("Method " + Method.Name + " has " + parameters.Length
                    + " parameters. Pose only supports methods with " + MaximumPoseParameters + " parameters or fewer.");

            var expressionParameters = new ParameterExpression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                expressionParameters[i] =
                    Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
            }

            return expressionParameters;
        }

        protected void AddCallResult() => AddCallResultWithParams(new object[] { });

        protected void AddCallResultWithParams(params object[] parameters) => CallResults.Add(new ShimmedMethodCall(parameters));

    }

    internal class ShimmedMethod<T> : ShimmedMethod
    {
        public T ReturnValue { get; set; }

        public bool HasCustomReturnValue => !EqualityComparer<T>.Default.Equals(ReturnValue, default(T));

        public ShimmedMethod(MethodInfo method, object invokingInstance = null) : base(method, invokingInstance)
        {
        }

        public ShimmedMethod(MethodInfo method, T returnValue, object invokingInstance = null) : base()
        {
            ReturnValue = returnValue;

            // init must be second so method generation will check for return value
            Init(method, invokingInstance);
        }

        protected override Shim GenerateShim()
        {
            if (!Method.IsStatic && InvokingInstance == null)
            {
                throw new InvalidOperationException(InvokingInstanceNotProvidedMessage);
            }

            return Shim.Replace(GenerateCallExpression()).With(GetShimActionWithReturn());
        }

        private Delegate GetShimActionWithReturn()
        {
            if(!_expressionParameters.Any() && InvokingInstance == null && !HasCustomReturnValue)
                return (Func<T>)(() => LogAndReturnDefault());

            return GenerateDynamicShim(typeof(T));
        }

        private T LogAndReturnDefault()
        {
            AddCallResult();
            return default(T);
        }
    }
}
