﻿using System.Numerics;
using Telegram.Common;
using Telegram.Controls.Cells;
using Telegram.Controls.Media;
using Telegram.ViewModels.Stories;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace Telegram.Views.Profile
{
    public sealed partial class ProfileStoriesTabPage : ProfileTabPage
    {
        public ProfileStoriesTabPage()
        {
            InitializeComponent();

            ElementCompositionPreview.SetIsTranslationEnabled(ManagePanel, true);
            ScrollingHost.RegisterPropertyChangedCallback(ListViewBase.SelectionModeProperty, OnSelectionModeChanged);
        }

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer == null)
            {
                args.ItemContainer = new GridViewItem();
                args.ItemContainer.Style = sender.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = sender.ItemTemplate;
                args.ItemContainer.ContextRequested += OnContextRequested;
            }

            args.IsContainerPrepared = true;
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var element = sender as FrameworkElement;
            var story = ScrollingHost.ItemFromContainer(sender) as StoryViewModel;

            if (story == null)
            {
                return;
            }

            var flyout = new MenuFlyout();

            if (story.CanToggleIsPinned)
            {
                flyout.CreateFlyoutItem(ViewModel.StoriesTab.ToggleStory, story, story.IsPinned ? Strings.ArchiveStory : Strings.SaveToProfile, story.IsPinned ? Icons.StoriesPinnedOff : Icons.StoriesPinned);
            }

            if (story.CanBeDeleted)
            {
                flyout.CreateFlyoutItem(ViewModel.StoriesTab.DeleteStory, story, Strings.Delete, Icons.Delete, destructive: true);
            }

            flyout.CreateFlyoutSeparator();

            if (flyout.Items.Count > 0)
            {
                flyout.CreateFlyoutItem(ViewModel.StoriesTab.SelectStory, story, Strings.Select, Icons.CheckmarkCircle);
            }

            args.ShowAt(flyout, element);
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            var story = args.Item as StoryViewModel;
            var content = args.ItemContainer.ContentTemplateRoot as StoryCell;

            content.Update(story);
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = ScrollingHost.ContainerFromItem(e.ClickedItem) as SelectorItem;
            var transform = container.TransformToVisual(Window.Current.Content);

            var point = transform.TransformPoint(new Point());
            var origin = new Rect(point.X, point.Y, container.ActualWidth, container.ActualHeight);

            ViewModel.StoriesTab.OpenStory(e.ClickedItem as StoryViewModel, origin, GetOrigin);
        }

        private Rect GetOrigin(ActiveStoriesViewModel activeStories)
        {
            var container = ScrollingHost.ContainerFromItem(activeStories.SelectedItem) as SelectorItem;
            if (container != null)
            {
                var transform = container.TransformToVisual(Window.Current.Content);
                var point = transform.TransformPoint(new Point());

                return new Rect(point.X, point.Y, container.ActualWidth, container.ActualHeight);
            }

            return Rect.Empty;
        }

        private void OnSelectionModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            ShowHideManagePanel(ScrollingHost.SelectionMode == ListViewSelectionMode.Multiple);
        }

        private bool _manageCollapsed = true;

        private void ShowHideManagePanel(bool show)
        {
            if (_manageCollapsed != show)
            {
                return;
            }

            _manageCollapsed = !show;
            ManagePanel.Visibility = Visibility.Visible;

            var manage = ElementCompositionPreview.GetElementVisual(ManagePanel);
            manage.Opacity = show ? 0 : 1;

            var batch = manage.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, args) =>
            {
                manage.Opacity = show ? 1 : 0;
                ManagePanel.Visibility = show
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };

            var offset1 = manage.Compositor.CreateVector3KeyFrameAnimation();
            offset1.InsertKeyFrame(show ? 0 : 1, new Vector3(0, 48, 0));
            offset1.InsertKeyFrame(show ? 1 : 0, new Vector3(0, 0, 0));

            var opacity1 = manage.Compositor.CreateScalarKeyFrameAnimation();
            opacity1.InsertKeyFrame(show ? 0 : 1, 0);
            opacity1.InsertKeyFrame(show ? 1 : 0, 1);

            manage.StartAnimation("Translation", offset1);
            manage.StartAnimation("Opacity", opacity1);

            batch.End();
        }

        #region Binding

        private string ConvertSelected(int count)
        {
            return Locale.Declension(Strings.R.StoriesSelected, count);
        }

        private string ConvertToggleIcon(bool pinned)
        {
            return pinned ? Icons.StoriesPinnedOff : Icons.StoriesPinned;
        }

        private string ConvertToggleText(bool pinned, int count)
        {
            if (pinned)
            {
                return Locale.Declension(Strings.R.ArchiveStories, count);
            }

            return Strings.SaveToProfile;
        }

        private string ConvertEmptyTitle(bool pinned)
        {
            return pinned ? Strings.NoPublicStoriesTitle : Strings.NoArchivedStoriesTitle;
        }

        private string ConvertEmptySubtitle(bool pinned)
        {
            return pinned ? Strings.NoStoriesSubtitle : Strings.NoArchivedStoriesSubtitle;
        }

        #endregion

    }
}
