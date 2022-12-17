﻿#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Screenbox.Core.Messages;
using Screenbox.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System;
using Windows.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.UI;
using Screenbox.Factories;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Screenbox.Controls;
using Screenbox.Core;
using NavigationViewDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode;

namespace Screenbox.ViewModels
{
    internal partial class FolderViewPageViewModel : ObservableRecipient,
        IRecipient<PropertyChangedMessage<NavigationViewDisplayMode>>
    {
        public ObservableCollection<StorageItemViewModel> Items { get; }

        public IReadOnlyList<StorageFolder> Breadcrumbs { get; private set; }

        private bool IsMediaContextRequested => _contextRequested?.Media != null;

        [ObservableProperty] private bool _isEmpty;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private NavigationViewDisplayMode _navigationViewDisplayMode;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlayCommand), nameof(PlayNextCommand), nameof(ShowPropertiesCommand))]
        private StorageItemViewModel? _contextRequested;

        private readonly IFilesService _filesService;
        private readonly INavigationService _navigationService;
        private readonly StorageItemViewModelFactory _storageVmFactory;
        private readonly DispatcherQueueTimer _loadingTimer;
        private bool _isActive;

        public FolderViewPageViewModel(IFilesService filesService, INavigationService navigationService,
            StorageItemViewModelFactory storageVmFactory)
        {
            _filesService = filesService;
            _storageVmFactory = storageVmFactory;
            _navigationService = navigationService;
            _loadingTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _navigationViewDisplayMode = Messenger.Send<NavigationViewDisplayModeRequestMessage>();
            Breadcrumbs = Array.Empty<StorageFolder>();
            Items = new ObservableCollection<StorageItemViewModel>();

            IsActive = true;
        }

        public void Receive(PropertyChangedMessage<NavigationViewDisplayMode> message)
        {
            NavigationViewDisplayMode = message.NewValue;
        }

        public async Task FetchContentAsync(object? parameter)
        {
            _isActive = true;
            switch (parameter)
            {
                case IReadOnlyList<StorageFolder> { Count: > 0 } breadcrumbs:
                    Breadcrumbs = breadcrumbs;
                    await FetchFolderContentAsync(breadcrumbs.Last());
                    break;
                case StorageLibrary library:
                    await FetchFolderContentAsync(library);
                    break;
            }
        }

        public void Clean()
        {
            _isActive = false;
            Items.Clear();
        }

        protected virtual void Navigate(object? parameter = null)
        {
            _navigationService.NavigateExisting(typeof(FolderViewPageViewModel), parameter);
        }

        [RelayCommand(CanExecute = nameof(IsMediaContextRequested))]
        private void Play(StorageItemViewModel item)
        {
            if (item.Media == null) return;
            Messenger.Send(new PlayMediaMessage(item.Media));
        }

        [RelayCommand(CanExecute = nameof(IsMediaContextRequested))]
        private void PlayNext(StorageItemViewModel item)
        {
            if (item.Media == null) return;
            Messenger.SendPlayNext(item.Media);
        }

        [RelayCommand(CanExecute = nameof(IsMediaContextRequested))]
        private async Task ShowPropertiesAsync(StorageItemViewModel item)
        {
            if (item.Media == null) return;
            await item.Media.LoadDetailsAsync();
            ContentDialog propertiesDialog = PropertiesView.GetDialog(item.Media);
            await propertiesDialog.ShowAsync();
        }

        [RelayCommand]
        private void Click(StorageItemViewModel item)
        {
            if (item.Media != null)
            {
                Messenger.Send(new PlayMediaMessage(item.Media));
            }
            else if (item.StorageItem is StorageFolder folder)
            {
                StorageFolder[] crumbs = Breadcrumbs.Append(folder).ToArray();
                Navigate(crumbs);
            }
        }

        private async Task FetchFolderContentAsync(StorageFolder folder)
        {
            Items.Clear();

            StorageItemQueryResult itemQuery = _filesService.GetSupportedItems(folder);
            uint fetchIndex = 0;
            while (_isActive)
            {
                _loadingTimer.Debounce(() => IsLoading = true, TimeSpan.FromMilliseconds(800));
                IReadOnlyList<IStorageItem> items = await itemQuery.GetItemsAsync(fetchIndex, 30);
                if (items.Count == 0) break;
                fetchIndex += (uint)items.Count;
                foreach (IStorageItem storageItem in items)
                {
                    StorageItemViewModel item = _storageVmFactory.GetTransient(storageItem);
                    Items.Add(item);
                }
            }

            _loadingTimer.Stop();
            IsLoading = false;
            IsEmpty = Items.Count == 0;
            if (!_isActive) return;
            IEnumerable<Task> loadingTasks = Items.Select(item =>
                item.Media != null && !string.IsNullOrEmpty(item.Path)
                    ? Task.WhenAll(item.UpdateCaptionAsync(), item.Media.LoadThumbnailAsync())
                    : item.UpdateCaptionAsync());

            await Task.WhenAll(loadingTasks);
        }

        private async Task FetchFolderContentAsync(StorageLibrary library)
        {
            if (library.Folders.Count <= 0)
            {
                IsEmpty = true;
                return;
            }

            if (library.Folders.Count == 1)
            {
                // StorageLibrary is always the root
                // Fetch content of the only folder if applicable
                StorageFolder folder = library.Folders[0];
                Breadcrumbs = new[] { folder };
                await FetchFolderContentAsync(folder);
            }
            else
            {
                Items.Clear();
                foreach (StorageFolder folder in library.Folders)
                {
                    StorageItemViewModel item = _storageVmFactory.GetTransient(folder);
                    Items.Add(item);
                    await item.UpdateCaptionAsync();
                }

                IsEmpty = Items.Count == 0;
            }
        }
    }
}
