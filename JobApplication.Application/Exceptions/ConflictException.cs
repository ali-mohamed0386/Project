namespace JobApplication.Application.Exceptions
{
    /// <summary>
    /// The resource already exists (for example a registered email address
    /// or a duplicate application) → 409 Conflict
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
