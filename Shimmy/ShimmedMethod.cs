using Pose;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shimmy
{
    internal class ShimmedMethod
    {
        public ShimmedMethod(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Shim = GenerateShim();

            CallResults = new List<ShimmedMethodCall>();
        }

        public MethodInfo Method { get; private set; }

        public List<ShimmedMethodCall> CallResults { get; private set; }

        public Shim Shim { get; private set; }

        private Shim GenerateShim()
        {
            if (Method.IsStatic)
            {

                return Shim.Replace(GenerateExpression()).With(() =>
                {
                    // todo: parameters (will have to make a method to do this)
                    CallResults.Add(new ShimmedMethodCall(new object[] { }));
                });
            }

            // todo: implement other method types
            throw new NotImplementedException();
        }

        private Expression<Action> GenerateExpression()
        {
            // todo: parameter mocking?
            var parameters = Method.GetParameters();
            var expressionParameters = new ParameterExpression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                expressionParameters[i] = 
                    Expression.Parameter(parameters[i].ParameterType, parameters[i].Name);
            }

            // todo: handle non-static methods
            var methodCall = Expression.Call(null, Method);
            return Expression.Lambda<Action>(methodCall, expressionParameters);
        }
    }
}
