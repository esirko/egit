using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using egit.Engine;
using egit.Models;
using egit.ViewModels;

namespace egit.Views
{
    public class View_CommitFileList : UserControl
    {
        public View_CommitFileList()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void HandleDoubleClick(object sender, RoutedEventArgs args)
        {
            List<FileAndStatus> files = new List<FileAndStatus>();
            IList selectedItems = (sender as DataGrid).SelectedItems;
            for (int i = 0; i < selectedItems.Count; i++)
            {
                files.Add(selectedItems[i] as FileAndStatus);
            }
            GitEngine.Get().SpawnExternalDiff(files);
        }
    }
}
