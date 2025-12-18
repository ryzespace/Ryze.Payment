namespace Ryze.Domain.Features.WalletBalance;

/// <summary>
/// Represents the available payment providers for wallet transactions.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Unknown"/> — Default value when the provider is not specified or recognized.</item>
/// <item><see cref="Stripe"/> — Indicates that the payment is handled via Stripe.</item>
/// <item><see cref="PayPal"/> — Indicates that the payment is handled via PayPal.</item>
/// </list>
/// </remarks>
public enum PaymentProvider
{
    /// <summary>
    /// Default or unrecognized payment provider.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Payment processed via Stripe.
    /// </summary>
    Stripe = 1,

    /// <summary>
    /// Payment processed via PayPal.
    /// </summary>
    PayPal = 2
}