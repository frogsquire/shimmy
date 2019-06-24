using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Shimmy
{
    internal static class DelegateTypeHelper
    {
        public static Type GetTypeForDelegate(Type[] paramTypesArray, Type returnType)
        {
            Type dynamicDelegateType;
            if (returnType != null)
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
