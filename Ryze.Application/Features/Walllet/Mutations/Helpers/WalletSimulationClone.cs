using System.Reflection;
using Ryze.Domain.Features.Wallet.Entity;

namespace Ryze.Application.Features.Walllet.Mutations.Helpers;

/// <summary>
/// Provides wallet cloning utilities used during mutation simulation.
/// </summary>
/// <remarks>
/// Creates a shallow copy of a wallet aggregate without invoking constructors
/// or domain initialization logic.
///
/// This helper is intended for simulation scenarios where mutation execution
/// needs an isolated aggregate instance while preserving the current object
/// state.
///
/// The clone operation does not recursively copy referenced objects. Nested
/// entities and collections remain shared references, therefore, this helper
/// should only be used when the mutation simulation does not modify the nested
/// state directly.
/// </remarks>
internal static class WalletSimulationClone
{
    /// <summary>
    /// Cached reference to the runtime memberwise clone method.
    /// </summary>
    private static readonly MethodInfo MemberwiseCloneMethod =
        typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Creates a shallow copy of a wallet aggregate.
    /// </summary>
    /// <param name="state">
    /// Wallet aggregate instance to clone.
    /// </param>
    /// <returns>
    /// A new wallet object containing copied field values from the source instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided wallet instance is null.
    /// </exception>
    public static Wallet ShallowClone(Wallet state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return (Wallet)MemberwiseCloneMethod.Invoke(state, null)!;
    }
}