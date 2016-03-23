﻿using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace MesLectures
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            this.MainFrame.Navigate(typeof (Books));
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(EditionPage));
            this.MySplitView.IsPaneOpen = false;
        }

        private void OneDriveButtonClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(OnedrivePage));
            this.MySplitView.IsPaneOpen = false;
        }

        private void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(SettingsPage));
            this.MySplitView.IsPaneOpen = false;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void ButtonBooksClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(Books));
            this.MySplitView.IsPaneOpen = false;
        }

        private void CompactHamburgerButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
    }
}