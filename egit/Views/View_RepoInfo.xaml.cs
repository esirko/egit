using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
}
