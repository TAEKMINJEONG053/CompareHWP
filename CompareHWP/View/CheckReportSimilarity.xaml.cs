using CompareHWP.Common;
using CompareHWP.CommonView;
using CompareHWP.Helper;
using CompareHWP.ViewModel;
using DevExpress.Xpf.Core.FilteringUI;
using DevExpress.Xpf.Editors;
using DevExpress.XtraReports.Parameters;
using JVM.ViewCommon.WPF.View.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompareHWP.View
{
    /// <summary>
    /// CheckReportSimilarity.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CheckReportSimilarity : UserControl
    {
        private ViewModel.CheckReportSimilarityVM vm = new ViewModel.CheckReportSimilarityVM();

        /// <summary>
        /// 한글 파일만 허용
        /// </summary>
        private readonly string[] allowedExtensions = { ".hwp", ".hwpx" };

        public CheckReportSimilarity()
        {
            InitializeComponent();

            DataContext = vm;
        }

        private void FileGrid_DragEnter(object sender, DragEventArgs e)
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

        private void FileGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                vm._busyService.IsBusy = true;

                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    vm._busyService.IsBusy = false;
                    return;
                }

                var droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                var hwpFiles = new List<string>();
                var notHwpFiles = new List<string>();
                var duplicateFiles = new List<string>();

                foreach (var path in droppedPaths)
                {
                    // 1️⃣ 파일인 경우
                    if (File.Exists(path))
                    {
                        if (System.IO.Path.GetExtension(path).Equals(".hwp", StringComparison.OrdinalIgnoreCase))
                            hwpFiles.Add(path);
                        else
                            notHwpFiles.Add(path);
                    }
                    // 2️⃣ 폴더인 경우
                    else if (Directory.Exists(path))
                    {
                        var filesInFolder = Directory.GetFiles(
                            path,
                            "*.hwp",
                            SearchOption.AllDirectories // 하위 폴더 포함
                        );

                        hwpFiles.AddRange(filesInFolder);
                    }
                }

                // 3️⃣ 중복 제거 (이미 추가된 파일 제외)
                foreach (var file in hwpFiles.Distinct())
                {
                    if (vm.FileList.Any(f => f.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase)))
                    {
                        duplicateFiles.Add(file);
                        continue;
                    }

                    // 파일 크기 계산
                    var fileInfo = new System.IO.FileInfo(file);
                    long sizeBytes = fileInfo.Length;
                    double sizeKb = sizeBytes / 1024.0;

                    vm.FileList.Add(new CheckReportSimilarityVM.FileItem()
                    {
                        FilePath = file,
                        SizeBytes = sizeBytes,
                        AddedTime = DateTime.Now
                    });
                }

                if (notHwpFiles.Count > 0 || duplicateFiles.Count > 0)
                {
                    var message = new List<string>();

                    if (duplicateFiles.Count > 0)
                        message.Add($"다음 파일들은 이미 목록에 추가되어 있습니다.\n{string.Join("\n", duplicateFiles)}");

                    if (notHwpFiles.Count > 0)
                        message.Add($"다음 파일들은 허용되지 않는 형식입니다.\n{string.Join("\n", notHwpFiles)}");

                    IOSMessageBox.Show(string.Join("\n\n", message), "파일 추가 오류", MessageBoxButton.OK, Common.IOSMessageBoxIcon.Warning);

                    //var main = System.Windows.Application.Current.MainWindow as MainWindow;
                    //main.ShowAlertControl(string.Join("\n\n", message), "파일 업로드 에러", eDialogButtonType.Ok, null, 5000, null, true);
                }

                //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                //var addFiles = vm.FileList;

                //var alreadyAddedFiles = new List<string>();
                //var invalidFiles = new List<string>();

                //foreach (var file in files)
                //{
                //    var extension = System.IO.Path.GetExtension(file).ToLowerInvariant();

                //    if (!allowedExtensions.Contains(extension))
                //    {
                //        invalidFiles.Add(file);
                //        continue;
                //    }

                //    // 파일 크기 계산
                //    var fileInfo = new System.IO.FileInfo(file);
                //    long sizeBytes = fileInfo.Length;
                //    double sizeKb = sizeBytes / 1024.0;

                //    if (addFiles.Any(p => p.FilePath == file) == false)
                //    {
                //        vm.FileList.Add(new ViewModel.FileItem()
                //        {
                //            FilePath = file,
                //            SizeBytes = sizeBytes,
                //            AddedTime = DateTime.Now
                //        });
                //    }
                //    else
                //    {
                //        alreadyAddedFiles.Add(file);
                //    }
                //}

                //if (alreadyAddedFiles.Count > 0 || invalidFiles.Count > 0)
                //{
                //    var message = new List<string>();

                //    if (alreadyAddedFiles.Count > 0)
                //        message.Add($"다음 파일들은 이미 목록에 추가되어 있습니다.\n{string.Join("\n", alreadyAddedFiles)}");

                //    if (invalidFiles.Count > 0)
                //        message.Add($"다음 파일들은 허용되지 않는 형식입니다.\n{string.Join("\n", invalidFiles)}");

                //    IOSMessageBox.Show(string.Join("\n\n", message), "파일 추가 오류", MessageBoxButton.OK, Common.IOSMessageBoxIcon.Warning);

                //    var main = System.Windows.Application.Current.MainWindow as MainWindow;
                //    main.ShowAlertControl(string.Join("\n\n", message), "파일 업로드 에러", eDialogButtonType.Ok, null, 5000, null, true);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                vm._busyService.IsBusy = false;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;

                vm.RemoveFileClick(null);
            }
        }

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ComboBoxEdit_Format.SelectedItem as ComboBoxEditItem;

            switch (selectedItem.Tag.ToString())
            {
                case "excel":
                    ExportHelper.ExportData(GridControl_SimilarityResults, ExportType.Excel);
                    break;
                case "pdf":
                    ExportHelper.ExportData(GridControl_SimilarityResults, ExportType.Pdf);
                    break;
                case "csv":
                    ExportHelper.ExportData(GridControl_SimilarityResults, ExportType.Csv);
                    break;
                case "html":
                    ExportHelper.ExportData(GridControl_SimilarityResults, ExportType.Html);
                    break;
                case "image":
                    ExportHelper.ExportData(GridControl_SimilarityResults, ExportType.Image);
                    break;
                default:
                    //vm.main.ShowAlertControl($"parameter : {selectedItem.Tag}", "정의되지 않음", eDialogButtonType.Ok, null, 5000, null, true);
                    break;
            }
        }
    }
}
