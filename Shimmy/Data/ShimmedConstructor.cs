using Shimmy.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Shimmy.Data
{
    internal class ShimmedConstructor<T> : ShimmedMember<ConstructorInfo>
    {
        public ConstructorInfo Constructor => Member;

        public T ReturnValue { get; set; }

        public ShimmedConstructor(ConstructorInfo constructor) : base()
        {
            ReturnValue = ShimmedMemberHelper.GetDefaultValue<T>();
            Init(constructor);
        }

        public ShimmedConstructor(ConstructorInfo constructor, T returnValue) : base()
        {
            ReturnValue = returnValue;
            Init(constructor);
        }

        protected override ParameterExpression[] GenerateExpressionParameters()
        {
            var parameters = Constructor.GetParameters();
            return GetExpressionParametersFromParameterInfo(parameters);
        }

        protected override Expression<Action> GenerateCallExpression()
        {
            return Expression.Lambda<Action>(Expression.New(Constructor, ExpressionParameters));
        }

        // todo: constructors for value types

        protected override Delegate GetShimAction()
        {
            if (!ExpressionParameters.Any())
                return (Func<T>)(() => LogAndReturn());

            return GenerateDynamicShim();
        }

        private T LogAndReturn()
        {
            AddCallResult();

            return ReturnValue;
        }

        private Delegate GenerateDynamicShim()
        {
            var expressionParamsArray = ExpressionParameters.Select(p => p.Type).ToArray();

            var dynamicMethod = new DynamicMethod("shimmy_ctor_" + Constructor.Name,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                Constructor.DeclaringType,
                expressionParamsArray,
                Constructor.DeclaringType,
                false);

            return ShimmedMemberHelper.GenerateDynamicShim(dynamicMethod,
                LibraryReferenceGuid,
                expressionParamsArray,
                Constructor.DeclaringType);
        }

        // todo: reduce duplicated code here
        public override void SetReturnValue(object value)
        {
            if (value.GetType() != typeof(T))
                throw new InvalidOperationException(string.Format(ShimmedMethod.InvalidReturnTypeError, value.GetType(), typeof(T)));

            ReturnValue = (T)value;
        }
    }
}
