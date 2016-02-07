using System;
using Plugin.Connectivity;
using ReactiveUI;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Plugin.Connectivity.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

namespace ReactiveX
{
	public class OrdersListViewModel : ReactiveObject
	{
		// Demo flag.
		static int count = 1;

		public ReactiveList<OrderViewModel> Orders { get; protected set; }
		public ReactiveCommand<List<OrderViewModel>> LoadOrdersCommand { get; protected set; }

		OrdersWebRepository _ordersRepository;
		IConnectivity _connectivity;

		bool _canLoadOrders;
		protected bool CanLoadOrders
		{
			get {
				return _canLoadOrders;
			}
			set {
				this.RaiseAndSetIfChanged(ref _canLoadOrders, value);
			}  
		}

		IDisposable _connectivityChangedDisposable;
		IDisposable _canLoadOrdersDisposable;
		IDisposable _loadOrdersCommandDisposable;

		public OrdersListViewModel()
		{
			_ordersRepository = new OrdersWebRepository ();
			_connectivity = CrossConnectivity.Current;
			Orders = new ReactiveList<OrderViewModel>();

			// Initial connectivity availability
			CanLoadOrders = _connectivity.IsConnected;

			// Convert a .NET event to an observable 
			// and subscribe to an observer *anonymous delegate extension*.
			IObservable<EventPattern<ConnectivityChangedEventArgs>> connectivityChangedObservable = 
				Observable.FromEventPattern<ConnectivityChangedEventArgs>(
					_connectivity, 
					"ConnectivityChanged",
					RxApp.MainThreadScheduler);

			// When the IConnectivity.ConnectivityChanged event is raised
			// the observable will push me the ConnectivityChangedEventArgs.
			_connectivityChangedDisposable = connectivityChangedObservable.Subscribe(evt => {
				// Set if we can load orders
				CanLoadOrders = evt.EventArgs.IsConnected;
			});

			// Cool stuff! ReactiveUI offers some Rx helpers.
			// When the CanLoadOrders property changes let me know.
			IObservable<bool> canLoadOrdersObservable = 
				this.WhenAny(x => x.CanLoadOrders, x => x.Value);

			// More Cool stuff! ReactiveCommands have built-in support for background
			// operations. RxCmd guarantees that this block will only run exactly
			// once at a time, and that the CanExecute will auto-disable while it
			// is running.
			LoadOrdersCommand = ReactiveCommand.CreateAsyncTask(
				canLoadOrdersObservable,
				async _ => 
				{
					return await _ordersRepository.GetAsync();;
				});

			// And if that is not Cool stuff! ReactiveCommands are themselves IObservables, whose value
			// are the results from the async method, guaranteed to arrive on the UI
			// thread. We're going to take the list of teams that the background
			// operation loaded, and put them into our TeamList.
			_loadOrdersCommandDisposable = LoadOrdersCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(
				orders => {
					foreach (OrderViewModel order in orders)
					{
						order.OrderNumber = string.Format("{0} - {1}", order.OrderNumber, count);
					}
					count++;
					Orders.InsertRange(0, orders);
				},
				ex => {
					UserError.Throw("Fetching orders exception: " + ex.Message, ex);
				});

			// Niiiiice! Whenever the CanLoadOrders changes, we're going to wait
			// for one second of "dead airtime", then invoke the LoadOrdersCommand
			// command.
			_canLoadOrdersDisposable = canLoadOrdersObservable
				.Where(x => x)
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.InvokeCommand(this, x => x.LoadOrdersCommand);
		}

		public void Dispose()
		{
			_connectivityChangedDisposable.Dispose ();
			_loadOrdersCommandDisposable.Dispose ();
			_canLoadOrdersDisposable.Dispose ();
		}
	}
}