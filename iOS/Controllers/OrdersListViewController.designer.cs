// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ReactiveX.iOS
{
	[Register ("OrdersListViewController")]
	partial class OrdersListViewController
	{
		[Outlet]
		UIKit.UIBarButtonItem refreshBarButtonItem { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (refreshBarButtonItem != null) {
				refreshBarButtonItem.Dispose ();
				refreshBarButtonItem = null;
			}
		}
	}
}
