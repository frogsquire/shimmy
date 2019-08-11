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
    internal class ShimmedMethod : ShimmedMember<MethodInfo>
    {
        public const string InvalidReturnTypeError = "Cannot set return value of type {0} on a method with return value of type {1}.";

        public MethodInfo Method => Member;
        
        protected override Expression<Action> GenerateCallExpression()
        {
            Expression poseIsACall = null;
            if (!Method.IsStatic)
            {
                var poseIsAMethod = typeof(Pose.Is).GetMethod("A").MakeGenericMethod(new[] { DeclaringType });
                poseIsACall = Expression.Call(null, poseIsAMethod);
            }
            return Expression.Lambda<Action>(Expression.Call(poseIsACall, Method, ExpressionParameters));
        }

        public ShimmedMethod(MethodInfo method) : base(method)
        {
        }

        protected ShimmedMethod()
        {
        }
       
        protected override Delegate GetShimAction()
        {
            // if it's not necessary, don't build a dynamic method
            // this shortcut bypasses the ShimmedMethodLibrary
            // and the overhead of a dynamicmethod
            // while still logging the call
            if (!ExpressionParameters.Any() && Method.IsStatic)
                return (Action)(() => AddCallResult());

            return GenerateDynamicShim();
        }

        protected Delegate GenerateDynamicShim(Type returnType = null)
        {
            var expressionParamsArray = ExpressionParameters.Select(p => p.Type).ToArray();

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

            return ShimmedMemberHelper.GenerateDynamicShim(dynamicMethod, 
                LibraryReferenceGuid, 
                paramTypesArray, 
                returnType);
       }

        protected override ParameterExpression[] GenerateExpressionParameters()
        {
            var parameters = Method.GetParameters();
            return GetExpressionParametersFromParameterInfo(parameters);
        }
    }

    internal class ShimmedMethod<T> : ShimmedMethod
    {
        public T ReturnValue { get; set; }

        private bool ReturnValueIsDefaultForType => !EqualityComparer<T>.Default.Equals(ReturnValue, default(T));

        public ShimmedMethod(MethodInfo method) : base()
        {
            ReturnValue = ShimmedMemberHelper.GetDefaultValue<T>();
            Init(method); 
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
            if(!ExpressionParameters.Any() && Method.IsStatic && !ReturnValueIsDefaultForType)
                return (Func<T>)(() => LogAndReturn());

            return GenerateDynamicShim(typeof(T));
        }

        private T LogAndReturn()
        {
            AddCallResult();
            return ReturnValue;
        }

        public override void SetReturnValue(object value)
        {
            if (value.GetType() != typeof(T))
                throw new InvalidOperationException(string.Format(InvalidReturnTypeError, value.GetType(), typeof(T)));

            ReturnValue = (T)value;    
        }
    }
}
