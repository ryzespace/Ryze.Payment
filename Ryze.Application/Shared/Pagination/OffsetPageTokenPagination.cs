using System.Text;
using System.Text.Json;

namespace Ryze.Application.Shared.Pagination;

/// <summary>
/// Offset-based implementation of page-token pagination.
/// </summary>
/// <remarks>
/// Encodes page position as Base64 JSON payload with an <c>offset</c> field.
/// Invalid tokens are treated as offset <c>0</c>.
/// </remarks>
public sealed class OffsetPageTokenPagination : IPageTokenPagination
{
    private const string OffsetKey = "offset";

    /// <summary>
    /// Applies offset-based pagination to an in-memory source collection.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="source">Source items to paginate.</param>
    /// <param name="pageSize">Requested page size.</param>
    /// <param name="pageToken">Optional token carrying encoded offset.</param>
    /// <returns>Page slice with items, next token (if any), and total count.</returns>
    public PageSlice<T> Apply<T>(
        IReadOnlyCollection<T> source,
        int pageSize,
        string? pageToken
    )
    {
        var offset = DecodeOffset(pageToken);

        var items = source
            .Skip(offset)
            .Take(pageSize)
            .ToArray();

        var nextPageToken = BuildNextToken(
            offset,
            pageSize,
            source.Count
        );

        return new PageSlice<T>(
            Items: items,
            NextPageToken: nextPageToken,
            TotalCount: source.Count
        );
    }

    /// <summary>
    /// Decodes page token into a numeric offset.
    /// </summary>
    /// <param name="pageToken">Encoded page token.</param>
    /// <returns>Decoded offset or <c>0</c> when token is empty/invalid.</returns>
    private static int DecodeOffset(string? pageToken)
    {
        if (string.IsNullOrWhiteSpace(pageToken))
            return 0;

        try
        {
            var decoded = Convert.FromBase64String(pageToken);
            var json = Encoding.UTF8.GetString(decoded);

            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            return data?.GetValueOrDefault(OffsetKey) ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Builds next-page token based on current offset and page size.
    /// </summary>
    /// <param name="offset">Current offset.</param>
    /// <param name="pageSize">Requested page size.</param>
    /// <param name="totalCount">Total number of items in source.</param>
    /// <returns>Next-page token or <see langword="null"/> when there is no next page.</returns>
    private static string? BuildNextToken(
        int offset,
        int pageSize,
        int totalCount
    )
    {
        var nextOffset = offset + pageSize;
        if (nextOffset >= totalCount)
            return null;

        var payload = new Dictionary<string, int>
        {
            [OffsetKey] = nextOffset
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Convert.ToBase64String(bytes);
    }
}
