namespace JobApplication.Application.Exceptions
{
    /// <summary>The requested resource does not exist → 404 Not Found</summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
