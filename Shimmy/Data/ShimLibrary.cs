using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shimmy.Data
{
    /*
     * ShimLibary routes call results called in from dynamically generated IL
     * to the appropriate methods. It is not called on methods which have no params and also
     * have a non-void return type.
     * 
     * TODO: support clearing of the library.
     * TODO: integrity issues around this class being public?
     */
    public static class ShimLibrary
    {
        public const string CannotGetReturnValueNoMethodRunningError = "Cannot get return value for method - no shim is running.";
        public const string CannotGetReturnValueNonMatchingTypeError = "Cannot get return value for method - specified return type does not match return type of shim.";

        private static Dictionary<Guid, ShimmedMember> _library = new Dictionary<Guid, ShimmedMember>();
        private static ShimmedMember _currentRunningMember; 

        internal static Guid Add(ShimmedMember member)
        {
            var referenceGuid = Guid.NewGuid();
            _library.Add(referenceGuid, member);
            return referenceGuid;
        }

        public static void SetRunningMethod(string referenceGuidString)
        {
            var referenceGuid = Guid.Parse(referenceGuidString);
            var record = _library.First(l => l.Key.Equals(referenceGuid));
            _currentRunningMember = record.Value;
        }

        // todo: improve this by validating active guid in first param?
        public static void AddCallResultToShim(object[] parameters)
        {
            if (_currentRunningMember == null)
                throw new NullReferenceException();

            _currentRunningMember.CallResults.Add(new ShimmedMemberCall(parameters, _currentRunningMember.Member));
        }

        public static void ClearRunningMethod()
        {
            _currentRunningMember = null;
        }

        public static T GetReturnValueAndClearRunningMethod<T>()
        {
            if (_currentRunningMember == null)
            {
                throw new InvalidOperationException(CannotGetReturnValueNoMethodRunningError);
            }

            var runningMemberGenerics = _currentRunningMember.GetType().GetGenericArguments();
            
            if (runningMemberGenerics == null || runningMemberGenerics.Length == 0 || runningMemberGenerics[0] != typeof(T))
            {
                throw new InvalidOperationException(CannotGetReturnValueNonMatchingTypeError);
            }

            T returnValue;
            if (_currentRunningMember is ShimmedMethod<T>)
                returnValue = ((ShimmedMethod<T>)_currentRunningMember).ReturnValue;
            else
                returnValue = ((ShimmedConstructor<T>)_currentRunningMember).ReturnValue;

            ClearRunningMethod();

            return returnValue;
        }
    }
}
