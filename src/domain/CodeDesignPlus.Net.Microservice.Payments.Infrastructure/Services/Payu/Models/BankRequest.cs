using System;
using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class BankRequest
{
    public string Language { get; set; } = null!;
    public string Command { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public bool Test { get; set; } = false;
    public BankListInformation BankListInformation { get; set; } = new();
}

public class BankListInformation
{
    public string PaymentMethod { get; set; } = null!;
    public string PaymentCountry { get; set; } = null!;
}
