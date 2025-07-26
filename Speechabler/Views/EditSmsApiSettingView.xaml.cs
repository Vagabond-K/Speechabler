using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Speechabler.Views
{
    [View]
    public partial class EditSmsApiSettingView : UserControl
    {
        public EditSmsApiSettingView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
