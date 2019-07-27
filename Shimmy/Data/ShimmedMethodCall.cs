using System;
using System.Reflection;

namespace Shimmy.Data
{
    public class ShimmedMethodCall
    {
        public ShimmedMethodCall(object[] parameters, MethodInfo method)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            CalledAt = DateTime.Now;
        }

        public DateTime CalledAt { get; private set; }

        public object[] Parameters { get; private set; }

        public MethodInfo Method { get; private set; }
    }
}
