﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Shimmy.Data
{
    /*
     * ShimmedMethodLibary routes call results called in from dynamically generated IL
     * to the appropriate methods. It is not called on methods which have no params and also
     * have a non-void return type.
     */
    internal static class ShimmedMethodLibrary
    {
        private static Dictionary<Guid, ShimmedMethod> _library = new Dictionary<Guid, ShimmedMethod>();
        private static ShimmedMethod _currentRunningMethod; 

        internal static Guid Add(ShimmedMethod method)
        {
            var referenceGuid = Guid.NewGuid();
            _library.Add(referenceGuid, method);
            return referenceGuid;
        }

        // todo: make these internal for safety
        public static void SetRunningMethod(string referenceGuidString)
        {
            var referenceGuid = Guid.Parse(referenceGuidString);
            var record = _library.First(l => l.Key.Equals(referenceGuid));
            _currentRunningMethod = record.Value;
        }

        // todo: improve this by validating active guid in first param?
        public static void AddCallResultToShim(object[] parameters)
        {
            if (_currentRunningMethod == null)
                throw new NullReferenceException();

            _currentRunningMethod.CallResults.Add(new ShimmedMethodCall(parameters));
        }

        public static void ClearRunningMethod()
        {
            _currentRunningMethod = null;
        }

        public static T GetReturnValueAndClearRunningMethod<T>()
        {
            if (_currentRunningMethod == null || _currentRunningMethod.GetType().IsSubclassOf(typeof(ShimmedMethod<>)))
            {
                throw new InvalidOperationException("Cannot get return value for method - no shim is running, or running shim has void return type.");
            }

            var runningMethodGenerics = _currentRunningMethod.GetType().GetGenericArguments();
            
            if (runningMethodGenerics == null || runningMethodGenerics.Length == 0 || runningMethodGenerics[0] != typeof(T))
            {
                throw new InvalidOperationException("Cannot get return value for method - specified return type does not match return type of shim.");
            }

            var returnValue = ((ShimmedMethod<T>)_currentRunningMethod).HasCustomReturnValue 
                ? ((ShimmedMethod<T>)_currentRunningMethod).ReturnValue 
                : GetDefaultValue<T>();

            ClearRunningMethod();

            return returnValue;
        }

        private static T GetDefaultValue<T>()
        {
            var returnType = typeof(T);

            // if it's a value type, or an object with parameters in the constructor
            // todo: investigate circular reference issue in object with params in constructor
            // todo: add tests for this case
            if (returnType.IsValueType || returnType.GetConstructor(Type.EmptyTypes) == null)
            {
                return default(T);
            }
            // if this is a reference type, and there is a parameterless constructor
            // build an empty new object and return that
            else
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}