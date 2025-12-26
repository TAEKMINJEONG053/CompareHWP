using CompareHWP.Common;
using CompareHWP.CommonViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CompareHWP.CommonView
{
    /// <summary>
    /// IOSMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOSMessageBox : Window
    {
        public IOSMessageBox()
        {
            InitializeComponent();
        }

        public static bool? Show(
        string message,
        string title = "",
        IOSMessageBoxIcon icon = IOSMessageBoxIcon.None,
        int autoCloseSeconds = 0)
        {
            var window = new IOSMessageBox();

            // 🔥 핵심 1: Owner 지정
            window.Owner = Application.Current?.MainWindow;

            window.DataContext = new IOSMessageBoxViewModel(
                title,
                message,
                icon,
                autoCloseSeconds,
                result =>
                {
                    window.DialogResult = result;
                    window.Close();
                });

            // 🔥 핵심 2: ShowDialog
            return window.ShowDialog();
        }


        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove(); // 🧲 드래그 이동
        }
    }
}
