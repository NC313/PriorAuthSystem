using System;
using System.Collections.Generic;
using System.Text;

namespace PriorAuthSystem.Domain.Enums;

public enum PriorAuthStatus
{
    Draft,
    Submitted,
    UnderReview,
    AdditionalInfoRequested,
    Approved,
    Denied,
    Appealed,
    AppealApproved,
    AppealDenied,
    Expired,
    Canceled
}
