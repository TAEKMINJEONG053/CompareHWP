using System.ComponentModel;

namespace CompareHWP.Services
{
    public class BusyService : INotifyPropertyChanged
    {
        public static BusyService Instance { get; } = new BusyService();

        /// <summary>
        /// 외부 new 방지
        /// </summary>
        private BusyService() { }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
