using System;

namespace Shimmy.Data
{
    public class ShimmedMethodCall
    {
        public ShimmedMethodCall(object[] parameters)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            CalledAt = DateTime.Now;
        }

        public DateTime CalledAt { get; private set; }

        public object[] Parameters { get; private set; }
    }
}
