using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using egit.ViewModels;

namespace egit.Views
{
    public class View_RepoInfo : UserControl
    {
        public View_RepoInfo()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            if (DataContext is ViewModel_RepoInfo)
            {
                ((ViewModel_RepoInfo)DataContext).RegisterView(this);
            }
        }
    }
}
