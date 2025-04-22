namespace Ethik.Utility.CQRS
{
    public interface IRequestDispatcher
    {
        Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken);
    }
}