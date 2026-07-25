namespace Ryze.Domain.Shared.Enum;

/// <summary>
/// Role assigned to a wallet owner.
/// </summary>
public enum OwnerRole
{
    /// <summary>
    /// Owner role is not specified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Primary owner with full control.
    /// </summary>
    Primary,

    /// <summary>
    /// Secondary owner with delegated permissions.
    /// </summary>
    Secondary,

    /// <summary>
    /// Authorized user with limited operational permissions.
    /// </summary>
    AuthorizedUser,

    /// <summary>
    /// Readonly user with no modification rights.
    /// </summary>
    ViewOnly
}
