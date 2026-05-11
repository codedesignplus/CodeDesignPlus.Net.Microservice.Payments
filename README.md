# 💳 Payments Microservice

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](LICENSE.md)
[![Tests](https://img.shields.io/badge/tests-passing-success)](tests/)
[![Coverage](https://img.shields.io/badge/coverage-80%25-green)]()
[![Docker](https://img.shields.io/badge/docker-ready-2496ED?logo=docker)](Dockerfile)

A production-ready microservice for processing payments and managing transactions built with .NET 9. Implements Clean Architecture, DDD, and CQRS patterns with support for multiple payment providers (PayU, Stripe, etc.).

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Technology Stack](#️-technology-stack)
- [Prerequisites](#️-prerequisites)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Payment Providers](#-payment-providers)
- [Configuration](#️-configuration)
- [Use Cases & Scenarios](#-use-cases--scenarios)
- [Architecture](#️-architecture)
- [Testing](#-testing)
- [Best Practices](#-best-practices)
- [Troubleshooting](#-troubleshooting)
- [Payment Flow](#-payment-flow)
- [Webhooks](#-webhooks)
- [Security](#-security)
- [FAQ](#-faq)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

The Payments microservice provides a unified API for payment processing across different payment providers. It abstracts the complexity of payment gateway integrations, offering features like:

- **Multi-provider support**: PayU, Stripe, and extensible to other providers
- **Payment initiation**: Create payment transactions with buyer/payer information
- **Card tokenization**: Securely tokenize credit cards for future use
- **Webhook handling**: Process asynchronous payment notifications
- **Status tracking**: Monitor payment lifecycle (Initiated → Succeeded/Failed)
- **Multi-currency support**: Handle payments in multiple currencies
- **Multi-tenancy**: Isolate payments by tenant
- **Payment methods**: Support for credit cards, debit cards, PSE, cash, and more

### 🚀 Quick Start

```bash
# 1. Start infrastructure services
git clone https://github.com/codedesignplus/CodeDesignPlus.Environment.Dev
cd CodeDesignPlus.Environment.Dev/resources
docker-compose up -d

# 2. Configure Vault secrets
cd ../../tools/vault
./config-vault.sh

# 3. Run the microservice
dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Payments.Rest

# 4. Access Swagger UI
open http://localhost:5000/swagger
```

### 📊 High-Level Architecture

```
┌─────────────┐
│   Client    │
│ Application │
└──────┬──────┘
       │ HTTPS + JWT
       │
┌──────▼──────────────────────────────────────────────┐
│         Payments Microservice (REST API)            │
│  ┌──────────────┐  ┌─────────────┐  ┌────────────┐ │
│  │ Controllers  │  │  MediatR    │  │  Handlers  │ │
│  │   (API)      │─▶│   (CQRS)    │─▶│ (Business) │ │
│  └──────────────┘  └─────────────┘  └────┬───────┘ │
│                                           │         │
│  ┌────────────────────────────────────────▼──────┐ │
│  │       Payment Provider Adapters               │ │
│  │  ┌─────────┐  ┌─────────┐  ┌──────────────┐  │ │
│  │  │  PayU   │  │ Stripe  │  │ Custom...    │  │ │
│  │  └─────────┘  └─────────┘  └──────────────┘  │ │
│  └────────────────────────────────────────────────┘ │
└───────┬──────────────────┬──────────────────┬───────┘
        │                  │                  │
   ┌────▼────┐      ┌──────▼──────┐    ┌─────▼─────┐
   │ MongoDB │      │Payment APIs │    │ RabbitMQ  │
   │(Txn Data)      │ (PayU,etc.) │    │ (Events)  │
   └─────────┘      └─────────────┘    └───────────┘
```

## 🚀 Key Features

### Core Capabilities

- ✅ **Multi-Provider Support**: Integrate with PayU, Stripe, and custom providers
- ✅ **Payment Initiation**: Create and process payment transactions
- ✅ **Card Tokenization**: Securely store card tokens for recurring payments
- ✅ **Webhook Processing**: Handle asynchronous payment confirmations
- ✅ **Status Management**: Track payment lifecycle and state transitions
- ✅ **Multi-Currency**: Support for USD, COP, EUR, and custom currencies
- ✅ **Payment Methods**: Credit cards, debit cards, PSE, cash, bank transfers
- ✅ **Buyer/Payer Separation**: Support for different buyer and payer entities
- ✅ **Reference Tracking**: Link payments to orders, subscriptions, or invoices
- ✅ **Problem Details**: RFC 7807 compliant error responses

### Technical Features

- Clean Architecture with DDD and CQRS
- Domain events for payment state changes
- MongoDB for transaction persistence
- RabbitMQ for event publishing
- Redis for distributed caching
- OAuth2/OpenID Connect security
- Provider adapter pattern for extensibility
- Multi-tenancy support
- Swagger/OpenAPI documentation
- Docker containerization
- Comprehensive test coverage (Unit, Integration)

## 🛠️ Technology Stack

### Core
- **.NET 9** - Runtime and framework
- **ASP.NET Core** - Web API framework
- **C# 13** - Programming language

### Storage & Data
- **MongoDB** - Transaction persistence and queries
- **Redis** - Distributed caching and session storage

### Messaging & Events
- **RabbitMQ** - Event publishing and message broker

### Payment Providers
- **PayU** - Latin America payment gateway
- **Stripe** - Global payment platform
- **Extensible** - Add custom providers via adapter pattern

### Architecture & Patterns
- **MediatR** - CQRS command/query handling
- **FluentValidation** - Input validation
- **Mapster** - Object mapping
- **NodaTime** - Date/time handling
- **Adapter Pattern** - Provider abstraction

### Security & Configuration
- **Vault** - Secret management
- **OAuth2/OpenID Connect** - Authentication
- **JWT Bearer** - Token-based security
- **HTTPS** - Encrypted communication

### DevOps & Testing
- **Docker** - Containerization
- **xUnit** - Unit/integration testing
- **Swagger/OpenAPI** - API documentation

## ⚙️ Prerequisites

### Required
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker & Docker Compose** - For infrastructure services
- **MongoDB 6.0+** - Document database
- **Redis 7.0+** - Caching layer
- **RabbitMQ 3.12+** - Message broker

### Optional
- **Vault** - Secret management (can use appsettings for local dev)
- **Payment Provider Accounts** - PayU, Stripe credentials for testing

## 🚀 Getting Started

The following instructions will help you set up the project on your local machine for development and testing purposes.

1. Clone the repository:
```bash
git clone <repository-url>
cd CodeDesignPlus.Net.Microservice.Payments
```

2. Run the MongoDB, Redis, and RabbitMQ services using Docker Compose. Clone this repository [CodeDesignPlus.Environment.Dev](https://github.com/codedesignplus/CodeDesignPlus.Environment.Dev) and run the following command:

```bash
cd resources
docker-compose up -d
```

3. Run the script to config the vault:

```bash
cd tools/vault
./config-vault.sh
```

4. Build the solution:
```bash
dotnet build
```

5. Run the desired entry point:
   
   - For REST API:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Payments.Rest
      ```

   - For gRPC:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Payments.gRpc
      ```

   - For Worker:
      ```bash
      dotnet run --project src/entrypoints/CodeDesignPlus.Net.Microservice.Payments.AsyncWorker
      ```

## 📡 API Endpoints

### Payment Operations

#### Initiate Payment
```http
POST /api/payment/initiate
Content-Type: application/json
Authorization: Bearer {token}
X-Tenant: {tenant-id}

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "module": "Orders",
  "referenceId": "order-123",
  "subTotal": {
    "amount": 100.00,
    "currency": "USD"
  },
  "tax": {
    "amount": 19.00,
    "currency": "USD"
  },
  "total": {
    "amount": 119.00,
    "currency": "USD"
  },
  "buyer": {
    "fullName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "document": "123456789",
    "documentType": "CC"
  },
  "payer": {
    "fullName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "document": "123456789",
    "documentType": "CC"
  },
  "paymentMethod": {
    "id": "credit-card-visa",
    "type": "CreditCard",
    "name": "Visa"
  },
  "description": "Order #123 - Electronics",
  "paymentProvider": "PayU"
}
```

**Response**: `200 OK` with initiation response
```json
{
  "paymentId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Initiated",
  "redirectUrl": "https://gateway.payu.com/checkout/...",
  "providerData": {
    "transactionId": "txn_abc123",
    "checkoutUrl": "https://gateway.payu.com/checkout/..."
  }
}
```

#### Get Payment by ID
```http
GET /api/payment/{id}
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Response**: `200 OK` with payment details
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "module": "Orders",
  "referenceId": "order-123",
  "status": "Succeeded",
  "subTotal": {
    "amount": 100.00,
    "currency": "USD"
  },
  "tax": {
    "amount": 19.00,
    "currency": "USD"
  },
  "total": {
    "amount": 119.00,
    "currency": "USD"
  },
  "buyer": {
    "fullName": "John Doe",
    "email": "john@example.com"
  },
  "payer": {
    "fullName": "John Doe",
    "email": "john@example.com"
  },
  "paymentMethod": {
    "type": "CreditCard",
    "name": "Visa"
  },
  "description": "Order #123 - Electronics",
  "paymentProvider": "PayU",
  "createdAt": "2026-05-11T10:00:00Z",
  "updatedAt": "2026-05-11T10:05:00Z"
}
```

#### List Payments (Paginated)
```http
GET /api/payment?limit=50&skip=0&filter=status eq 'Succeeded'&orderby=createdAt desc
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Query Parameters**:
- `limit` (optional): Number of items per page (default: 100)
- `skip` (optional): Number of items to skip (default: 0)
- `filter` (optional): OData filter expression
- `orderby` (optional): OData order expression

**Response**: `200 OK` with paginated results
```json
{
  "data": [...],
  "totalCount": 250,
  "limit": 50,
  "skip": 0
}
```

#### Tokenize Credit Card
```http
POST /api/payment/tokenize
Content-Type: application/json
Authorization: Bearer {token}
X-Tenant: {tenant-id}

{
  "paymentProvider": "PayU",
  "payer": {
    "fullName": "John Doe",
    "email": "john@example.com",
    "document": "123456789",
    "documentType": "CC"
  },
  "cardNumber": "4111111111111111",
  "expirationDate": "12/25",
  "cvv": "123",
  "cardHolderName": "JOHN DOE"
}
```

**Response**: `200 OK` with token
```json
{
  "token": "tok_1234567890abcdef",
  "maskedCard": "411111******1111",
  "brand": "Visa",
  "expirationDate": "12/25"
}
```

### Payment Method Management

#### Get Available Payment Methods
```http
GET /api/paymentmethod?country=CO&provider=PayU
Authorization: Bearer {token}
X-Tenant: {tenant-id}
```

**Response**: `200 OK` with available payment methods
```json
{
  "data": [
    {
      "id": "credit-card-visa",
      "type": "CreditCard",
      "name": "Visa",
      "description": "Visa Credit Card",
      "enabled": true,
      "country": "CO",
      "provider": "PayU"
    },
    {
      "id": "pse",
      "type": "BankTransfer",
      "name": "PSE",
      "description": "Pagos Seguros en Línea",
      "enabled": true,
      "country": "CO",
      "provider": "PayU"
    }
  ]
}
```

### Webhook Endpoints

#### Receive Payment Notification (Provider Webhook)
```http
POST /api/payment/notify/{providerName}
Content-Type: application/json
[AllowAnonymous]

{
  // Provider-specific payload
  // Signature validation performed internally
}
```

**Response**: `200 OK` if webhook processed successfully

### Error Responses

All errors follow RFC 7807 Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Payment not found.",
  "extensions": {
    "layer": "Application",
    "error_code": "PAY-404",
    "traceId": "0HMVJ3K7S5Q2K:00000001"
  }
}
```

**Common Status Codes**:
- `200 OK` - Success
- `400 Bad Request` - Invalid input or business rule violation
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Invalid webhook signature
- `404 Not Found` - Payment not found
- `500 Internal Server Error` - Server error

## 💳 Payment Providers

### PayU Integration

**Configuration**:
```json
{
  "PayU": {
    "ApiUrl": "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi",
    "MerchantId": "your-merchant-id",
    "ApiKey": "your-api-key",
    "ApiLogin": "your-api-login",
    "AccountId": "your-account-id",
    "WebhookSecret": "your-webhook-secret"
  }
}
```

**Supported Countries**: Colombia, Mexico, Argentina, Brazil, Chile, Peru, Panama

**Supported Payment Methods**:
- Credit Cards (Visa, Mastercard, Amex)
- Debit Cards
- PSE (Colombia)
- Cash (Baloto, Efecty, etc.)
- Bank Transfers

### Stripe Integration

**Configuration**:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

**Supported Countries**: Global (150+ countries)

**Supported Payment Methods**:
- Credit/Debit Cards
- Apple Pay
- Google Pay
- SEPA Direct Debit
- And many more...

### Adding Custom Providers

Implement the `IPaymentProviderAdapter` interface:

```csharp
public interface IPaymentProviderAdapter
{
    Task<InitiatePaymentResult> InitiatePaymentAsync(
        PaymentAggregate payment, 
        CancellationToken cancellationToken);
    
    Task<WebhookResponse> ProcessWebhookAsync(
        HttpRequest request, 
        CancellationToken cancellationToken);
    
    Task<TokenizeCardResult> TokenizeCardAsync(
        TokenizeCardCommand command, 
        CancellationToken cancellationToken);
}
```

Register in DI container:
```csharp
services.AddScoped<IPaymentProviderAdapter, CustomProviderAdapter>();
services.AddSingleton<IPaymentProviderAdapterFactory, PaymentProviderAdapterFactory>();
```

## ⚙️ Configuration

### Payment Provider Setup

Configure providers in `appsettings.json`:

```json
{
  "Payment": {
    "DefaultProvider": "PayU",
    "Providers": {
      "PayU": {
        "Enabled": true,
        "ApiUrl": "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi",
        "MerchantId": "508029",
        "ApiKey": "4Vj8eK4rloUd272L48hsrarnUA",
        "ApiLogin": "pRRXKOl8ikMmt9u",
        "AccountId": "512321",
        "WebhookSecret": "your-webhook-secret"
      },
      "Stripe": {
        "Enabled": false,
        "SecretKey": "sk_test_...",
        "PublishableKey": "pk_test_...",
        "WebhookSecret": "whsec_..."
      }
    }
  }
}
```

### Security Configuration

```json
{
  "Security": {
    "Authority": "https://your-identity-server.com",
    "Audience": "payments-api",
    "RequireHttpsMetadata": true,
    "ValidateIssuer": true,
    "ValidateAudience": true
  }
}
```

### Multi-tenancy

The microservice supports multi-tenancy through the `X-Tenant` header. Each request must include a tenant ID:

```http
X-Tenant: 9588813a-7bc0-4be4-a169-293061881cc3
```

Payments are isolated by tenant at the repository level.

### Environment Variables

Key environment variables for Docker deployment:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
MONGO_CONNECTION_STRING=mongodb://mongo:27017
REDIS_CONNECTION_STRING=redis:6379
RABBITMQ_HOST=rabbitmq
VAULT_ADDRESS=http://vault:8200
VAULT_TOKEN=your-vault-token
PAYU_API_KEY=your-payu-key
STRIPE_SECRET_KEY=your-stripe-key
```

## 🎯 Use Cases & Scenarios

### 1. E-commerce Checkout
Process payment for online shopping cart:

```bash
# Step 1: Customer initiates checkout
POST /api/payment/initiate
- Module: "Orders"
- ReferenceId: order-12345
- Total: $119.00
- PaymentProvider: PayU

# Step 2: Redirect customer to payment gateway
Response: redirectUrl → https://gateway.payu.com/checkout/...

# Step 3: Customer completes payment on gateway

# Step 4: Webhook notification received
POST /api/payment/notify/PayU
- PaymentId: {guid}
- Status: Succeeded

# Step 5: System updates order status
Event: PaymentResponseAssociatedDomainEvent → Order service
```

### 2. Subscription Billing
Tokenize card for recurring payments:

```bash
# Step 1: Tokenize customer's card
POST /api/payment/tokenize
- CardNumber: 4111111111111111
- ExpirationDate: 12/25
- CVV: 123

Response: token → "tok_abc123xyz"

# Step 2: Store token with subscription
SubscriptionService stores token for future charges

# Step 3: Monthly billing
POST /api/payment/initiate
- PaymentMethod: { token: "tok_abc123xyz" }
- Description: "Monthly subscription"
```

### 3. Multi-Tenant Marketplace
Handle payments for multiple vendors:

```bash
# Vendor A payment
POST /api/payment/initiate
Headers: X-Tenant: vendor-a-id
- ReferenceId: vendor-a-order-1

# Vendor B payment (isolated)
POST /api/payment/initiate
Headers: X-Tenant: vendor-b-id
- ReferenceId: vendor-b-order-1

# Payments are completely isolated by tenant
```

### 4. Split Payments
Separate buyer and payer entities:

```bash
# Company (Buyer) orders, but employee (Payer) pays
POST /api/payment/initiate
{
  "buyer": {
    "fullName": "ACME Corporation",
    "email": "billing@acme.com",
    "document": "900123456"
  },
  "payer": {
    "fullName": "John Employee",
    "email": "john@acme.com",
    "document": "123456789"
  }
}
```

### 5. Payment Status Tracking
Monitor payment lifecycle:

```bash
# Check payment status
GET /api/payment/{id}

Status progression:
1. Initiated → Payment created, awaiting gateway response
2. Succeeded → Payment confirmed via webhook
3. Failed → Payment rejected by gateway

# Query all successful payments
GET /api/payment?filter=status eq 'Succeeded'

# Query payments for specific order
GET /api/payment?filter=referenceId eq 'order-123'
```

## 🏗️ Architecture

### Clean Architecture Layers

```
src/
├── domain/                          # Domain Layer
│   ├── Domain/                      # Aggregates, Entities, Value Objects
│   │   ├── PaymentAggregate.cs     # Main aggregate root
│   │   ├── Enums/                  # PaymentStatus, PaymentProvider
│   │   ├── ValueObjects/           # PaymentMethod, etc.
│   │   ├── DomainEvents/           # PaymentInitiated, etc.
│   │   └── Repositories/           # IPaymentRepository
│   ├── Application/                 # Application Layer
│   │   ├── Commands/               # InitiatePayment, UpdateStatus, TokenizeCard
│   │   ├── Queries/                # GetById, GetAll
│   │   ├── DTOs/                   # PaymentDto, InitiatePaymentResponseDto
│   │   ├── Common/                 # IPaymentProviderAdapter
│   │   └── Validators/             # FluentValidation rules
│   └── Infrastructure/              # Infrastructure Layer
│       ├── Repositories/           # MongoDB implementation
│       └── Services/               # PayU, Stripe adapters
└── entrypoints/                     # Presentation Layer
    ├── Rest/                        # REST API
    │   ├── Controllers/            # PaymentController, PaymentMethodController
    │   └── Program.cs              # Startup configuration
    ├── gRpc/                        # gRPC API
    └── AsyncWorker/                 # Background jobs
```

### CQRS Pattern

**Commands** (Write operations):
- `InitiatePaymentCommand` - Create payment transaction
- `UpdateStatusCommand` - Update payment status from webhook
- `TokenizeCardCommand` - Tokenize credit card

**Queries** (Read operations):
- `GetPaymentByIdQuery` - Get payment by ID
- `GetAllPaymentsQuery` - List with pagination
- `GetPaymentMethodsQuery` - Get available payment methods

### Domain Events

Published to RabbitMQ after successful operations:
- `PaymentInitiatedDomainEvent` - Payment created
- `PaymentInitiationRespondedDomainEvent` - Gateway responded
- `PaymentResponseAssociatedDomainEvent` - Final status received (Succeeded/Failed)

### Payment Flow

```
Client initiates payment
     ↓
[Controller] → Validates request
     ↓
[InitiatePaymentCommand] → Creates PaymentAggregate
     ↓
[Handler] → Persists to MongoDB
     ↓
[PaymentProviderAdapter] → Calls PayU/Stripe API
     ↓
[SetInitiateResponse] → Stores gateway response
     ↓
[IPubSub] → Publishes PaymentInitiatedDomainEvent
     ↓
Returns redirect URL to client
     ↓
Customer completes payment on gateway
     ↓
[Webhook] → Gateway calls /api/payment/notify/PayU
     ↓
[ProcessWebhookAsync] → Validates signature
     ↓
[UpdateStatusCommand] → Updates PaymentAggregate status
     ↓
[SetFinalResponse] → Stores final gateway response
     ↓
[IPubSub] → Publishes PaymentResponseAssociatedDomainEvent
     ↓
Order service listens and updates order status
```

## 🧪 Testing

### Unit & Integration Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/unit/CodeDesignPlus.Net.Microservice.Payments.Rest.Test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

### Manual Testing with Postman

Import the Postman collection from `docs/postman/` for manual testing.

### Testing Payment Webhooks Locally

Use ngrok to expose local endpoint for webhook testing:

```bash
# Start ngrok tunnel
ngrok http 5000

# Configure webhook URL in PayU dashboard
https://abc123.ngrok.io/api/payment/notify/PayU

# Test webhook with provider's test panel
```

## 💡 Best Practices

### Payment Integration

#### ✅ DO: Validate amounts before initiation
```csharp
// Ensure total = subTotal + tax
var total = subTotal + tax;
DomainGuard.IsTrue(total != calculatedTotal, Errors.TotalMustMatch);
```

#### ✅ DO: Store payment reference IDs
```csharp
// Link payments to your domain entities
public class Order
{
    public Guid PaymentId { get; set; }  // Store payment aggregate ID
    public PaymentStatus PaymentStatus { get; set; }
}
```

#### ✅ DO: Handle webhook idempotency
```csharp
// Webhooks may be called multiple times
if (payment.Status != PaymentStatus.Initiated)
{
    // Already processed, return success without updating
    return Ok();
}
```

#### ✅ DO: Use tokenization for recurring payments
```csharp
// First payment: tokenize card
var tokenResult = await TokenizeCardAsync(cardDetails);

// Store token securely
subscription.PaymentToken = tokenResult.Token;

// Future payments: use token
var payment = InitiatePaymentAsync(new PaymentRequest {
    PaymentToken = subscription.PaymentToken
});
```

#### ❌ DON'T: Store sensitive card data
```csharp
// Bad: Never store raw card data
public class Payment
{
    public string CardNumber { get; set; }  // ❌ PCI violation
    public string CVV { get; set; }          // ❌ Never store CVV
}

// Good: Store only tokens and masked data
public class Payment
{
    public string Token { get; set; }        // ✅ Provider token
    public string MaskedCard { get; set; }   // ✅ Last 4 digits
}
```

#### ❌ DON'T: Trust client-side amounts
```csharp
// Bad: Accept total from client
var payment = new Payment { Total = request.Total };  // ❌ Can be manipulated

// Good: Recalculate server-side
var order = await GetOrderAsync(request.OrderId);
var total = order.CalculateTotal();  // ✅ Server-side calculation
```

### Security

1. **Always validate webhook signatures** to prevent spoofing
2. **Use HTTPS only** in production
3. **Rotate API keys** regularly
4. **Never log sensitive data** (card numbers, CVV, API keys)
5. **Implement rate limiting** on payment endpoints
6. **Use separate accounts** for testing and production
7. **Monitor for fraud** patterns and suspicious transactions

### Error Handling

```csharp
// Handle provider errors gracefully
try
{
    var result = await providerAdapter.InitiatePaymentAsync(payment);
}
catch (PaymentProviderException ex)
{
    // Log provider error
    logger.LogError(ex, "Provider error: {Message}", ex.Message);
    
    // Return user-friendly message
    return Problem(
        title: "Payment Failed",
        detail: "Unable to process payment at this time. Please try again.",
        statusCode: 500
    );
}
```

## 🐛 Troubleshooting

### Common Issues

#### Issue: Webhook signature validation fails
**Cause**: Incorrect webhook secret or payload modification.

**Solution**:
```bash
# Verify webhook secret matches provider dashboard
"WebhookSecret": "your-correct-secret"

# Check raw request body is used for signature validation
var rawBody = await new StreamReader(request.Body).ReadToEndAsync();
var isValid = ValidateSignature(rawBody, signature, secret);
```

#### Issue: Payment stuck in "Initiated" status
**Cause**: Webhook not received or failed processing.

**Solution**:
```bash
# Check webhook endpoint is accessible
curl -X POST https://your-domain.com/api/payment/notify/PayU

# Check provider webhook logs in their dashboard

# Manually trigger webhook from provider's test panel

# Check RabbitMQ for failed event publishing
docker logs rabbitmq
```

#### Issue: "Total must equal subtotal plus tax" error
**Cause**: Rounding errors or incorrect calculation.

**Solution**:
```csharp
// Use decimal for money calculations
decimal subTotal = 100.00m;
decimal tax = 19.00m;
decimal total = subTotal + tax;  // 119.00m

// Avoid floating point
float subTotal = 100.00f;  // ❌ Can cause rounding errors
```

#### Issue: Provider returns "Invalid API credentials"
**Cause**: Wrong API key, merchant ID, or environment mismatch.

**Solution**:
```bash
# Verify credentials match provider dashboard
# Ensure using sandbox credentials for test environment
# Check for extra spaces or line breaks in configuration

# Test credentials with provider's API test tool
```

#### Issue: MongoDB connection timeout
**Cause**: MongoDB not accessible or wrong connection string.

**Solution**:
```bash
# Test MongoDB connectivity
mongosh "mongodb://localhost:27017"

# Check Docker containers
docker ps | grep mongo

# Verify connection string
"Mongo": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "payments"
}
```

### Debug Mode

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "CodeDesignPlus": "Trace",
      "CodeDesignPlus.Net.Microservice.Payments.Infrastructure": "Trace",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Health Checks

Check service health:
```bash
curl http://localhost:5000/health
```

Expected response:
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "MongoDB", "status": "Healthy" },
    { "name": "Redis", "status": "Healthy" },
    { "name": "RabbitMQ", "status": "Healthy" }
  ]
}
```

## 🔒 Security

### PCI DSS Compliance

This microservice follows PCI DSS guidelines:

- ✅ **No card data storage**: Only tokens are stored
- ✅ **Encrypted transmission**: HTTPS required
- ✅ **Secure token handling**: Tokens encrypted at rest
- ✅ **Access control**: OAuth2 authentication
- ✅ **Audit logging**: All payment events logged
- ✅ **Network isolation**: Containerized deployment

### Webhook Security

Webhooks are protected by:
- **Signature validation**: HMAC-SHA256 signatures
- **IP whitelisting**: Only provider IPs allowed (firewall level)
- **Replay protection**: Timestamp validation
- **TLS encryption**: HTTPS only

### Best Practices

1. **Rotate secrets regularly**: API keys, webhook secrets
2. **Use environment-specific keys**: Separate test/production
3. **Monitor for anomalies**: Unusual payment patterns
4. **Implement fraud detection**: IP checks, velocity limits
5. **Log security events**: Failed auth, invalid signatures
6. **Regular security audits**: Penetration testing

## 🔄 Webhooks

### Understanding Webhooks

Webhooks are asynchronous HTTP callbacks that payment providers use to notify your system about payment status changes.

### Webhook Flow

```
Payment Gateway
     ↓
Completes payment processing
     ↓
Sends HTTP POST to your webhook URL
     ↓
POST /api/payment/notify/{provider}
     ↓
Your system validates signature
     ↓
Updates payment status
     ↓
Publishes domain event
     ↓
Returns 200 OK to gateway
```

### Webhook Security

```csharp
// Signature validation example (PayU)
public bool ValidateSignature(string rawBody, string receivedSignature, string secret)
{
    var computedSignature = ComputeHmacSha256(rawBody, secret);
    return computedSignature == receivedSignature;
}
```

### Testing Webhooks

```bash
# Use ngrok for local testing
ngrok http 5000

# Configure webhook URL in provider dashboard
https://abc123.ngrok.io/api/payment/notify/PayU

# Trigger test webhook from provider's dashboard
```

### Webhook Retry Logic

Most providers retry failed webhooks:
- Retry intervals: 1min, 5min, 15min, 1hr, 24hr
- Max retries: 5-10 attempts
- Your endpoint should be idempotent

## ❓ FAQ

### General Questions

**Q: Which payment providers are supported?**
A: Currently PayU and Stripe. The architecture supports adding custom providers via the adapter pattern.

**Q: Can I use multiple providers simultaneously?**
A: Yes, each payment can specify a different provider. Configure all providers in appsettings and select per transaction.

**Q: How do I test payments without real money?**
A: Use sandbox/test credentials from payment providers. PayU and Stripe both offer test environments with test card numbers.

**Q: What happens if a webhook fails?**
A: Providers automatically retry webhooks. Implement idempotency to handle duplicate notifications safely.

**Q: Can I refund payments?**
A: Refunds are not currently implemented in the API. They must be processed through the provider's dashboard or API directly.

### Technical Questions

**Q: Why separate Buyer and Payer?**
A: Supports B2B scenarios where a company (buyer) orders but an employee (payer) pays with their personal card.

**Q: How does tokenization work?**
A: Card details are sent directly to the provider API, which returns a token. Only the token is stored, never raw card data.

**Q: What's the payment lifecycle?**
A: `Initiated` (created) → Gateway processes → Webhook received → `Succeeded` or `Failed` (final status).

**Q: How is multi-tenancy enforced?**
A: The `X-Tenant` header is mandatory. Repository queries automatically filter by tenant. Payments are isolated.

**Q: Can I track payment analytics?**
A: Yes, query payments by date range, status, module, etc. Use the pagination API with OData filters.

**Q: How do domain events work?**
A: Events are published to RabbitMQ after state changes. Other microservices (Orders, Subscriptions) subscribe and react.

### Troubleshooting Questions

**Q: Why does my payment show "Initiated" forever?**
A: Webhook was not received. Check webhook URL configuration, network accessibility, and provider webhook logs.

**Q: Why do I get 403 Forbidden on webhook?**
A: Signature validation failed. Verify webhook secret matches provider dashboard and raw body is used for validation.

**Q: How do I debug webhook issues?**
A: Use ngrok for local testing, check provider webhook logs, enable trace logging, verify signature calculation.

**Q: What if total doesn't match subtotal + tax?**
A: Use `decimal` for money calculations, avoid `float`/`double`. Ensure client-side and server-side calculations match.

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

### Development Workflow

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/stripe-integration
   ```

3. **Make your changes**
   - Follow existing code style
   - Add tests for new features
   - Update documentation

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Commit with conventional commits**
   ```bash
   git commit -m "feat: add Stripe provider adapter"
   git commit -m "fix: resolve webhook signature validation"
   git commit -m "docs: update payment flow diagram"
   ```

6. **Push and create Pull Request**
   ```bash
   git push origin feature/stripe-integration
   ```

### Code Standards

- **C# Coding Style**: Follow .editorconfig rules
- **Test Coverage**: Aim for >80% coverage
- **Documentation**: Update README.md for new features
- **Naming Conventions**:
  - Commands: `{Action}Command` (e.g., `InitiatePaymentCommand`)
  - Queries: `{Action}Query` (e.g., `GetPaymentByIdQuery`)
  - Handlers: `{CommandOrQuery}Handler`
  - Tests: `{MethodName}_{Scenario}_{ExpectedResult}`

### Pull Request Checklist

- [ ] Code compiles without warnings
- [ ] All tests pass
- [ ] New features have tests
- [ ] Documentation updated
- [ ] CHANGELOG.md updated (if applicable)
- [ ] No breaking changes (or documented with migration guide)
- [ ] Follows SOLID principles and Clean Architecture

## 📞 Support & Resources

### Getting Help

- **GitHub Issues**: [Report bugs or request features](https://github.com/codedesignplus/CodeDesignPlus.Net.Microservice.Payments/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/codedesignplus/CodeDesignPlus.Net.Microservice.Payments/discussions)
- **Documentation**: [CodeDesignPlus Docs](https://codedesignplus.github.io/)
- **Email**: support@codedesignplus.com

### Related Projects

- **CodeDesignPlus.Net.Sdk**: Core SDK with shared abstractions
- **CodeDesignPlus.Environment.Dev**: Local development environment setup
- **Template Repository**: Microservice scaffolding template

## 📄 License

This project is licensed under the **GNU Lesser General Public License v3.0** - see the [LICENSE.md](LICENSE.md) file for details.

### What This Means

- ✅ **Commercial use**: Use in commercial applications
- ✅ **Modification**: Modify the source code
- ✅ **Distribution**: Distribute the software
- ✅ **Private use**: Use privately
- ⚠️ **Disclose source**: Must disclose source for derivative works
- ⚠️ **License and copyright notice**: Include license and copyright
- ⚠️ **Same license**: Derivative works must use LGPL v3.0

## 🙏 Acknowledgments

Built with:
- **CodeDesignPlus SDK** - Core abstractions and utilities
- **.NET 9** - Microsoft's modern development platform
- **PayU** - Latin America payment gateway
- **Stripe** - Global payment platform
- **MongoDB** - Flexible document database
- **Open Source Community** - For all the amazing tools and libraries

---

**Made with ❤️ by CodeDesignPlus**

*For questions, suggestions, or contributions, please open an issue or pull request.*
