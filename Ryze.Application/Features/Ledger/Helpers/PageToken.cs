using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Ryze.Application.Features.Ledger.Helpers;

/// <summary>
/// Represents the position of the last item returned by cursor-based pagination.
/// </summary>
/// <param name="SortBy">Field used to order the result set.</param>
/// <param name="SortDirection">Direction used to order the result set.</param>
/// <param name="LastValue">Serialized value of the last item's primary sort field.</param>
/// <param name="LastId">Unique identifier used as a deterministic tie-breaker.</param>
public sealed record PageCursor(
    string SortBy,
    string SortDirection,
    string? LastValue,
    Guid? LastId
);

/// <summary>
/// Encodes and decodes opaque cursor tokens used for ledger pagination.
/// </summary>
public static class PageToken
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Encodes a page cursor into a Base64Url token.
    /// </summary>
    /// <param name="cursor">Cursor to encode.</param>
    /// <returns>Encoded page token.</returns>
    public static string Encode(PageCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        ValidateCursor(cursor);

        var json = JsonSerializer.Serialize(cursor, JsonOpts);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Decodes a Base64Url page token into a page cursor.
    /// </summary>
    /// <param name="token">Encoded page token.</param>
    /// <returns>
    /// Decoded cursor or <see langword="null"/> when no token was provided.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when the token is malformed or contains an invalid cursor.
    /// </exception>
    public static PageCursor? Decode(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var base64 = token
                .Replace('-', '+')
                .Replace('_', '/');

            base64 = base64.PadRight(
                base64.Length + (4 - base64.Length % 4) % 4,
                '=');

            var bytes = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(bytes);

            var cursor = JsonSerializer.Deserialize<PageCursor>(
                json,
                JsonOpts);

            if (cursor is null)
            {
                throw new FormatException(
                    "The page token does not contain a valid cursor.");
            }

            ValidateCursor(cursor);

            return cursor;
        }
        catch (JsonException ex)
        {
            throw new FormatException(
                "The page token contains invalid JSON.",
                ex);
        }
        catch (FormatException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            throw new FormatException(
                "The page token is invalid.",
                ex);
        }
    }

    /// <summary>
    /// Creates a page token representing the specified ledger entry.
    /// </summary>
    /// <param name="entry">Last ledger entry returned by the current page.</param>
    /// <param name="sortBy">Field used to order the result set.</param>
    /// <param name="sortDirection">Direction used to order the result set.</param>
    /// <returns>Encoded page token.</returns>
    public static string FromEntry(
        Domain.Features.Ledger.Entity.JournalEntry entry,
        string sortBy,
        string sortDirection)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var lastValue = GetSortValue(
            entry,
            normalizedSortBy);

        return Encode(
            new PageCursor(
                normalizedSortBy,
                normalizedSortDirection,
                lastValue,
                entry.Id));
    }

    /// <summary>
    /// Gets the serialized value used as the primary cursor position.
    /// </summary>
    /// <param name="entry">Ledger entry used to create the cursor.</param>
    /// <param name="sortBy">Field used to order the result set.</param>
    /// <returns>Invariant serialized sort value.</returns>
    private static string GetSortValue(
        Domain.Features.Ledger.Entity.JournalEntry entry,
        string sortBy)
    {
        return sortBy switch
        {
            "Amount" =>
                entry.Posting.Amount.ToString(
                    CultureInfo.InvariantCulture),

            "Status" =>
                ((int)entry.Posting.Status).ToString(
                    CultureInfo.InvariantCulture),

            "Type" =>
                ((int)entry.Posting.Type).ToString(
                    CultureInfo.InvariantCulture),

            "Timestamp" =>
                entry.Timestamp
                    .ToUniversalTime()
                    .ToString(
                        "O",
                        CultureInfo.InvariantCulture),

            _ => throw new ArgumentOutOfRangeException(
                nameof(sortBy),
                sortBy,
                "Unsupported ledger entry sort field.")
        };
    }

    /// <summary>
    /// Validates the contents of a decoded page cursor.
    /// </summary>
    /// <param name="cursor">Cursor to validate.</param>
    private static void ValidateCursor(PageCursor cursor)
    {
        _ = NormalizeSortBy(cursor.SortBy);
        _ = NormalizeSortDirection(cursor.SortDirection);

        if (cursor.LastId is null)
        {
            throw new FormatException(
                "The page token does not contain a valid last entry identifier.");
        }

        if (string.IsNullOrWhiteSpace(cursor.LastValue))
        {
            throw new FormatException(
                "The page token does not contain a valid last sort value.");
        }
    }

    /// <summary>
    /// Normalizes and validates the requested sort field.
    /// </summary>
    /// <param name="sortBy">Requested sort field.</param>
    /// <returns>Canonical sort field name.</returns>
    private static string NormalizeSortBy(string sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            throw new ArgumentException(
                "Sort field cannot be empty.",
                nameof(sortBy));
        }

        return sortBy.Trim() switch
        {
            var value when value.Equals(
                "Amount",
                StringComparison.OrdinalIgnoreCase) => "Amount",

            var value when value.Equals(
                "Status",
                StringComparison.OrdinalIgnoreCase) => "Status",

            var value when value.Equals(
                "Type",
                StringComparison.OrdinalIgnoreCase) => "Type",

            var value when value.Equals(
                "Timestamp",
                StringComparison.OrdinalIgnoreCase) => "Timestamp",

            _ => throw new ArgumentOutOfRangeException(
                nameof(sortBy),
                sortBy,
                "Unsupported ledger entry sort field.")
        };
    }

    /// <summary>
    /// Normalizes and validates the requested sort direction.
    /// </summary>
    /// <param name="sortDirection">Requested sort direction.</param>
    /// <returns>Canonical sort direction.</returns>
    private static string NormalizeSortDirection(
        string sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
        {
            throw new ArgumentException(
                "Sort direction cannot be empty.",
                nameof(sortDirection));
        }

        return sortDirection.Trim() switch
        {
            var value when value.Equals(
                "Asc",
                StringComparison.OrdinalIgnoreCase) => "Asc",

            var value when value.Equals(
                "Desc",
                StringComparison.OrdinalIgnoreCase) => "Desc",

            _ => throw new ArgumentOutOfRangeException(
                nameof(sortDirection),
                sortDirection,
                "Sort direction must be either 'Asc' or 'Desc'.")
        };
    }
}
