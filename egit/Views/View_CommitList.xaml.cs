using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace egit.Views
{
    public class View_CommitList : UserControl
    {
        public View_CommitList()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
