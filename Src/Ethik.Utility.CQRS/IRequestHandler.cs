namespace Ethik.Utility.CQRS;

public interface IRequestHandler<in TRequest, TResult>
{
    Task<TResult> Handle(TRequest request, CancellationToken cancellation);
}
