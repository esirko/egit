using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using egit.Models;
using egit.ViewModels;

namespace egit.Views
{
    public class View_CommitList : UserControl
    {
        public View_CommitList()
        {
            this.InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            // Hack to get around inability to data-bind to SelectedItems
            if (DataContext is ViewModel_CommitList)
            {
                ((ViewModel_CommitList)DataContext).RegisterView(this);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            DataGridCommitList = this.Find<DataGrid>("DataGrid_CommitList");
            //DataGridCommitList.SelectionChanged += DataGridCommitList_SelectionChanged;
        }

        /*
        public List<CommitWrapper> SelectedItems = new List<CommitWrapper>();
        private void DataGridCommitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems.Clear();
            foreach (CommitWrapper cw in DataGridCommitList.SelectedItems)
            {
                SelectedItems.Add(cw);
            }
        }
        */

        public DataGrid DataGridCommitList;
    }
}
