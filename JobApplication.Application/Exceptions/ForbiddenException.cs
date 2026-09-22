namespace JobApplication.Application.Exceptions
{
    /// <summary>The user is authenticated but does not own the resource → 403 Forbidden</summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
