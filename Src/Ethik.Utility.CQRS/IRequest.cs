namespace Ethik.Utility.CQRS;

public interface IRequest<out TResult>
{ 
}



/* 
 * IRequestDispatcher _dispatcher;
 * _dispatcher.Send(new SomeCommand());
 * 
 * */