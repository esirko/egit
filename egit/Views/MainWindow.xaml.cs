using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using egit.Engine;

namespace egit.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            /*
            View_CommitList mainCommitList = this.Find<View_CommitList>("MainCommitList");
            View_CommitList secondaryCommitList = this.Find<View_CommitList>("SecondaryCommitList");
            GitEngine.Get().RegisterCommitLists(mainCommitList, secondaryCommitList);
            */
        }
    }
}
