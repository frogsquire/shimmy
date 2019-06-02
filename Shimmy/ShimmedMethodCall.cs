using System;
using System.Collections.Generic;
using System.Text;

namespace Shimmy
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
