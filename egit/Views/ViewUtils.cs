using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;

namespace egit.Views
{
    class ViewUtils
    {
        static List<IBrush> ChangelistColors = new List<IBrush> { Brushes.Pink, Brushes.SkyBlue, Brushes.LightGreen, Brushes.Yellow, Brushes.Orange,
            Brushes.PaleVioletRed, Brushes.Cyan, Brushes.Magenta, Brushes.Lime,
            Brushes.MediumVioletRed, Brushes.Teal, Brushes.Lavender,
            Brushes.SandyBrown, Brushes.Beige,
            Brushes.Sienna, Brushes.MintCream, Brushes.Olive, Brushes.NavajoWhite,
            Brushes.AliceBlue,
            Brushes.Gray};

        public static void ColorRowByChangelist(DataGridRow row, int changelistIndex)
        {
            if (changelistIndex > 0)
            {
                row.Background = ChangelistColors[(changelistIndex - 1) % ChangelistColors.Count];
            }
        }

    }
}
