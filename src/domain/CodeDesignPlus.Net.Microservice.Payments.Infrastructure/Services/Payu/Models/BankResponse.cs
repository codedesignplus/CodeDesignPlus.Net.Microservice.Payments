using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;


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