using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace egit.Views
{
    public class View_CommitInfo : UserControl
    {
        public View_CommitInfo()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
