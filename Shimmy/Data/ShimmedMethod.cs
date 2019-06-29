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
        private Guid _libraryReferenceGuid;
        protected ParameterExpression[] _expressionParameters;
        protected Type DeclaringType;

        public const int MaximumPoseParameters = 10; 

        public MethodInfo Method { get; private set; }

        public List<ShimmedMethodCall> CallResults { get; private set; }

        public Shim Shim { get; private set; }

        public ShimmedMethod(MethodInfo method)
        {
            Init(method);
        }

        protected ShimmedMethod() { }

        protected void Init(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            _expressionParameters = GenerateExpressionParameters();
            _libraryReferenceGuid = ShimmedMethodLibrary.Add(this);
            DeclaringType = method.DeclaringType;

            Shim = GenerateShim();
            CallResults = new List<ShimmedMethodCall>();
        }

        protected virtual Shim GenerateShim()
        {
            return Shim.Replace(GenerateCallExpression()).With(GetShimAction());
        }

        protected Expression<Action> GenerateCallExpression()
        {
            Expression poseIsACall = null;
            if (!Method.IsStatic)
            {
                var poseIsAMethod = typeof(Pose.Is).GetMethod("A").MakeGenericMethod(new[] { DeclaringType });
                poseIsACall = Expression.Call(null, poseIsAMethod);
            }
            return Expression.Lambda<Action>(Expression.Call(poseIsACall, Method, _expressionParameters));
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
            if (Method.IsStatic)
            {
                paramTypesArray = expressionParamsArray;
            }
            else
            {
                var invokingTypeArray = new Type[] { DeclaringType };
                paramTypesArray = new Type[expressionParamsArray.Length + 1];
                invokingTypeArray.CopyTo(paramTypesArray, 0);
                expressionParamsArray.CopyTo(paramTypesArray, 1);
            }

            var dynamicMethod = new DynamicMethod("shimmy_" + Method.Name, 
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                Method.ReturnType, 
                paramTypesArray, 
                DeclaringType,
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

        public ShimmedMethod(MethodInfo method) : base(method)
        {
        }

        public ShimmedMethod(MethodInfo method, T returnValue) : base()
        {
            ReturnValue = returnValue;

            // init must be second so method generation will check for return value
            Init(method);
        }

        protected override Shim GenerateShim()
        {
            return Shim.Replace(GenerateCallExpression()).With(GetShimActionWithReturn());
        }

        private Delegate GetShimActionWithReturn()
        {
            if(!_expressionParameters.Any() && Method.IsStatic && !HasCustomReturnValue)
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
