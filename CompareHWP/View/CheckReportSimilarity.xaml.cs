using JVM.ViewCommon.WPF.View.Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var addFiles = vm.FileList;

                var alreadyAddedFiles = new List<string>();
                var invalidFiles = new List<string>();

                foreach (var file in files)
                {
                    var extension = System.IO.Path.GetExtension(file).ToLowerInvariant();

                    //if (!allowedExtensions.Contains(extension))
                    //{
                    //    invalidFiles.Add(file);
                    //    continue;
                    //}

                    // 파일 크기 계산
                    var fileInfo = new System.IO.FileInfo(file);
                    long sizeBytes = fileInfo.Length;
                    double sizeKb = sizeBytes / 1024.0;

                    if (addFiles.Any(p => p.FilePath == file) == false)
                    {
                        vm.FileList.Add(new ViewModel.FileItem()
                        {
                            FilePath = file,
                            SizeBytes = sizeBytes,
                            AddedTime = DateTime.Now
                        });
                    }
                    else
                    {
                        alreadyAddedFiles.Add(file);
                    }
                }

                if (alreadyAddedFiles.Count > 0 || invalidFiles.Count > 0)
                {
                    var message = new List<string>();

                    if (alreadyAddedFiles.Count > 0)
                        message.Add($"다음 파일들은 이미 목록에 추가되어 있습니다.\n{string.Join("\n", alreadyAddedFiles)}");

                    if (invalidFiles.Count > 0)
                        message.Add($"다음 파일들은 허용되지 않는 형식입니다.\n{string.Join("\n", invalidFiles)}");

                    var main = System.Windows.Application.Current.MainWindow as MainWindow;
                    main.ShowAlertControl(string.Join("\n\n", message), "파일 업로드 에러", eDialogButtonType.Ok, null, 5000, null, true);
                }
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
    }
}
