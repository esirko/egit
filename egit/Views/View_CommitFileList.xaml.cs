using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
}
