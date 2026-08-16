using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Helper;

public static class CustomerBillingRules
{
    public static void ValidateAmpereScheduleAssignment(
        PlanType plan,
        Guid? ampereScheduleId,
        AppPreferences preferences)
    {
        if (!preferences.AmpereSchedulePricingEnabled || plan != PlanType.Ampere)
            return;

        if (ampereScheduleId is null)
        {
            throw new DomainException("Error.AmpereScheduleRequired");
        }
    }

    public static Guid? NormalizeAmpereScheduleId(PlanType plan, Guid? ampereScheduleId)
        => plan == PlanType.Ampere ? ampereScheduleId : null;
}
