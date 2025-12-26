using System.Windows.Controls;

namespace CompareHWP.View
{
    /// <summary>
    /// CheckDistance.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CheckDistance : UserControl
    {
        public CheckDistance()
        {
            InitializeComponent();

            DataContext = new ViewModel.CheckDistanceVM();
        }
    }
}
