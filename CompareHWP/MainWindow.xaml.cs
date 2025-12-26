using CompareHWP.Common;
using CompareHWP.CommonView;
using CompareHWP.Helper;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using JVM.ViewCommon.WPF.View.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CompareHWP
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM vm = new MainWindowVM();

        public class AlertControlEx : DevExpress.XtraBars.Alerter.AlertControl
        {
            public eDialogResultType LastResult { get; set; }
        }

        public class AlertFormEx
        {
            public eDialogButtonType Button { get; set; }
            public Action Action { get; set; }
            public eScreenDiv? ScreenDiv { get; set; }
        }

        public AlertControlEx alertControl { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
            InitializeAlertControl();
        }

        private void InitializeAlertControl()
        {
            alertControl = new AlertControlEx();
            alertControl.AppearanceCaption.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Bold);
            alertControl.AppearanceCaption.Options.UseFont = true;
            alertControl.AppearanceHotTrackedText.Font = new System.Drawing.Font("Arial Unicode MS", 11F);
            alertControl.AppearanceHotTrackedText.Options.UseFont = true;
            alertControl.AppearanceText.Font = new System.Drawing.Font("Arial Unicode MS", 11F);
            alertControl.AppearanceText.Options.UseFont = true;
            alertControl.AllowHtmlText = false;
            alertControl.AutoFormDelay = 5000;
            alertControl.FormDisplaySpeed = DevExpress.XtraBars.Alerter.AlertFormDisplaySpeed.Slow;
            alertControl.FormShowingEffect = DevExpress.XtraBars.Alerter.AlertFormShowingEffect.SlideVertical;

            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("DevExpress Style", "Office White");

            alertControl.AlertClick += (sender, e) =>
            {
                var alertForm = e.AlertForm;
                var alertFormEx = alertForm.Tag as AlertFormEx;
                if (alertFormEx != null)
                {
                    this.Topmost = true;
                    alertControl.LastResult = JVM.ViewCommon.WPF.View.Common.CommonDialog.ShowDialog(this, e.AlertForm.Info.Text, eDialogImageType.Information, alertFormEx.Button);
                    if (alertFormEx.Action != null)
                    {
                        alertFormEx.Action.Invoke();
                    }
                    this.Topmost = false;
                }
            };

            //  AutoDisplay 최대치로 설정 시 Pin 걸어서 자동으로 닫히지 않게 설정
            alertControl.FormLoad += (sender, e) =>
            {
                if (alertControl.AutoFormDelay == int.MaxValue)
                    e.Buttons.PinButton.SetDown(true);
            };
        }

        public DevExpress.XtraBars.Alerter.AlertForm ShowAlertControl(string text, string caption = null, eDialogButtonType button = eDialogButtonType.Ok, Action action = null, int autoFormDelay = 5000, eScreenDiv? screenDiv = null, bool avoidDuplication = false)
        {
            DevExpress.XtraBars.Alerter.AlertForm alertForm = null;

            try
            {
                DevExpress.XtraBars.Alerter.AlertForm existAlertForm = null;

                alertControl.AutoFormDelay = autoFormDelay;
                //alertControl.FormLocation = (DevExpress.XtraBars.Alerter.AlertFormLocation)Properties.Settings.Default.AlertControlPosition;

                var st = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
                var sb = new StringBuilder();
                sb.AppendLine("AlertControl 생성");
                sb.AppendLine($"FileName : {st.DeclaringType?.Name}, MethodName : {st.Name}");
                sb.AppendLine($"Caption : {caption}, text : {text}, Delay : {autoFormDelay}");
                //Log.Info(sb.ToString());

                if (avoidDuplication
                    && (existAlertForm = alertControl.AlertFormList.FirstOrDefault(p => p.Tag != null && (p.Tag as AlertFormEx).ScreenDiv == screenDiv)) != null)  //LabelPrint 메시지이면 이미 떠있는 AlertControl을 찾음
                {
                    existAlertForm.AlertInfo.Caption = caption;
                    existAlertForm.AlertInfo.Text = text;
                    existAlertForm.AutoFormDelay = autoFormDelay;
                    existAlertForm.Refresh();

                    alertForm = existAlertForm;
                }
                else
                {
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("The Bezier", "Office White");

                    alertControl.Show(null, caption, text);
                    var lastAlertForm = alertControl.AlertFormList.LastOrDefault();
                    if (lastAlertForm != null)
                    {
                        lastAlertForm.Tag = new AlertFormEx()
                        {
                            Button = button,
                            Action = action,
                            ScreenDiv = screenDiv
                        };

                        alertForm = existAlertForm;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Info2("Exception", MethodBase.GetCurrentMethod().Name, ex.ToString(), LocalProperty.ExceptionLogFileName, true, false);
            }

            return alertForm;
        }

        private void Button_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            //var fileNames = HWPHelper.SelectHwpFiles();

            //if (fileNames.Count >= 2)
            //{
            //    var docs = HWPHelper.LoadDocuments(fileNames, TextBox_Marker.Text);

            //    var docsPairs = HWPHelper.BuildAllPairs(docs);

            //    foreach (var docsPair in docsPairs)
            //    {
            //        string similarity = TextSimilarity.CosineSimilarity(docsPair.DocA.Text, docsPair.DocB.Text);
            //        TextBox_Log.Text += $"[{similarity}] {docsPair.DocA.FileName}, {docsPair.DocB.FileName}\n\n";

            //        TextBox_Log.Text += $"{docsPair.DocA.FileName}\n{docsPair.DocA.Text}\n\n";
            //        TextBox_Log.Text += $"{docsPair.DocB.FileName}\n{docsPair.DocB.Text}";
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("두 개 이상의 HWP 파일을 선택해주세요.", "파일 선택 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private void Button_ExtractText_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var addFiles = vm.FileList;

            var alreadyAddedFiles = new List<string>();

            foreach (var file in files)
            {
                if (addFiles.Any(p => p == file) == false)
                {
                    vm.FileList.Add(file);
                }
                else
                {
                    alreadyAddedFiles.Add(file);
                }
            }

            if (alreadyAddedFiles.Count > 0)
            {
                string message = "다음 파일들은 이미 목록에 추가되어 있습니다.\n" +
                    string.Join("\n", alreadyAddedFiles);
                MessageBox.Show(message, "파일 중복", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FileListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void ListBoxEdit_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            var edit = sender as DevExpress.Xpf.Editors.ListBoxEdit;
            if (edit == null)
                return;

            vm.UpdateSelectedItems(edit.SelectedItems.OfType<string>().ToList());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!IsActive)
                return;

            // Alt + L (중요: SystemKey 사용)
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt
                && e.SystemKey == Key.L)
            {
                e.Handled = true;

                TxtSearchTool txtSearchTool = new TxtSearchTool(@"D:\1.IntiPharmLogFolder\202512\20251224");
                txtSearchTool.ShowDialog();
            }
        }
    }
}
