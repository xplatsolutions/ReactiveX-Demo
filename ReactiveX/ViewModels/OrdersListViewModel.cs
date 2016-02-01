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
		static int count = 1;
		public OrdersListViewModel()
		{
			_ordersRepository = new OrdersWebRepository ();
			_connectivity = CrossConnectivity.Current;
			CanLoadOrders = _connectivity.IsConnected;
			Orders = new ReactiveList<OrderViewModel>();

			// CoolStuff: We're describing here, in a *declarative way*, the
			// conditions in which the LoadTeamList command is enabled. Now,
			// our Command IsEnabled is perfectly efficient, because we're only
			// updating the UI in the scenario when it should change.
			IObservable<EventPattern<ConnectivityChangedEventArgs>> connectivityChangedObservable = 
				Observable.FromEventPattern<ConnectivityChangedEventArgs>(
					_connectivity, 
					"ConnectivityChanged",
					RxApp.MainThreadScheduler);

			connectivityChangedObservable.Subscribe(evt => {
				CanLoadOrders = evt.EventArgs.IsConnected;
			});

			IObservable<bool> canLoadOrdersObservable = 
				this.WhenAny(x => x.CanLoadOrders, x => x.Value);

			// CoolStuff: ReactiveCommands have built-in support for background
			// operations. RxCmd guarantees that this block will only run exactly
			// once at a time, and that the CanExecute will auto-disable while it
			// is running.
			LoadOrdersCommand = ReactiveCommand.CreateAsyncTask(
				canLoadOrdersObservable,
				async _ => 
				{
					return await _ordersRepository.GetAsync();;
				});

			// CoolStuff: ReactiveCommands are themselves IObservables, whose value
			// are the results from the async method, guaranteed to arrive on the UI
			// thread. We're going to take the list of teams that the background
			// operation loaded, and put them into our TeamList.
			LoadOrdersCommand.Subscribe(
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

			// CoolStuff: Whenever the Email address changes, we're going to wait
			// for one second of "dead airtime", then invoke the LoadTeamList
			// command.
			canLoadOrdersObservable
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.InvokeCommand(this, x => x.LoadOrdersCommand);
		}
	}
}