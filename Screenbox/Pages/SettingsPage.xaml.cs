﻿using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;
using Screenbox.Core.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Screenbox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        internal SettingsPageViewModel ViewModel => (SettingsPageViewModel)DataContext;

        internal CommonViewModel Common { get; }
        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<SettingsPageViewModel>();
            Common = Ioc.Default.GetRequiredService<CommonViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadLibraryLocations();
        }

        private void ContentRoot_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;
            ButtonBase? settingsCard = panel.Children.OfType<ButtonBase>().FirstOrDefault();
            if (settingsCard == null) return;
            IEnumerable<TextBlock> sectionHeaders = panel.Children.OfType<TextBlock>();
            foreach (TextBlock sectionHeader in sectionHeaders)
            {
                sectionHeader.Width = settingsCard.ActualWidth;
            }
        }
    }
}
