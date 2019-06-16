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
        private Guid _libraryReferenceGuid;
        protected ParameterExpression[] _expressionParameters;

        public const int MaximumPoseParameters = 10; 

        public MethodInfo Method { get; private set; }

        public List<ShimmedMethodCall> CallResults { get; private set; }

        public Shim Shim { get; private set; }

        public ShimmedMethod(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            _expressionParameters = GenerateExpressionParameters();
            _libraryReferenceGuid = ShimmedMethodLibrary.Add(this);

            Shim = GenerateShim();
            CallResults = new List<ShimmedMethodCall>();
        }

        protected virtual Shim GenerateShim()
        {
            if (Method.IsStatic)
            {
                return Shim.Replace(GenerateCallExpression()).With(GetShimAction());
            }

            // todo: implement other method types
            throw new NotImplementedException();

        }

        protected Expression<Action> GenerateCallExpression()
        {
            return Expression.Lambda<Action>(Expression.Call(null, Method, _expressionParameters));
        }

        private Delegate GetShimAction()
        {
            if (!_expressionParameters.Any())
                return (Action)(() => AddCallResult());

            return GenerateDynamicShim();
        }

        protected Delegate GenerateDynamicShim(Type returnType = null)
        {
            var paramTypesArray = _expressionParameters.Select(p => p.Type).ToArray();

            var dynamicMethod = new DynamicMethod("shimmy_" + Method.Name, 
                MethodAttributes.Public | MethodAttributes.Static , // todo: support non-static
                CallingConventions.Standard,
                Method.ReturnType, 
                paramTypesArray, 
                typeof(ShimmedMethod),
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
                var isAMethod = typeof(Is).GetMethod("A");
                var genericIsAMethod = isAMethod.MakeGenericMethod(new[] { returnType });
                ilGenerator.EmitCall(OpCodes.Call, genericIsAMethod, null);
            }

            ilGenerator.MarkLabel(returnLabel);
            ilGenerator.Emit(OpCodes.Ret);

            Type dynamicDelegateType; 
            if (returnType != null)
            {
                var paramTypesArrayWithReturnType = paramTypesArray.Concat(new[] { returnType }).ToArray();
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
        public ShimmedMethod(MethodInfo method) : base(method)
        {
        }

        protected override Shim GenerateShim()
        {
            if (Method.IsStatic)
            {
                return Shim.Replace(GenerateCallExpression()).With(GetShimActionWithReturn());
            }

            // todo: implement other method types
            throw new NotImplementedException();
        }
        
        private Delegate GetShimActionWithReturn()
        {
            if(!_expressionParameters.Any())
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
