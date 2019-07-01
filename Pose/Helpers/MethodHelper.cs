using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pose.Helpers
{
    /*
     * Exposes some Pose internal utility methods publicly.
     * todo: abstract out this code, duplicate it, or otherwise find a better solution
     */
    public static class MethodHelper
    {
        public static MethodBase GetMethodFromExpression(Expression expression, bool setter, out Object instanceOrType)
        {
            return ShimHelper.GetMethodFromExpression(expression, setter, out instanceOrType);
        }
    }
}