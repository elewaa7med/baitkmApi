using System;

namespace Baitkm.Infrastructure.Validation
{
    public class SmartException : ApplicationException
    {
        public SmartException(string message = "Smart Eception was thrown") : base(message)
        {
        }
    }
}
