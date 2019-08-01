﻿using Shimmy.Data;
using System;
using System.Reflection.Emit;

namespace Shimmy.Helpers
{
    internal static class ShimmedMemberHelper
    {
        internal static Delegate GenerateDynamicShim(DynamicMethod dynamicMethod, Guid referenceGuid, Type[] paramTypesArray, Type returnType)
        {
            var ilGenerator = dynamicMethod.GetILGenerator();
            var returnLabel = ilGenerator.DefineLabel();

            var arrayLocal = ilGenerator.DeclareLocal(typeof(object[]));

            // mark the current shimmed method as the one to send call results
            // todo: for now, guid is loaded as a string for convenience of not having to pointerize
            ilGenerator.Emit(OpCodes.Ldstr, referenceGuid.ToString());
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimLibrary).GetMethod("SetRunningMethod"), null);

            // create a new object array of necessary length
            ilGenerator.Emit(OpCodes.Ldc_I4, paramTypesArray.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            ilGenerator.Emit(OpCodes.Stloc, arrayLocal);

            // load each parameter into the object array
            for (int i = 0; i < paramTypesArray.Length; i++)
            {
                // set current array index
                ilGenerator.Emit(OpCodes.Ldloc, arrayLocal);
                ilGenerator.Emit(OpCodes.Ldc_I4, i);

                // load the parameter
                ilGenerator.Emit(OpCodes.Ldarg, i);

                // if this is a value type, box it
                if (paramTypesArray[i].IsValueType)
                    ilGenerator.Emit(OpCodes.Box, paramTypesArray[i]);

                // save the element into the array
                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }

            // call the method which will save these parameters
            ilGenerator.Emit(OpCodes.Ldloc, arrayLocal);
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimLibrary).GetMethod("AddCallResultToShim"), null);

            // return - with default return value if necessary
            // provided via a call so the stack will accomodate it
            if (returnType == null)
            {
                ilGenerator.EmitCall(OpCodes.Call, typeof(ShimLibrary).GetMethod("ClearRunningMethod"), null);
            }
            else
            {
                var method = typeof(ShimLibrary).GetMethod("GetReturnValueAndClearRunningMethod").MakeGenericMethod(new Type[] { returnType });
                ilGenerator.EmitCall(OpCodes.Call, method, null);
            }

            ilGenerator.MarkLabel(returnLabel);
            ilGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(DelegateTypeHelper.GetTypeForDelegate(paramTypesArray, returnType));
        }

    }
}