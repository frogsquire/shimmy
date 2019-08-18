using Shimmy.Data;
using System;
using System.Reflection;
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

            // invoke the target (pass-through) if enabled
            // (this will also update the return value if relevant)
            ilGenerator.Emit(OpCodes.Ldloc, arrayLocal); // todo: necessary to do this again?
            ilGenerator.EmitCall(OpCodes.Call, typeof(ShimLibrary).GetMethod("InvokePassThroughIfSet"), null);

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

        public static object GetDefaultValue(Type returnType)
        {
            // if it's a value type, or an object with parameters in the constructor
            // todo: investigate circular reference issue in object with params in constructor
            // todo: add tests for this case
            if (!returnType.IsValueType && returnType.GetConstructor(Type.EmptyTypes) == null)
            {
                return null; // equivalent to default of returnType
            }
            // if this is a reference type, and there is a parameterless constructor
            // build an empty new object and return that
            else
            {
                return Activator.CreateInstance(returnType);
            }
        }

        public static bool MemberCanReturn(MemberInfo member) 
            => !((member is MethodInfo)
                && ((MethodInfo)member).ReturnType == typeof(void));
    }
}
