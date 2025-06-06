using System;
using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class BankRequest
{
    public string Language { get; set; } = null!;
    public string Command { get; set; } = null!;
    [Required]
    public PayuMerchant Merchant { get; set; } = null!;
    public bool Test { get; set; } = false;
    [Required]
    public BankListInformation BankListInformation { get; set; } = new();
}

public class BankResponse
{
    public string Code { get; set; } = null!;
    public string? Error { get; set; } = null!;
    public Bank[] Banks { get; set; } = [];
}

public class Bank
{
    public string Id { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PseCode { get; set; } = null!;
}

public class BankListInformation
{
    [Required]
    public string PaymentMethod { get; set; } = null!;
    [Required]
    public string PaymentCountry { get; set; } = null!;
}
