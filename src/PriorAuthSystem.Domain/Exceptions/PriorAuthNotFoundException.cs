namespace PriorAuthSystem.Domain.Exceptions;

public class PriorAuthNotFoundException : KeyNotFoundException
{
    public Guid RequestId { get; }

    public PriorAuthNotFoundException(Guid requestId)
        : base($"Prior authorization request with ID '{requestId}' was not found.")
    {
        RequestId = requestId;
    }
}
