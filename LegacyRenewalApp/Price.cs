using System;

namespace LegacyRenewalApp;

public class Price
{
    public decimal paymentFee{ get; set; }
    public decimal discountAmount{ get; set; }
    public decimal supportFee{ get; set; }
    public decimal subtotalAfterDiscount{ get; set; }
    public decimal taxRate{ get; set; }
    public decimal taxBase{ get; set; }
    public decimal taxAmount{ get; set; }
    public decimal finalAmount{ get; set; }

    public Price( string normalizedPaymentMethod, string notes, bool useLoyaltyPoints, int seatCount,Customer customer,  decimal baseAmount, SubscriptionPlan plan, bool includePremiumSupport, string normalizedPlanCode)
    {

        this.discountAmount = calculateDiscount(useLoyaltyPoints, seatCount,notes, customer, baseAmount, plan);
        this.subtotalAfterDiscount = baseAmount - discountAmount;
        if (this.subtotalAfterDiscount < 300m)
        {
            this.subtotalAfterDiscount = 300m;
            notes += "minimum discounted subtotal applied; ";
        }
        this.supportFee = calculateSupFee(includePremiumSupport, normalizedPlanCode, normalizedPlanCode);
        this.paymentFee =calculateFee(normalizedPaymentMethod, subtotalAfterDiscount, supportFee, notes);
        this.taxRate = calculateTaxRate(customer);
        this.taxBase = subtotalAfterDiscount + supportFee + paymentFee;
        this.taxAmount = taxBase * taxRate;
        this.finalAmount = taxBase + taxAmount;
        if (this.finalAmount < 500m)
        {
            this.finalAmount = 500m;
            notes += "minimum invoice amount applied; ";
        }
    }
    public decimal calculateFee(string normalizedPaymentMethod,  decimal subtotalAfterDiscount, decimal supportFee, string notes)
    {
        decimal paymentFee = 0m;

        if (normalizedPaymentMethod == "CARD")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
            notes += "card payment fee; ";
        }
        else if (normalizedPaymentMethod == "BANK_TRANSFER")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
            notes += "bank transfer fee; ";
        }
        else if (normalizedPaymentMethod == "PAYPAL")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
            notes += "paypal fee; ";
        }
        else if (normalizedPaymentMethod == "INVOICE")
        {
            paymentFee = 0m;
            notes += "invoice payment; ";
        }
        else
        {
            throw new ArgumentException("Unsupported payment method");
        }
        return paymentFee;

    }
    public decimal calculateDiscount(bool useLoyaltyPoints, int seatCount, string notes, Customer customer,  decimal baseAmount, SubscriptionPlan plan)
    {
        decimal discountAmount = 0m;
        
        if (customer.Segment == "Silver")
        {
            discountAmount += baseAmount * 0.05m;
            notes += "silver discount; ";
        }
        else if (customer.Segment == "Gold")
        {
            discountAmount += baseAmount * 0.10m;
            notes += "gold discount; ";
        }
        else if (customer.Segment == "Platinum")
        {
            discountAmount += baseAmount * 0.15m;
            notes += "platinum discount; ";
        }
        else if (customer.Segment == "Education" && plan.IsEducationEligible)
        {
            discountAmount += baseAmount * 0.20m;
            notes += "education discount; ";
        }

        if (customer.YearsWithCompany >= 5)
        {
            discountAmount += baseAmount * 0.07m;
            notes += "long-term loyalty discount; ";
        }
        else if (customer.YearsWithCompany >= 2)
        {
            discountAmount += baseAmount * 0.03m;
            notes += "basic loyalty discount; ";
        }

        if (seatCount >= 50)
        {
            discountAmount += baseAmount * 0.12m;
            notes += "large team discount; ";
        }
        else if (seatCount >= 20)
        {
            discountAmount += baseAmount * 0.08m;
            notes += "medium team discount; ";
        }
        else if (seatCount >= 10)
        {
            discountAmount += baseAmount * 0.04m;
            notes += "small team discount; ";
        }

        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            discountAmount += pointsToUse;
            notes += $"loyalty points used: {pointsToUse}; ";
        }
        return discountAmount;
    }
    public decimal calculateSupFee(bool includePremiumSupport, string normalizedPlanCode, string notes)
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

    public decimal calculateTaxRate(Customer customer)
    {
        decimal taxRate = 0.20m;
        if (customer.Country == "Poland")
        {
            taxRate = 0.23m;
        }
        else if (customer.Country == "Germany")
        {
            taxRate = 0.19m;
        }
        else if (customer.Country == "Czech Republic")
        {
            taxRate = 0.21m;
        }
        else if (customer.Country == "Norway")
        {
            taxRate = 0.25m;
        }
        return  taxRate;
    }
}