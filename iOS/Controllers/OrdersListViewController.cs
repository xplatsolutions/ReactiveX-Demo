using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ReactiveUI;
using System.Linq;

namespace ReactiveX.iOS
{
	partial class OrdersListViewController: ReactiveTableViewController, IViewFor<OrdersListViewModel>
	{
		OrdersListViewModel _viewModel;
		public OrdersListViewModel ViewModel 
		{
			get { return _viewModel; }
			set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
		}

		object IViewFor.ViewModel {
			get { return _viewModel; }
			set { ViewModel = (OrdersListViewModel)value; }
		}

		public OrdersListViewController (IntPtr handle) : base (handle)
		{

		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewModel = new OrdersListViewModel();

			this.BindCommand (ViewModel, vm => vm.LoadOrdersCommand, v => v.refreshBarButtonItem);

			TableView.RegisterNibForCellReuse(OrderCell.Nib, OrderCell.Key);
			TableView.Source = new ReactiveTableViewSource<OrderViewModel>(
				TableView,
				ViewModel.Orders,
				OrderCell.Key,
				40.0f, 
				cell => {
//					Console.WriteLine(cell);
			});
		}
	}
}
