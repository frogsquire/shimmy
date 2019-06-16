using System;
using System.Collections.Generic;
using System.Text;

namespace Shimmy
{
    public static class EmptyInstance
    {

        /*
         * Returns a new, empty instance of an object with a parameterless constructor.
         */
        public static T Make<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}
