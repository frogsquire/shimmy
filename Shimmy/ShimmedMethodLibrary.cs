using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shimmy
{
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

        public static void ClearRunningMethod()
        {
            _currentRunningMethod = null;
        }

        // todo: improve this by validating active guid in first param?
        public static void AddCallResultToShim(object[] parameters)
        {
            if (_currentRunningMethod == null)
                throw new NullReferenceException();

            _currentRunningMethod.CallResults.Add(new ShimmedMethodCall(parameters));
        }
    }
}
