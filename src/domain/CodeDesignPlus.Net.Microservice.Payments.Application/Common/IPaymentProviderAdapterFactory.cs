using System;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;

public interface IPaymentProviderAdapterFactory
{
    IPaymentProviderAdapter GetAdapter(PaymentProvider provider);
}
