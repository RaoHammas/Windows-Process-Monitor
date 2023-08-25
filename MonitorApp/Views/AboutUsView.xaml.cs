using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MonitorApp.Views
{
    /// <summary>
    /// Interaction logic for AboutUsView.xaml
    /// </summary>
    public partial class AboutUsView : UserControl
    {
        public AboutUsView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var objProcessInfo = new ProcessStartInfo
                {
                    FileName = "https://github.com/RaoHammas/Windows-Process-Monitor",
                    UseShellExecute = true
                };

                Process.Start(objProcessInfo);
            }
            catch (Exception ex)
            {
            }
        }
    }
}