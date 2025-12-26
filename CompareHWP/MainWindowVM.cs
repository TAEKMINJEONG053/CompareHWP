using CompareHWP.Helper;
using CompareHWP.Services;
using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CompareHWP
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly BusyService _busyService = BusyService.Instance;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value, nameof(IsBusy));
        }

        private ObservableCollection<string> _fileList;
        public ObservableCollection<string> FileList
        {
            get => _fileList;
            set => SetProperty(ref _fileList, value, nameof(FileList));
        }

        private List<string> _selectedFileList;

        public ICommand ClearButtonClickCommand { get; }

        public MainWindowVM()
        {
            // BusyService 변경 → VM에 반영
            _busyService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BusyService.IsBusy))
                {
                    // 초기값 동기화
                    IsBusy = _busyService.IsBusy;
                }
            };

            FileList = new ObservableCollection<string>();

            ClearButtonClickCommand = new RelayCommand(ClearButtonClick);
        }

        public void UpdateSelectedItems(List<string> selectedItems)
        {
            _selectedFileList = selectedItems;
        }

        private void ClearButtonClick(object parameter)
        {
            if (parameter.ToString() == "ALL")
            {
                _fileList.Clear();
                _selectedFileList.Clear();
            }
            else
            {
                foreach (var selectedFile in _selectedFileList)
                {
                    _fileList.Remove(selectedFile);
                }

                _selectedFileList.Clear();
            }
        }
    }
}
