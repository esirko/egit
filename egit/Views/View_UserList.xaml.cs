using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace egit.Views
{
    public class View_UserList : UserControl
    {
        public View_UserList()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
