namespace Ryze.Domain.Features.Shared;

/// <summary>
/// Represents business rule violation raised by the domain model.
/// </summary>
/// <remarks>
/// Use this exception type when aggregate/entity invariants are broken
/// or an operation is not allowed in the current domain state.
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">Domain-specific error message.</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class
    /// with an inner exception.
    /// </summary>
    /// <param name="message">Domain-specific error message.</param>
    /// <param name="innerException">Underlying exception that caused this failure.</param>
    public DomainException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
