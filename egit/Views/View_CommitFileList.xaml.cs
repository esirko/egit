using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using egit.Engine;
using egit.Models;
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
        }

        private void SpawnExternalDiff()
        {
            List<FileAndStatus> files = new List<FileAndStatus>();
            IList selectedItems = MyDataGrid.SelectedItems;
            for (int i = 0; i < selectedItems.Count; i++)
            {
                files.Add(selectedItems[i] as FileAndStatus);
            }
            GitEngine.Get().SpawnExternalDiff(files);
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
    }
}
