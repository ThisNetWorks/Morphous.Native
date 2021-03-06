// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using Foundation;
using Morphous.Native.iOS.UI;
using Morphous.Native.Models;
using UIKit;

namespace Morphous.Native.iOS
{
    public partial class TermPartView : ElementView<ITermPart>
	{
		public TermPartView (IntPtr handle) : base (handle)
		{
		}

        protected override void Bind()
        {
            base.Bind();

            var tableView = DisplayContext.RootView as UITableView;

            if (tableView == null)
            {
                throw new InvalidCastException("The RootView of the current DisplayContext must be a UITableView for TermPart to work");
            }

            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 200;
            tableView.Source = new ItemsSource(DisplayContext, Element.ContentItems);
        }

        private class ItemsSource : UITableViewSource
        {
            private readonly DisplayContext _displayContext;
            private readonly IList<IContentItem> _items;

            public ItemsSource(DisplayContext displayContext, IList<IContentItem> items)
            {
                _displayContext = displayContext;
                _items = items;
            }

            public override nint RowsInSection(UITableView tableview, nint section) => _items.Count;

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var item = _items[indexPath.Row];
                var cell = new ContentItemCell(_displayContext, item, "termcellid");

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var contentItem = _items[indexPath.Row];

                var contentViewController = MphIOS.ContentItemViewController(contentItem);

                _displayContext.ViewController.ShowViewController(contentViewController, this);
            }
        }
    }
}
