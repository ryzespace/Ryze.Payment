using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Transfer.Contexts;

/// <summary>
/// Defines common contract for transfer operation contexts.
/// </summary>
/// <remarks>
/// Transfer contexts carry operation specific data required during execution
/// of transfer related application workflows.
///
/// Contexts are consumed by the mutation pipeline and provide strongly typed
/// boundary between application commands and domain operations. Each concrete
/// implementation represents specific transfer action, for example creation,
/// confirmation, cancellation, or reversal.
///
/// The interface intentionally does not expose members directly because the
/// shared context lifecycle and metadata are provided by <see cref="IContext"/>.
/// </remarks>
public interface ITransferContext : IContext
{
}