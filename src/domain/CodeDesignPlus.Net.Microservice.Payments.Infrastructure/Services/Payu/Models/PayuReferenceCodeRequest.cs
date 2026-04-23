using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuReferenceCodeRequest
{
    public bool Test { get; set; }
    public string Language { get; set; } = null!;
    public string Command { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public RequestDetails Details { get; set; } = null!;
}

public class RequestDetails
{
    public string ReferenceCode { get; set; } = null!;
}