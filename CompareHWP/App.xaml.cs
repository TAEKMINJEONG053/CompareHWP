using CompareHWP.Common;
using CompareHWP.CommonView;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CompareHWP
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // UI 스레드 예외
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 백그라운드(Task) 예외
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task 예외 (async/await)
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            try
            {
                Log.LogPath = @"D:\1. SejongHelperLog";
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Info2("Exception", MethodBase.GetCurrentMethod().Name, ex.ToString(), "Exception_Log", true, false);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowFatalError(e.Exception, "UI Thread");
            e.Handled = true; // 앱 강제 종료 방지 (필요 시)
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowFatalError(e.ExceptionObject as Exception, "AppDomain");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ShowFatalError(e.Exception, "Task");
            e.SetObserved();
        }

        private void ShowFatalError(Exception ex, string source)
        {
            try
            {
                Log.Info2("Exception", MethodBase.GetCurrentMethod().Name, ex.ToString(), "Exception_Log", true, false);

                IOSMessageBox.Show(
                    $"치명적인 오류가 발생했습니다. 프로그램을 재시작 해주세요.\n\n오류 발생 위치: {source}\n오류 메시지: {ex.Message}",
                    "치명적인 오류",
                    MessageBoxButton.OK,
                    Common.IOSMessageBoxIcon.Warning);
            }
            catch
            {
                // 최후의 최후 (로그조차 안 되면 아무것도 못 함)
            }
        }
    }
}
