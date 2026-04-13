using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        public RenewalInvoice CreateRenewalInvoice(int customerId, string planCode, int seatCount, string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            checkInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customerRepository = new CustomerRepository();
            var planRepository = new SubscriptionPlanRepository();

            var customer = customerRepository.GetById(customerId);
            var plan = planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            string notes = string.Empty;
            Price price = new Price(normalizedPaymentMethod, notes, useLoyaltyPoints, seatCount, customer, baseAmount,
                plan, includePremiumSupport, normalizedPlanCode);
            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(price.discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(price.supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(price.paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(price.taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(price.finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            LegacyBillingGateway.SaveInvoice(invoice);

            notifyCustomer(customer, normalizedPlanCode, invoice);

            return invoice;
        }
        private void notifyCustomer(Customer customer, string normalizedPlanCode, RenewalInvoice invoice)
        {
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                LegacyBillingGateway.SendEmail(customer.Email, subject, body);
            }
        }
        private static void checkInput(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }
        }
    }
}
