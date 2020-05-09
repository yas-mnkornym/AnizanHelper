using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace AnizanHelper.Views.Behaviors
{
	public class DataGridBindableSelectedItemsBehavior : Behavior<DataGrid>
	{
		#region
		private static DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
			"SelectedItems",
			typeof(IList),
			typeof(DataGridBindableSelectedItemsBehavior),
			new PropertyMetadata(null, (d, e) =>
			{
				var behavior = d as DataGridBindableSelectedItemsBehavior;
				if (behavior == null) { return; }

				var newValue = e.NewValue as IList;

				if (behavior.disp_ != null)
				{
					behavior.disp_.Dispose();
					behavior.disp_ = null;
				}

				var collection = newValue as INotifyCollectionChanged;
				if (collection != null)
				{
					try
					{
						behavior.isUpdating_ = true;
						foreach (var item in newValue)
						{
							behavior.AssociatedObject.SelectedItems.Add(item);
						}
					}
					finally
					{
						behavior.isUpdating_ = false;
					}

					collection.CollectionChanged += behavior.collection_CollectionChanged;
					behavior.disp_ = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(collection, "CollectionChanged")
						.Subscribe(args =>
						{
							behavior.collection_CollectionChanged(args.Sender, args.EventArgs);
						});
				}
			}));
		private bool isUpdating_ = false;
		private IDisposable disp_ = null;
		public IList SelectedItems
		{
			get
			{
				return (IList)this.GetValue(SelectedItemsProperty);
			}
			set
			{
				this.SetValue(SelectedItemsProperty, value);
			}
		}

		private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.isUpdating_) { return; }

			this.isUpdating_ = true;
			try
			{
				if (e.Action == NotifyCollectionChangedAction.Reset)
				{
					this.AssociatedObject.SelectedItems.Clear();
				}
				else
				{
					if (e.OldItems != null)
					{
						foreach (var item in e.OldItems)
						{
							this.AssociatedObject.SelectedItems.Remove(item);
						}
					}

					if (e.NewItems != null)
					{
						foreach (var item in e.NewItems)
						{
							this.AssociatedObject.SelectedItems.Add(item);
						}
					}
				}
			}
			finally
			{
				this.isUpdating_ = false;
			}
		}
		#endregion

		#region ビヘイビアOverride
		protected override void OnAttached()
		{
			base.OnAttached();
			this.AssociatedObject.SelectionChanged += this.AssociatedObject_SelectionChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
		}
		#endregion


		private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.isUpdating_) { return; }
			foreach (var item in e.AddedItems)
			{
				this.SelectedItems.Add(item);
			}
			foreach (var item in e.RemovedItems)
			{
				this.SelectedItems.Remove(item);
			}
		}
	}
}
