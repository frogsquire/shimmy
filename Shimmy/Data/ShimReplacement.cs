using Shimmy.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shimmy.Data
{
    internal class ShimReplacement
    {
        public const string InvalidReturnTypeError = "Cannot set return value of type {0} on a method with return value of type {1}.";
        public const string CannotSetReturnTypeOnVoidMemberError = "Cannot set return value on an action which does not return a value.";
        public const string CannotGetReturnValueOfVoidMember = "The action for this member cannot have a return value.";

        private object _returnValue;

        public object ReturnValue
        {
            get
            {
                // todo: evaluate IsPassThrough and, if so, invoke
                if (HasReturnValue)
                    return _returnValue;

                throw new InvalidOperationException(CannotGetReturnValueOfVoidMember);
            }
            set
            {
                if (!HasReturnValue)
                    throw new InvalidOperationException(CannotSetReturnTypeOnVoidMemberError);

                if (value != null && value.GetType() != ReturnType)
                    throw new InvalidOperationException(string.Format(InvalidReturnTypeError, value.GetType(), ReturnType));

                _returnValue = value;
            }
        }

        private Type ReturnType { get; }

        public ShimmedMember Parent { get; }

        // todo: add way to set this via the posewrapper/shimmedmethod
        public bool IsPassThrough { get; set; }

        public bool HasReturnValue { get; }

        public MemberInfo Target => Parent.Member;

        public ShimReplacement(ShimmedMember parent, Type returnType, bool isPassThrough = false)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(ReturnType));
            IsPassThrough = isPassThrough;
            HasReturnValue = ShimmedMemberHelper.MemberCanReturn(parent.Member);

            if (HasReturnValue)
                ReturnValue = ShimmedMemberHelper.GetDefaultValue(returnType);
        }

        public ShimReplacement(ShimmedMember parent, Type returnType, object returnValue, bool isPassThrough = false)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(ReturnType));
            IsPassThrough = isPassThrough;
            HasReturnValue = ShimmedMemberHelper.MemberCanReturn(parent.Member) && ReturnType != typeof(void);

            if (HasReturnValue)
                ReturnValue = returnValue;
        }
    }
}
