using CompareHWP.Common;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace CompareHWP.CommonViewModel
{
    public class IOSMessageBoxViewModel : ViewModelBase
    {
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

        public string CountdownText =>
            RemainSeconds > 0 ? $"{RemainSeconds}초 후 자동 종료" : string.Empty;

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private DispatcherTimer _timer;
        private readonly Action<bool> _closeAction;

        public IOSMessageBoxViewModel(
            string title,
            string message,
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
    }
}
