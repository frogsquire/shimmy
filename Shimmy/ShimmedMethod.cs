using Pose;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Shimmy
{
    internal abstract class ShimmedMethod
    {
        public MethodInfo Method { get; private set; }

        public List<ShimmedMethodCall> CallResults { get; private set; }

        public Shim Shim { get; private set; }

        public ShimmedMethod(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Shim = GenerateShim();

            CallResults = new List<ShimmedMethodCall>();
        }

        protected abstract Shim GenerateShim();
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
                // todo: object standing in for void here
                if (typeof(T) == typeof(object))
                    return Shim.Replace(GenerateVoidCallExpression()).With(() => AddCallResult());
                else
                    return Shim.Replace(GenerateCallExpression()).With(GetShimActionWithReturn());
            }

            // todo: implement other method types
            throw new NotImplementedException();
        }


        private Expression<Action> GenerateVoidCallExpression()
        {
            var expressionParameters = GenerateExpressionParameters();
            return Expression.Lambda<Action>(Expression.Call(null, Method), expressionParameters);
        }

        private Expression<Func<T>> GenerateCallExpression()
        {
            var expressionParameters = GenerateExpressionParameters();
            return Expression.Lambda<Func<T>>(Expression.Call(null, Method), expressionParameters);
        }

        private ParameterExpression[] GenerateExpressionParameters()
        {
            // todo: parameter mocking?
            var parameters = Method.GetParameters();
            var expressionParameters = new ParameterExpression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                expressionParameters[i] =
                    Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
            }

            return expressionParameters;
        }

        private Func<T> GetShimActionWithReturn()
        {
            return () => LogAndReturnDefault();
        }

        private T LogAndReturnDefault()
        {
            AddCallResult();
            return default(T);
        }

        private void AddCallResult() => CallResults.Add(new ShimmedMethodCall(new object[] { }));
    }
}
