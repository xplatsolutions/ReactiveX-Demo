using System;

using Foundation;
using UIKit;
using ReactiveUI;
using System.Reactive.Linq;

namespace ReactiveX.iOS
{
	public partial class OrderCell : ReactiveTableViewCell, IViewFor<OrderViewModel>
	{
		public static readonly NSString Key = new NSString ("OrderCell");
		public static readonly UINib Nib;

		static OrderCell ()
		{
			Nib = UINib.FromName ("OrderCell", NSBundle.MainBundle);
		}

		OrderViewModel _viewModel;
		public OrderViewModel ViewModel {
			get { return _viewModel; }
			set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
		}

		object IViewFor.ViewModel {
			get { return _viewModel; }
			set { ViewModel = (OrderViewModel)value; }
		}

		public OrderCell (IntPtr handle) : base (handle)
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			// When the ViewModel property is set to not null let me know.
			this.WhenAny(x => x.ViewModel, x => x.Value)
				.Where(x => x != null)
				.Subscribe(x => { 
					// OneWayBind the ViewModel.OrderNumber to the OrderCell.TextLabel.Text
					this.OneWayBind(ViewModel, vm => vm.OrderNumber, v => v.TextLabel.Text);
				});
			
		}

		public static OrderCell Create()
		{
			return (OrderCell)Nib.Instantiate(null, null)[0];
		}
	}
}