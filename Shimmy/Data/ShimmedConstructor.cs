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
                return (Func<T>)(() => LogAndReturnOrCreate());

            return GenerateDynamicShim();
        }

        private T LogAndReturnOrCreate()
        {
            AddCallResult();

            // todo: improve this
            if (ReturnValue != null)
                return ReturnValue;

            // todo: objects with only constructors that are parameterized
            return Activator.CreateInstance<T>();
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
    }
}
