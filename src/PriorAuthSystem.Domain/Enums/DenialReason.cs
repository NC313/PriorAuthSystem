using System;
using System.Collections.Generic;
using System.Text;

namespace PriorAuthSystem.Domain.Enums;

    public enum DenialReason
    {
        NotMedicallyNecessary,
        ServiceNotCovered,
        RequiresAlternativeTreatment,
        InsufficientDocumentation,
        OutOfNetwork,
        DuplicateRequest,
        EligibilityIssue,
        PriorAuthNotRequired,
        Other
    }

