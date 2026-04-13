namespace LegacyRenewalApp;

public class SupportFee
{
    public decimal calculateFee(bool includePremiumSupport, string normalizedPlanCode, string notes)
    {
        decimal supportFee = 0m;
        if (includePremiumSupport)
        {
            if (normalizedPlanCode == "START")
            {
                supportFee = 250m;
            }
            else if (normalizedPlanCode == "PRO")
            {
                supportFee = 400m;
            }
            else if (normalizedPlanCode == "ENTERPRISE")
            {
                supportFee = 700m;
            }

            notes += "premium support included; ";
        }
        return supportFee;
    }
}