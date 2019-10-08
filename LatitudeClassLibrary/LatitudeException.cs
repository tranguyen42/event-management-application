using System;

namespace LatitudeClassLibrary
{
    public class LatitudeException: Exception
    {
        public LatitudeException() : base() { }
        public LatitudeException(string message) : base(message) { }
    }
}
