using System;
using System.Reflection;

namespace Shimmy.Data
{
    public class ShimmedMemberCall
    {
        public ShimmedMemberCall(object[] parameters, MemberInfo member)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Member = member ?? throw new ArgumentNullException(nameof(member));
            CalledAt = DateTime.Now;
        }

        public DateTime CalledAt { get; private set; }

        public object[] Parameters { get; private set; }

        public MemberInfo Member { get; private set; }
    }
}
