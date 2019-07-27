using System;
using System.Collections.Generic;
using System.Text;

namespace Shimmy
{
    [Flags]
    public enum WrapperOptions
    {
        None=0,
        ShimSpecialNames=1,
        ShimPrivateMembers=2
    }
}
