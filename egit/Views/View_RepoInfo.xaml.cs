using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

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

            var fontComboBox = this.Find<ComboBox>("CBTest");
            //fontComboBox.Items = new List<FontFamily>() { new FontFamily("Arial"), new FontFamily("Times New Roman") }; // FontManager.Current.GetInstalledFontFamilyNames().Select(x => new FontFamily(x));
            fontComboBox.Items = new List<string>() { "Text A", "Text B" };
            fontComboBox.SelectedIndex = 0;
        }
    }
}
