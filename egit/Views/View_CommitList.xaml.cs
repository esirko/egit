using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
            DataGridCommitList.LoadingRow += DataGridCommitList_LoadingRow;
        }

        private void DataGridCommitList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            CommitWrapper cw = e.Row.DataContext as CommitWrapper;
            if (cw?.Changelist != null)
            {
                if (int.TryParse(cw.Id, out int intid))
                {
                    ViewUtils.ColorRowByChangelist(e.Row, intid);
                }
            }
        }

        public DataGrid DataGridCommitList;
    }
}
