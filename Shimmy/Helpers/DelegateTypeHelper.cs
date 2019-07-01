using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shimmy.Helpers
{
    internal static class DelegateTypeHelper
    {
        public static Type GetTypeForDelegate(ParameterInfo[] parameters, Type returnType = null)
        {
            return GetTypeForDelegate(parameters.Select(p => p.ParameterType).ToArray(), returnType);
        }

        public static Type GetTypeForDelegate(Type[] paramTypesArray, Type returnType = null)
        {
            Type dynamicDelegateType;
            if (returnType != null && returnType != typeof(void))
            {
                var paramTypesArrayWithReturnType = paramTypesArray.Concat(new[] { returnType }).ToArray();

                // todo: theoretical limit here of 16 parameters, because func only supports that many
                dynamicDelegateType = Expression.GetFuncType(paramTypesArrayWithReturnType);
            }
            else
            {
                dynamicDelegateType = Expression.GetActionType(paramTypesArray);
            }

            return dynamicDelegateType;
        }
    }
}
