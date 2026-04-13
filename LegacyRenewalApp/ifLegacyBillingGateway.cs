namespace LegacyRenewalApp;

public interface ifLegacyBillingGateway
{
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
}