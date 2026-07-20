using System.Collections.Immutable;

namespace Ryze.Domain.Features.Wallet.ValueObject;

/// <summary>
/// Immutable Value Object representing arbitrary metadata
/// associated with Wallet Owner.
/// </summary>
/// <remarks>
/// <para>
/// Metadata is modeled as a normalized, immutable key–value map.
/// Keys are case-insensitive, trimmed, and stored in lower-case form.
/// </para>
/// <para>
/// Intended usage includes:
/// - extensible attributes
/// - integration-specific flags
/// - non-core, non-behavioral data
/// </para>
/// </remarks>
public sealed class Metadata : IEquatable<Metadata>
{
    private readonly ImmutableDictionary<string, string> _values;

    /// <summary>
    /// An empty <see cref="Metadata"/> instance.
    /// </summary>
    public static readonly Metadata Empty = new(
        ImmutableDictionary<string, string>.Empty
    );

    /// <summary>
    /// Gets a read-only view of all metadata entries.
    /// </summary>
    /// <remarks>
    /// Keys are guaranteed to be normalized.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Values => _values;

    /// <summary>
    /// Creates a new <see cref="Metadata"/> instance from the given dictionary.
    /// </summary>
    /// <param name="values">
    /// Initial metadata values. Keys will be normalized.
    /// </param>
    /// <remarks>
    /// <para>
    /// If <paramref name="values"/> is <c>null</c> or empty,
    /// the resulting instance will be equivalent to <see cref="Empty"/>.
    /// </para>
    /// <para>
    /// The input dictionary is not retained; all values are copied
    /// into an immutable structure.
    /// </para>
    /// </remarks>
    public Metadata(IDictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            _values = ImmutableDictionary<string, string>.Empty;
            return;
        }

        _values = values.ToImmutableDictionary(
            keySelector: kv => NormalizeKey(kv.Key),
            elementSelector: kv => kv.Value
        );
    }

    private Metadata(ImmutableDictionary<string, string> values)
    {
        _values = values;
    }

    /// <summary>
    /// Determines whether a metadata entry with the given key exists.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>
    /// <c>true</c> if the key exists; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    public bool Contains(string key)
        => _values.ContainsKey(NormalizeKey(key));

    /// <summary>
    /// Gets the value associated with the given metadata key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>
    /// The metadata value, or <c>null</c> if the key does not exist.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    public string? Get(string key)
        => CollectionExtensions.GetValueOrDefault(_values, NormalizeKey(key));

    /// <summary>
    /// Returns a new <see cref="Metadata"/> instance with the specified
    /// key set to the given value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>
    /// A new <see cref="Metadata"/> instance containing the updated entry.
    /// </returns>
    /// <remarks>
    /// If the key already exists, its value is replaced.
    /// The current instance is not modified.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    public Metadata With(string key, string value)
        => new(_values.SetItem(NormalizeKey(key), value));

    /// <summary>
    /// Returns a new <see cref="Metadata"/> instance without
    /// the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>
    /// A new <see cref="Metadata"/> instance with the entry removed.
    /// </returns>
    /// <remarks>
    /// If the key does not exist, the instance is returned unchanged.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    public Metadata Without(string key)
        => new(_values.Remove(NormalizeKey(key)));

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty.", nameof(key));

        return key.Trim().ToLowerInvariant();
    }

    #region Equality

    /// <summary>
    /// Determines whether this instance is equal to another
    /// <see cref="Metadata"/> instance.
    /// </summary>
    /// <param name="other">The other metadata instance.</param>
    /// <returns>
    /// <c>true</c> if both instances contain the same key–value pairs;
    /// otherwise <c>false</c>.
    /// </returns>
    public bool Equals(Metadata? other)
        => other is not null && _values.SequenceEqual(other._values);

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Metadata other && Equals(other);

    /// <summary>
    /// Returns a hash code based on all metadata entries.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var kv in _values)
                hash = hash * 23 + HashCode.Combine(kv.Key, kv.Value);
            return hash;
        }
    }

    #endregion
}
