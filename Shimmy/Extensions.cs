using System;
using System.Linq;
using System.Reflection;

namespace Shimmy
{
    public static class Extensions
    {
        /*
         * Based on StackOverflow #588149 - Type.GetMethod() does not support locating overloaded generic methods.
         * Parameter types are not checked because they are generic and not known when locating.
         */
        public static MethodInfo GetGenericMethod(this Type t, string name, Type[] genericArgTypes, Type returnType)
        {
            return (from m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                where m.Name == name &&
                                m.GetGenericArguments().Length == genericArgTypes.Length &&
                                m.ReturnType == returnType
                                select m).Single().MakeGenericMethod(genericArgTypes);
        }
        
    }
}
