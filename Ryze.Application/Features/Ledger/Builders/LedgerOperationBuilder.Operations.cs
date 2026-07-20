using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Builders;

public sealed partial class LedgerOperationBuilder
{
    /// <summary>
    /// Adds an internal transfer operation between two ledger accounts.
    /// </summary>
    /// <remarks>
    /// Represents a movement of funds where the source account is debited
    /// and the destination account is credited with the same amount.
    /// </remarks>
    /// <param name="fromAccountId">Source ledger account identifier.</param>
    /// <param name="toAccountId">Destination ledger account identifier.</param>
    /// <param name="amount">Amount being transferred.</param>
    /// <param name="currency">Currency of the operation.</param>
    /// <param name="description">Business description of the transfer.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder Transfer(
        string fromAccountId,
        string toAccountId,
        decimal amount,
        string currency,
        string description = "Internal Transfer")
    {
        _builder.AddDebit(fromAccountId, "internal", amount, currency, description, _ => { })
            .AddCredit(toAccountId, "internal", amount, currency, description, _ => { });

        return this;
    }

    /// <summary>
    /// Adds a deposit operation from an external payment source.
    /// </summary>
    /// <remarks>
    /// Records incoming funds from a gateway account into the destination
    /// ledger account.
    /// </remarks>
    /// <param name="toAccountId">Destination ledger account identifier.</param>
    /// <param name="gateway">External payment gateway identifier.</param>
    /// <param name="amount">Deposit amount.</param>
    /// <param name="currency">Currency of the operation.</param>
    /// <param name="description">Business description of the deposit.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder Deposit(
        string toAccountId,
        string gateway,
        decimal amount,
        string currency,
        string description = "Deposit")
    {
        _builder.AddDebit($"gateway:{gateway}", "gateway", amount, currency,
            $"Inbound from {gateway}", _ => { })
            .AddCredit(toAccountId, "user_wallet", amount, currency,
                description, _ => { });

        return this;
    }

    /// <summary>
    /// Adds a platform fee charging operation.
    /// </summary>
    /// <param name="fromAccountId">Account charged with the fee.</param>
    /// <param name="feeType">Fee classification.</param>
    /// <param name="amount">Fee amount.</param>
    /// <param name="currency">Currency of the operation.</param>
    /// <param name="description">Business description of the fee.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder Fee(
        string fromAccountId,
        string feeType,
        decimal amount,
        string currency,
        string description = "Platform Fee")
    {
        _builder.AddDebit(fromAccountId, "user_wallet", amount, currency, 
            description,
            m => m.With("fee_type", feeType))
            .AddCredit(ChartOfAccounts.RevenueTransactionFees, 
                amount,
                currency,
                description,
                m => m.With("fee_type", feeType));

        return this;
    }

    /// <summary>
    /// Adds a balance adjustment operation.
    /// </summary>
    /// <remarks>
    /// Supports both positive and negative account corrections.
    /// Adjustments may optionally use escrow accounts when funds are reserved.
    /// </remarks>
    /// <param name="accountId">Account affected by the adjustment.</param>
    /// <param name="adjustmentType">Business reason for the adjustment.</param>
    /// <param name="amount">Adjustment amount.</param>
    /// <param name="currency">Currency of the adjustment.</param>
    /// <param name="isEncumbrance">Indicates whether funds are held in escrow.</param>
    /// <param name="description">Business description of the adjustment.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder Adjustment(
        string accountId,
        string adjustmentType,
        decimal amount,
        string currency,
        bool isEncumbrance = false,
        string description = "Ledger Adjustment")
    {
        var counterAccount = isEncumbrance
            ? ChartOfAccounts.EscrowFunds
            : ChartOfAccounts.SuspenseAccount;

        Action<MetadataBuilder> metadataConfig =
            m => m.With("adjustment_type", adjustmentType)
                  .With("encumbrance", isEncumbrance.ToString());

        if (ShouldIncrease(adjustmentType))
        {
            _builder.AddDebit(counterAccount, amount, currency, description, metadataConfig)
                .AddCredit(accountId, "user_wallet", amount, currency, description, metadataConfig);
        }
        else
        {
            _builder.AddDebit(accountId, "user_wallet", amount, currency, description, metadataConfig)
                .AddCredit(counterAccount, amount, currency, description, metadataConfig);
        }

        return this;
    }

    /// <summary>
    /// Adds a withdrawal operation to an external destination.
    /// </summary>
    /// <param name="fromAccountId">Account from which funds are withdrawn.</param>
    /// <param name="destination">External payout destination.</param>
    /// <param name="amount">Withdrawal amount.</param>
    /// <param name="currency">Currency of the operation.</param>
    /// <param name="description">Business description of the withdrawal.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder Withdrawal(
        string fromAccountId,
        string destination,
        decimal amount,
        string currency,
        string description = "Withdrawal")
    {
        _builder.AddDebit(fromAccountId, "user_wallet", amount, currency, description, _ => { })
            .AddCredit($"payout:{destination}",
                "payout",
                amount,
                currency,
                $"Outbound to {destination}",
                _ => { });

        return this;
    }

    /// <summary>
    /// Adds a funds reservation operation by moving funds into escrow.
    /// </summary>
    /// <param name="fromAccountId">Account providing reserved funds.</param>
    /// <param name="amount">Reserved amount.</param>
    /// <param name="currency">Currency of the reservation.</param>
    /// <param name="description">Business description.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder EscrowHold(
        string fromAccountId,
        decimal amount,
        string currency,
        string description = "Funds Hold")
    {
        _builder.AddDebit(fromAccountId, "user_wallet", amount, currency, description, _ => { })
            .AddCredit(ChartOfAccounts.EscrowFunds, amount, currency, description, _ => { });

        return this;
    }

    /// <summary>
    /// Adds a funds release operation from escrow back to an account.
    /// </summary>
    /// <param name="toAccountId">Destination account receiving released funds.</param>
    /// <param name="amount">Released amount.</param>
    /// <param name="currency">Currency of the operation.</param>
    /// <param name="description">Business description.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder EscrowRelease(
        string toAccountId,
        decimal amount,
        string currency,
        string description = "Funds Release")
    {
        _builder.AddDebit(ChartOfAccounts.EscrowFunds, amount, currency, description, _ => { })
            .AddCredit(toAccountId, "user_wallet", amount, currency, description, _ => { });

        return this;
    }

    /// <summary>
    /// Adds a currency exchange operation.
    /// </summary>
    /// <remarks>
    /// Represents conversion between currencies while maintaining
    /// ledger traceability of both sides of the exchange.
    /// </remarks>
    public LedgerOperationBuilder CurrencyExchange(
        string accountId,
        decimal fromAmount,
        string fromCurrency,
        decimal toAmount,
        string toCurrency,
        string description = "Currency Exchange")
    {
        _builder.AddDebit(accountId, "user_wallet", fromAmount, fromCurrency, description, _ => { })
            .AddCredit(ChartOfAccounts.CurrencyExchangeGainLoss,
                fromAmount,
                fromCurrency,
                "FX Source",
                _ => { })
            .AddDebit(ChartOfAccounts.CurrencyExchangeGainLoss,
                toAmount,
                toCurrency,
                "FX Destination",
                _ => { })
            .AddCredit(accountId,
                "user_wallet",
                toAmount,
                toCurrency,
                description,
                _ => { });

        return this;
    }

    /// <summary>
    /// Adds a split payment operation distributing funds to multiple recipients.
    /// </summary>
    /// <param name="fromAccountId">Source account identifier.</param>
    /// <param name="totalAmount">Total amount distributed.</param>
    /// <param name="currency">Currency of the payment.</param>
    /// <param name="distributions">Recipient accounts and allocated amounts.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder SplitTransfer(
        string fromAccountId,
        decimal totalAmount,
        string currency,
        List<(string toAccountId, decimal amount, string description)> distributions)
    {
        _builder.AddDebit(fromAccountId,
            "unknown",
            totalAmount,
            currency,
            $"Split payment source: {totalAmount} {currency}",
            _ => { });

        foreach (var dist in distributions)
        {
            _builder.AddCredit(
                dist.toAccountId,
                "unknown",
                dist.amount,
                currency,
                dist.description,
                _ => { });
        }

        return this;
    }

    /// <summary>
    /// Adds a revenue recognition operation.
    /// </summary>
    /// <remarks>
    /// Moves previously reserved escrow funds into recognized revenue.
    /// </remarks>
    public LedgerOperationBuilder RecognizeRevenue(
        decimal amount,
        string currency,
        string feeType,
        string description = "Revenue Recognition")
    {
        _builder.AddDebit(ChartOfAccounts.EscrowFunds,
            amount, 
            currency,
            description,
            _ => { })
            .AddCredit(ChartOfAccounts.RevenueTransactionFees,
                amount,
                currency,
                description,
                m => m.With("fee_type", feeType));

        return this;
    }

    /// <summary>
    /// Adds a refund operation linked to an original transaction.
    /// </summary>
    public LedgerOperationBuilder Refund(
        string originalTransactionId,
        string fromAccountId,
        string toAccountId,
        decimal amount,
        string currency,
        string description = "Refund")
    {
        _builder.AddDebit(toAccountId,
            "user_wallet",
            amount,
            currency,
            description, 
            m => m.WithExternalRef(originalTransactionId))
            .AddCredit(fromAccountId,
                "user_wallet",
                amount,
                currency,
                description,
                m => m.WithExternalRef(originalTransactionId));

        return this;
    }

    /// <summary>
    /// Adds a pending authorization hold operation.
    /// </summary>
    /// <remarks>
    /// Reserves funds without immediately finalizing the balance impact.
    /// </remarks>
    public LedgerOperationBuilder AuthorizeHold(
        string accountId,
        decimal amount,
        string currency,
        string description = "Auth Hold")
    {
        _builder.AddDebit(accountId,
                    amount,
                    currency,
                    description,
                    "user_wallet",
                    status: EntryStatus.Pending)
                .AddCredit(ChartOfAccounts.EscrowFunds,
                    amount,
                    currency,
                    description,
                    status: EntryStatus.Pending);

        return this;
    }

    /// <summary>
    /// Adds a capture operation for a previously authorized hold.
    /// </summary>
    public LedgerOperationBuilder CaptureHold(
        string originalHoldId,
        string toAccountId,
        decimal amount,
        string currency,
        string description = "Capture Hold")
    {
        _builder.AddDebit(ChartOfAccounts.EscrowFunds, 
            amount,
            currency,
            description,
            m => m.WithExternalRef(originalHoldId))
            .AddCredit(toAccountId,
                "unknown",
                amount,
                currency,
                description,
                m => m.WithExternalRef(originalHoldId));

        return this;
    }

    /// <summary>
    /// Cancels a previously authorized funds hold.
    /// </summary>
    public LedgerOperationBuilder CancelHold(
        string originalHoldId,
        string accountId,
        decimal amount,
        string currency,
        string description = "Cancel Hold")
    {
        _builder.AddDebit(ChartOfAccounts.EscrowFunds, 
            amount, 
            currency,
            description,
            m => m.WithExternalRef(originalHoldId))
            .AddCredit(accountId,
                "user_wallet",
                amount,
                currency,
                description,
                m => m.WithExternalRef(originalHoldId));

        return this;
    }

    /// <summary>
    /// Adds a payout operation with an additional platform fee.
    /// </summary>
    public LedgerOperationBuilder PayOutWithFee(
        string fromAccountId,
        string destination,
        decimal amount,
        decimal feeAmount,
        string currency,
        string feeType,
        string description = "Payout with Fee")
    {
        _builder.AddDebit(fromAccountId,
            amount + feeAmount,
            currency,
            description,
            "user_wallet")
            .AddCredit($"payout:{destination}", 
                amount,
                currency,
                $"Outbound: {destination}")
            .AddCredit(ChartOfAccounts.RevenueTransactionFees,
                feeAmount,
                currency,
                $"Fee for payout: {feeType}",
                m => m.With("fee_type", feeType));

        return this;
    }
}