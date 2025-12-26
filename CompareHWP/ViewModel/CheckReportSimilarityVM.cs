using CompareHWP.Common;
using CompareHWP.CommonView;
using CompareHWP.Helper;
using CompareHWP.Services;
using DevExpress.Mvvm;
using JVM.ViewCommon.WPF.View.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace CompareHWP.ViewModel
{
    public class CheckReportSimilarityVM : ViewModelBase
    {
        public readonly BusyService _busyService = BusyService.Instance;
        private MainWindow main = System.Windows.Application.Current.MainWindow as MainWindow;

        private ObservableCollection<FileItem> _fileList;
        public ObservableCollection<FileItem> FileList
        {
            get => _fileList;
            set => SetProperty(ref _fileList, value, nameof(FileList));
        }

        /// <summary>
        /// GridControl 의 SelectedItems 에 바인딩하는 경우 미리 생성해줘야함
        /// </summary>
        private ObservableCollection<FileItem> _selectedFileList = new ObservableCollection<FileItem>();
        public ObservableCollection<FileItem> SelectedFileList
        {
            get => _selectedFileList;
            set => SetProperty(ref _selectedFileList, value, nameof(SelectedFileList));
        }

        private ObservableCollection<SimilarityResult> _similarityResults;
        public ObservableCollection<SimilarityResult> SimilarityResults
        {
            get => _similarityResults;
            set => SetProperty(ref _similarityResults, value, nameof(SimilarityResults));
        }

        public ICommand AnalyzeCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand RemoveFileCommand { get; }

        public CheckReportSimilarityVM()
        {
            FileList = new ObservableCollection<FileItem>();
            SimilarityResults = new ObservableCollection<SimilarityResult>();

            RemoveFileCommand = new RelayCommand(RemoveFileClick);
            AnalyzeCommand = new RelayCommand(AnalyzeCommandClick);
            ClearCommand = new RelayCommand(ClearCommandClick);
        }

        private void RemoveFileClick(object parameter)
        {
            if (SelectedFileList == null || SelectedFileList.Count == 0)
            {
                main.ShowAlertControl("삭제할 파일을 선택해주세요.", "파일 선택 오류", eDialogButtonType.Ok, null, 5000, null, true);
                return;
            }

            var deleteFiles = string.Join("\n", SelectedFileList.Select(f => f.FileName));

            var messageBoxResult = IOSMessageBox.Show(
                $"다음 파일들을 목록에서 삭제하시겠습니까?\n\n{deleteFiles}",
                "파일 삭제 확인",
                IOSMessageBoxIcon.Question);

            var title = messageBoxResult == true ? "파일 삭제 완료" : "파일 삭제 취소";
            var message = messageBoxResult == true ? $"다음 파일들이 목록에서 삭제되었습니다.\n\n{deleteFiles}" : "파일 삭제가 취소되었습니다.";

            if (messageBoxResult == true)
                foreach (var file in SelectedFileList.ToList())
                    FileList.Remove(file);

            main.ShowAlertControl(message, title, eDialogButtonType.Ok, null, 5000, null, true);

            //var messageBoxResult = System.Windows.MessageBox.Show($"다음 파일들을 목록에서 삭제하시겠습니까?\n\n{deleteFiles}", "파일 삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //var title = messageBoxResult == MessageBoxResult.Yes ? "파일 삭제 완료" : "파일 삭제 취소";
            //var message = messageBoxResult == MessageBoxResult.Yes ? $"다음 파일들이 목록에서 삭제되었습니다.\n\n{deleteFiles}" : "파일 삭제가 취소되었습니다.";

            //if (messageBoxResult == MessageBoxResult.Yes)
            //    foreach (var file in SelectedFileList.ToList())
            //        FileList.Remove(file);

            //main.ShowAlertControl(message, title, eDialogButtonType.Ok, null, 5000, null, true);
        }

        private void AnalyzeCommandClick(object parameter)
        {
            if (FileList.Count == 0)
                main.ShowAlertControl("비교 가능한 문서가 없습니다.\n문서를 추가해주십시오.", "비교 불가", eDialogButtonType.Ok, null, 5000, null, true);
            else if (FileList.Count == 1)
                main.ShowAlertControl("문서가 1개 밖에 없습니다.\n문서를 추가해주십시오.", "비교 불가", eDialogButtonType.Ok, null, 5000, null, true);
            else
            {
                _busyService.IsBusy = true;
                // 1️. HWP → DocumentText (marker 사용)
                string marker = "멘토링 수행 내용"; // 필요 시 UI에서 받기
                var documents = HWPHelper.LoadDocuments(FileList.Select(p => p.FilePath).ToList(), marker);

                // 2️. 모든 문서 쌍 생성
                var pairs = HWPHelper.BuildAllPairs(documents);

                var results = new List<SimilarityResult>();

                // 3️. 유사도 계산
                foreach (var pair in pairs)
                {
                    string textA = TextPreprocessor.Preprocess(pair.DocA.Text);
                    string textB = TextPreprocessor.Preprocess(pair.DocB.Text);

                    double similarity = TextSimilarity.CosineSimilarityPercent(textA, textB);

                    results.Add(new SimilarityResult
                    {
                        SourceFile = pair.DocA.FileName,
                        TargetFile = pair.DocB.FileName,
                        Similarity = similarity,
                        CheckedTime = DateTime.Now
                    });
                }

                SimilarityResults = new ObservableCollection<SimilarityResult>(results.OrderByDescending(p => p.Similarity));
                _busyService.IsBusy = false;
            }
        }

        private void ClearCommandClick(object parameter)
        {
            if (FileList == null || FileList.Count == 0)
            {
                main.ShowAlertControl("리스트를 초기화 할 수 없습니다.", "리스트 초기화 실패", eDialogButtonType.Ok, null, 5000, null, true);
            }
            else
            {
                FileList.Clear();
                main.ShowAlertControl("리스트를 초기화하였습니다.", "리스트 초기화 완료", eDialogButtonType.Ok, null, 5000, null, true);
            }
        }
    }

    public class FileItem
    {
        /// <summary>전체 파일 경로</summary>
        public string FilePath { get; set; }

        /// <summary>파일명만</summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>파일 크기 (Byte)</summary>
        public long SizeBytes { get; set; }

        /// <summary>KB 단위</summary>
        public double SizeKb => SizeBytes / 1024.0;

        /// <summary>추가 시각</summary>
        public DateTime AddedTime { get; set; }

        /// <summary>파일 확장자</summary>
        public string Extension => Path.GetExtension(FilePath);

        /// <summary>UI 표시용 (KB / MB 자동)</summary>
        public string SizeText
        {
            get
            {
                if (SizeBytes < 1024 * 1024)
                    return $"{SizeBytes / 1024.0:F1} KB";
                return $"{SizeBytes / 1024.0 / 1024.0:F2} MB";
            }
        }
    }

    public class SimilarityResult
    {
        /// <summary>기준 파일</summary>
        public string SourceFile { get; set; }

        /// <summary>비교 대상 파일</summary>
        public string TargetFile { get; set; }

        /// <summary>유사도 (0~100)</summary>
        public double Similarity { get; set; }

        /// <summary>의심 여부 판정</summary>
        public string Result
        {
            get
            {
                if (Similarity >= 80)
                    return "⚠ 매우 높음";
                if (Similarity >= 60)
                    return "주의";
                return "정상";
            }
        }

        /// <summary>UI 표시용 퍼센트</summary>
        public string SimilarityText => $"{Similarity:F1}%";

        /// <summary>검사 시간</summary>
        public DateTime CheckedTime { get; set; }
    }

    public class DocumentText
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Text { get; set; }
    }

    public class DocumentPair
    {
        public DocumentText DocA { get; set; }
        public DocumentText DocB { get; set; }
    }
}
