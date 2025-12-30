using CompareHWP.Common;
using DevExpress.Mvvm;
using JVM.ViewCommon.WPF.View.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static DevExpress.XtraEditors.XtraInputBox;

namespace CompareHWP.CommonViewModel
{
    public class IOSMessageBoxViewModel : ViewModelBase
    {
        private MainWindow main = Application.Current.MainWindow as MainWindow;

        public string Title
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Message
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string IconText
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string IconColor
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int RemainSeconds
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        private Visibility _cancelVisibility;
        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set => SetProperty(ref _cancelVisibility, value, nameof(CancelVisibility));
        }

        private Visibility _okVisibility;
        public Visibility OkVisibility
        {
            get => _okVisibility;
            set => SetProperty(ref _okVisibility, value, nameof(OkVisibility));
        }

        public string CountdownText =>
            RemainSeconds > 0 ? $"{RemainSeconds}초 후 자동 종료" : string.Empty;

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CopyCommand { get; }

        private DispatcherTimer _timer;
        private readonly Action<bool> _closeAction;

        public IOSMessageBoxViewModel(
            string title,
            string message,
            MessageBoxButton buttons,
            IOSMessageBoxIcon icon,
            int autoCloseSeconds,
            Action<bool> closeAction)
        {
            Title = title;
            Message = message;
            _closeAction = closeAction;

            switch (icon)
            {
                case IOSMessageBoxIcon.Warning:
                    IconText = "⚠️";
                    IconColor = "#FF9500"; // iOS Orange
                    break;

                case IOSMessageBoxIcon.Success:
                    IconText = "✅";
                    IconColor = "#34C759"; // iOS Green
                    break;

                case IOSMessageBoxIcon.Info:
                    IconText = "ℹ️";
                    IconColor = "#007AFF"; // iOS Blue
                    break;

                case IOSMessageBoxIcon.Question:
                    IconText = "❓";
                    IconColor = "#007AFF"; // iOS Blue (질문은 보통 블루 계열)
                    break;

                default:
                    IconText = string.Empty;
                    IconColor = "Transparent";
                    break;
            }


            OkCommand = new DelegateCommand(() => Close(true));
            CancelCommand = new DelegateCommand(() => Close(false));
            CopyCommand = new DelegateCommand(Copy);

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    OkVisibility = Visibility.Visible;
                    CancelVisibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    OkVisibility = Visibility.Visible;
                    CancelVisibility = Visibility.Visible;
                    break;
                default:
                    main.ShowAlertControl($"지원하지 않는 버튼 형식입니다.\nbuttons : {buttons}", "미지원", eDialogButtonType.Ok, null, 5000, null, true);
                    break;
            }

            if (autoCloseSeconds > 0)
                StartTimer(autoCloseSeconds);
        }

        private void StartTimer(int seconds)
        {
            RemainSeconds = seconds;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_, __) =>
            {
                RemainSeconds--;

                RaisePropertyChanged(nameof(CountdownText));

                if (RemainSeconds <= 0)
                    Close(true);
            };

            _timer.Start();
        }

        private void Close(bool result)
        {
            _timer?.Stop();
            _closeAction?.Invoke(result);
        }

        private void Copy()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                Clipboard.SetText(Message);
                main.ShowAlertControl($"메시지가 클립보드에 저장되었습니다.", "메시지 복사", eDialogButtonType.Ok, null, 5000, null, true);
            }
        }
    }
}
