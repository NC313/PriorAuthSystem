using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Domain.Exceptions;

public class InvalidStatusTransitionException : InvalidOperationException
{
    public PriorAuthStatus FromStatus { get; }
    public PriorAuthStatus ToStatus { get; }

    public InvalidStatusTransitionException(PriorAuthStatus fromStatus, PriorAuthStatus toStatus)
        : base($"Invalid status transition from '{fromStatus}' to '{toStatus}'.")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }
}
