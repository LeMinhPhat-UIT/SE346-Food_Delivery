using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Common
{
    public class ConfirmationResponse
    {
        public string Message { get; set; } = null!;

        public ConfirmationResponse(string message) { this.Message = message; }
    }
}
