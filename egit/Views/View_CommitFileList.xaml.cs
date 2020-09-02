using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using egit.Engine;
using egit.Models;
using MessageBox.Avalonia;
using ReactiveUI;

namespace egit.Views
{
    class HackyCommand : ICommand
    {
        public HackyCommand(Action onEnterKeyPressed)
        {
            OnEnterKeyPressed = onEnterKeyPressed;
        }

        Action OnEnterKeyPressed;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OnEnterKeyPressed();
        }
    }

    public class View_CommitFileList : UserControl
    {
        public View_CommitFileList()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            MyDataGrid = this.Find<DataGrid>("DataGrid");

            // This hacky routing of the Enter key press seems to be the only way I can intercept Enter key presses on Avalonia's DataGrid. 
            // It doesn't support PreviewKeyDown which is apparently a simpler workaround in WPF.
            MyDataGrid.KeyBindings.Add(new KeyBinding()
            {
                Gesture = new KeyGesture(Key.Enter),
                Command = new HackyCommand(HandleEnterKeyPressed),
            });
            MyDataGrid.LoadingRow += MyDataGrid_LoadingRow;

            MyContextMenu = this.Find<ContextMenu>("MyContextMenu");
            MyContextMenu.ContextMenuOpening += MyContextMenu_ContextMenuOpening;

            MenuItem cmDiff= this.Find<MenuItem>("CMDiff");
            cmDiff.Click += CmDiff_Click;

            MenuItem cmFindInFileTree = this.Find<MenuItem>("CMFindInFileTree");
            cmFindInFileTree.Click += CmFindInFileTree_Click;

            MenuItem cmFindInExplorer = this.Find<MenuItem>("CMFindInExplorer");
            cmFindInExplorer.Click += CmFindInExplorer_Click;

            CMMoveToChangelist = this.Find<MenuItem>("CMMoveToChangelist");

            MenuItem cmSubmit= this.Find<MenuItem>("CMSubmit");
            cmSubmit.Click += CmSubmit_Click;

            MenuItem cmRevert = this.Find<MenuItem>("CMRevert");
            cmRevert.Click += CmRevert_Click;

        }

        private void MyContextMenu_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CMMoveToChangelist.IsEnabled = MyDataGrid.SelectedItems.Count > 0;

            List<MenuItem> subMenu = new List<MenuItem>();
            List<Changelist> changelists = GitEngine.Get().ModelTransient.Changelists;

            for (int i = 0; i < GitEngine.Get().ModelTransient.Changelists.Count; i++)
            {
                MenuItem mi = new MenuItem() { Header = $"{i} ({changelists[i].Files.Count} file[s]) {changelists[i].Description}", Tag = i};
                mi.Click += CmSubMenuMoveToChangelist_Click;
                subMenu.Add(mi);
            }

            MenuItem mi2 = new MenuItem() { Header = $"<New changelist...>", Tag = -1};
            mi2.Click += CmSubMenuMoveToChangelist_Click;
            subMenu.Add(mi2);

            CMMoveToChangelist.Items = subMenu;
        }

        private void CmSubMenuMoveToChangelist_Click(object sender, RoutedEventArgs e)
        {
            int changelistId = (int)(sender as MenuItem).Tag;
            string changelistDescription = null;

            if (changelistId == -1)
            {
                changelistId = -1;
                changelistDescription = "new changelist";
                MessageBoxManager.GetMessageBoxStandardWindow("", "TODO: need an input box to specify this").Show();
                return;
                /*
                if (InputBox.Show("New changelist", "Enter a description for the new changelist", ref changelistDescription) != DialogResult.OK)
                {
                    return;
                }
                */
            }

            GitEngine.Get().MoveFilesToChangelist(GetSelectedFiles(), changelistId, changelistDescription);

        }

        private List<FileAndStatus> GetSelectedFiles()
        {
            List<FileAndStatus> files = new List<FileAndStatus>();
            IList selectedItems = MyDataGrid.SelectedItems;
            for (int i = 0; i < selectedItems.Count; i++)
            {
                files.Add(selectedItems[i] as FileAndStatus);
            }
            return files;
        }

        private FileAndStatus GetFirstSelectedFile()
        {
            IList selectedItems = MyDataGrid.SelectedItems;
            if (selectedItems.Count > 0)
            {
                return selectedItems[0] as FileAndStatus;
            }
            return null;
        }

        private void CmDiff_Click(object sender, RoutedEventArgs e)
        {
            SpawnExternalDiff();
        }

        private void CmFindInFileTree_Click(object sender, RoutedEventArgs e)
        {
            FileAndStatus selectedFile = GetFirstSelectedFile();
            if (selectedFile != null)
            {
                GitEngine.Get().SelectFileInFileTree(selectedFile.FileName);
            }
        }

        private void CmFindInExplorer_Click(object sender, RoutedEventArgs e)
        {
            FileAndStatus selectedFile = GetFirstSelectedFile();
            if (selectedFile != null)
            {
                // TODO: this doesn't work on windows, not sure why, it almost does though.
                Process.Start("explorer.exe", "/select, " + Path.Combine(GitEngine.Get().Repo.Info.WorkingDirectory, selectedFile.FileName));
            }
        }

        private void CmSubmit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CmRevert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MyDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            FileAndStatus fileAndStatus = e.Row.DataContext as FileAndStatus;
            if (fileAndStatus != null)
            {
                int changelistIndex = GitEngine.Get().ModelTransient.GetChangelistForFile(fileAndStatus.FileName);
                ViewUtils.ColorRowByChangelist(e.Row, changelistIndex);
            }
        }

        private void SpawnExternalDiff()
        {
            GitEngine.Get().SpawnExternalDiff(GetSelectedFiles());
        }

        void HandleDoubleClick(object sender, RoutedEventArgs args)
        {
            SpawnExternalDiff();
        }

        private void HandleEnterKeyPressed()
        {
            HandleKeyDown(MyDataGrid, new KeyEventArgs() { Key = Key.Enter });
        }

        void HandleKeyDown(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Enter:
                    SpawnExternalDiff();
                    break;
            }
        }

        DataGrid MyDataGrid;
        ContextMenu MyContextMenu;
        MenuItem CMMoveToChangelist;
    }
}
