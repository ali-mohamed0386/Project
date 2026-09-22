namespace JobApplication.Application.Exceptions
{
    /// <summary>The request is malformed or rejected by a business rule → 400 Bad Request</summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
