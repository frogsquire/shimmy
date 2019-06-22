using Pose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shimmy
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

            // unmark the current shim - no longer active
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimmedMethodLibrary).GetMethod("ClearRunningMethod"), null);

            // return - with default return value if necessary
            // provided via a call so the stack will accomodate it (?)
            if (returnType != null)
            {
                // if it's a value type, or an object with parameters in the constructor
                // return the default behavior (using Pose.Is.A)
                // todo: make this configurable behavior
                // todo: investigate circular reference issue in object with params in constructor
                // todo: add tests for this case
                if (returnType.IsValueType || returnType.GetConstructor(Type.EmptyTypes) == null)
                {
                    var isAMethod = typeof(Is).GetMethod("A");
                    var genericIsAMethod = isAMethod.MakeGenericMethod(new[] { returnType });
                    ilGenerator.EmitCall(OpCodes.Call, genericIsAMethod, null);
                }
                // if this is a reference type, and there is a parameterless constructor
                // build an empty new object and return that
                else
                {
                    var makeObjectMethod = typeof(EmptyInstance).GetMethod("Make");
                    var genericMakeMethod = makeObjectMethod.MakeGenericMethod(new[] { returnType });
                    ilGenerator.EmitCall(OpCodes.Call, genericMakeMethod, null);
                }
            }

            ilGenerator.MarkLabel(returnLabel);
            ilGenerator.Emit(OpCodes.Ret);

            Type dynamicDelegateType; 
            if (returnType != null)
            {
                var paramTypesArrayWithReturnType = paramTypesArray.Concat(new[] { returnType }).ToArray();

                // todo: theoretical limit here of 16 parameters, because func only supports that many
                dynamicDelegateType = Expression.GetFuncType(paramTypesArrayWithReturnType);
            }
            else
            {
                dynamicDelegateType = Expression.GetActionType(paramTypesArray);
            }

            return dynamicMethod.CreateDelegate(dynamicDelegateType);
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

        protected void AddCallResult() => AddCallResultWithParams(this, new object[] { });

        protected void AddCallResultWithParams(params object[] parameters) => CallResults.Add(new ShimmedMethodCall(parameters));

    }

    internal class ShimmedMethod<T> : ShimmedMethod
    {
        public ShimmedMethod(MethodInfo method, object invokingInstance = null) : base(method, invokingInstance)
        {
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
            if(!_expressionParameters.Any() && InvokingInstance == null)
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
