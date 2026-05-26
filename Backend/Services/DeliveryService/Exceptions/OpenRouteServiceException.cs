namespace DeliveryService.Exceptions
{
    public class OpenRouteServiceException : Exception
    {
        public OpenRouteServiceException(string message)
            : base(message)
        {
        }

        public OpenRouteServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
