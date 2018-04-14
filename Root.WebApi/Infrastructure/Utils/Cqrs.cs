using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Quarks.CQRS.Impl;

namespace Quarks.CQRS
{
    public interface IRxCommandDispatcher : ICommandDispatcher
    {
        IObservable<Unit> Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }

    public interface IRxQueryDispatcher : IQueryDispatcher
    {
        IObservable<TResult> Dispatch<TResult>(IQuery<TResult> query);
    }

    public class RxCommandDispatcher : CommandDispatcher, IRxCommandDispatcher
    {
        private readonly SynchronizationContext _ctx;

        public RxCommandDispatcher(ICommandHandlerFactory commandHandlerFactory, SynchronizationContext ctx) : base(commandHandlerFactory)
        {
            _ctx = ctx;
        }

        public IObservable<Unit> Dispatch<TCommand>(TCommand command) where TCommand : ICommand
        {
            return DispatchAsync(command, CancellationToken.None).ToObservable().ObserveOn(_ctx);
        }
    }

    public class RxQueryDispatcher : QueryDispatcher, IRxQueryDispatcher
    {
        private readonly SynchronizationContext _ctx;

        public RxQueryDispatcher(IQueryHandlerFactory commandHandlerFactory, SynchronizationContext ctx) : base(commandHandlerFactory)
        {
            _ctx = ctx;
        }

        public IObservable<TResult> Dispatch<TResult>(IQuery<TResult> query)
        {
            return DispatchAsync(query, CancellationToken.None).ToObservable().ObserveOn(_ctx);
        }
    }
}
