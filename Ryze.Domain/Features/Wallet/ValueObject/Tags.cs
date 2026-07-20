using System.Collections.Immutable;

namespace Ryze.Domain.Features.Wallet.ValueObject;

/// <summary>
/// Value object representing a normalized, immutable set of tags.
/// </summary>
/// <remarks>
/// Tags are case-insensitive, trimmed, and stored in normalized form.
/// Equality is based on set equivalence, not insertion order.
/// </remarks>
public sealed class Tags : IEquatable<Tags>
{
    private readonly ImmutableHashSet<string> _values;

    /// <summary>
    /// Empty tags instance.
    /// </summary>
    public static readonly Tags Empty =
        new(ImmutableHashSet<string>.Empty);

    /// <summary>
    /// Gets the normalized tag values.
    /// </summary>
    public IReadOnlyCollection<string> Values => _values;

    /// <summary>
    /// Creates a new <see cref="Tags"/> value object from a tag collection.
    /// </summary>
    /// <remarks>
    /// Null input results in an empty tag set.
    /// Invalid or whitespace-only values are ignored.
    /// </remarks>
    [System.Text.Json.Serialization.JsonConstructor]
    public Tags(IReadOnlyCollection<string>? values)
    {
        if (values is null)
        {
            _values = ImmutableHashSet<string>.Empty;
            return;
        }

        _values = values
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToImmutableHashSet();
    }

    private Tags(ImmutableHashSet<string> values)
    {
        _values = values;
    }

    /// <summary>
    /// Determines whether the tag set contains the specified tag.
    /// </summary>
    public bool Contains(string tag)
        => _values.Contains(Normalize(tag));

    /// <summary>
    /// Returns a new <see cref="Tags"/> instance with the specified tag added.
    /// </summary>
    public Tags Add(string tag)
        => new(_values.Add(Normalize(tag)));

    /// <summary>
    /// Returns a new <see cref="Tags"/> instance with the specified tag removed.
    /// </summary>
    public Tags Remove(string tag)
        => new(_values.Remove(Normalize(tag)));

    /// <summary>
    /// Normalizes a tag to its canonical form.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the tag is null, empty, or whitespace.
    /// </exception>
    private static string Normalize(string tag) => 
        string.IsNullOrWhiteSpace(tag) ? throw new ArgumentException("Tag cannot be empty.", nameof(tag))
            : tag.Trim().ToLowerInvariant();

    #region Equality

    /// <summary>
    /// Determines value equality based on set equivalence.
    /// </summary>
    public bool Equals(Tags? other)
        => other is not null && _values.SetEquals(other._values);

    public override bool Equals(object? obj)
        => obj is Tags other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            // Order-independent, deterministic hashing
            foreach (var tag in _values.OrderBy(t => t))
                hash = hash * 23 + tag.GetHashCode();

            return hash;
        }
    }

    #endregion
}
