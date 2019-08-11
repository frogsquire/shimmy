using Pose;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Shimmy.Data
{
    internal abstract class ShimmedMember
    {
        public const int MaximumPoseParameters = 10;

        protected Guid LibraryReferenceGuid;
        protected ParameterExpression[] ExpressionParameters;
        protected Type DeclaringType;

        protected ShimmedMember() { }

        public Shim Shim { get; protected set; }

        public ShimReplacement ShimAction { get; protected set; }

        public List<ShimmedMemberCall> CallResults { get; protected set; }

        public MemberInfo Member { get; protected set; }

        protected abstract ParameterExpression[] GenerateExpressionParameters();

        protected ParameterExpression[] GetExpressionParametersFromParameterInfo(ParameterInfo[] parameters)
        {
            if (parameters.Length > MaximumPoseParameters)
                throw new ArgumentException("Member has " + parameters.Length
                    + " parameters. Pose only supports methods with " + MaximumPoseParameters + " parameters or fewer.");

            var expressionParameters = new ParameterExpression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                expressionParameters[i] =
                    Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
            }

            return expressionParameters;
        }

        protected virtual Shim GenerateShim()
        {
            return Shim.Replace(GenerateCallExpression()).With(GetShimAction());
        }

        protected abstract Expression<Action> GenerateCallExpression();

        protected abstract Delegate GetShimAction();

        public object ReturnValue
        {
            get => ShimAction.ReturnValue;
            set
            {
                ShimAction.ReturnValue = value;
            }
        }
    }

    internal abstract class ShimmedMember<T> : ShimmedMember where T : MemberInfo
    {
        public new T Member { get; protected set; }

        public ShimmedMember(T member)
        {
            Init(member, typeof(void), null);
        }

        protected ShimmedMember() 
        {
        }

        protected void Init(T member, Type shimActionReturnType, object shimActionReturnValue)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            base.Member = member; // todo: is necessary?

            ExpressionParameters = GenerateExpressionParameters();
            LibraryReferenceGuid = ShimLibrary.Add(this);
            DeclaringType = member.DeclaringType;
            ShimAction = new ShimReplacement(this, shimActionReturnType, shimActionReturnValue);

            Shim = GenerateShim();
            CallResults = new List<ShimmedMemberCall>();
        }

        protected void AddCallResult() => AddCallResultWithParams(new object[] { });

        protected void AddCallResultWithParams(params object[] parameters) => CallResults.Add(new ShimmedMemberCall(parameters, Member));
    }
}
