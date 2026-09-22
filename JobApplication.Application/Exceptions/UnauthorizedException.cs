namespace JobApplication.Application.Exceptions
{
    /// <summary>Invalid credentials → 401 Unauthorized</summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
