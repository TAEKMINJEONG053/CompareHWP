using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CompareHWP.CommonView
{
    /// <summary>
    /// TxtSearchTool.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TxtSearchTool : Window
    {
        private string _currentFolder;
        private string _currentOpenedFile;
        private bool _isHighlighted;

        private ObservableCollection<SearchResult> _results =
            new ObservableCollection<SearchResult>();

        public TxtSearchTool(string initialFolderPath)
        {
            InitializeComponent();

            GridResult.ItemsSource = _results;
        }

        // ===== 모델 =====
        public class SearchResult
        {
            public string FullPath { get; set; }
            public int LineNumber { get; set; }
            public string LineText { get; set; }
        }

        public class FileSystemItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsDirectory { get; set; }
            public ObservableCollection<FileSystemItem> Children { get; set; }
                = new ObservableCollection<FileSystemItem>();
        }

        // ================= 폴더 선택 =================

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                _currentFolder = dlg.SelectedPath;
                TxtCurrentFolder.Text = _currentFolder;

                LoadFolderTree(_currentFolder);

                TxtPreview.Document = new FlowDocument();
                _results.Clear();

                _currentOpenedFile = null;
                _isHighlighted = false;
            }
        }

        // ================= 트리 =================

        private void LoadFolderTree(string path)
        {
            FolderTree.Items.Clear();
            FolderTree.Items.Add(CreateTreeItem(CreateNode(path)));
        }

        private FileSystemItem CreateNode(string path)
        {
            var node = new FileSystemItem
            {
                Name = System.IO.Path.GetFileName(path),
                FullPath = path,
                IsDirectory = true
            };

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                    node.Children.Add(CreateNode(dir));

                foreach (var file in Directory.GetFiles(path, "*.txt"))
                    node.Children.Add(new FileSystemItem
                    {
                        Name = System.IO.Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
            }
            catch { }

            return node;
        }

        private TreeViewItem CreateTreeItem(FileSystemItem item)
        {
            var tvi = new TreeViewItem { Header = item.Name, Tag = item };
            foreach (var c in item.Children)
                tvi.Items.Add(CreateTreeItem(c));
            return tvi;
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tvi = FolderTree.SelectedItem as TreeViewItem;
            var item = tvi?.Tag as FileSystemItem;

            if (item == null || item.IsDirectory)
                return;

            EnsureFileLoaded(item.FullPath);
        }

        // ================= 검색 =================

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            _results.Clear();

            if (string.IsNullOrWhiteSpace(_currentFolder))
                return;

            string keyword = TxtSearch.Text;
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            foreach (var file in Directory.GetFiles(_currentFolder, "*.txt", SearchOption.AllDirectories))
            {
                int lineNo = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNo++;
                    if (line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _results.Add(new SearchResult
                        {
                            FullPath = file,
                            LineNumber = lineNo,
                            LineText = line
                        });
                    }
                }
            }
        }

        // ================= 검색 결과 더블클릭 =================

        private void Result_RowDoubleClick(object sender, DevExpress.Xpf.Grid.RowDoubleClickEventArgs e)
        {
            var row = GridResult.CurrentItem as SearchResult;
            if (row == null)
                return;

            EnsureFileLoaded(row.FullPath);

            Dispatcher.InvokeAsync(() =>
            {
                EnsureHighlight(TxtSearch.Text);
                MoveCaretToLine(row.LineNumber);
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        // ================= 파일 로드 =================

        private void EnsureFileLoaded(string path)
        {
            if (_currentOpenedFile == path)
                return;

            TxtPreview.Document = new FlowDocument();

            string text = File.ReadAllText(path);

            // ❗ 반드시 Paragraph 1개 + Run 1개
            TxtPreview.Document.Blocks.Add(
                new Paragraph(new Run(text))
            );

            TxtPreview.CaretPosition = TxtPreview.Document.ContentStart;

            _currentOpenedFile = path;
            _isHighlighted = false;
        }

        // ================= 하이라이트 =================

        private void EnsureHighlight(string keyword)
        {
            if (_isHighlighted || string.IsNullOrWhiteSpace(keyword))
                return;

            TextPointer pointer = TxtPreview.Document.ContentStart;

            while (pointer != null &&
                   pointer.CompareTo(TxtPreview.Document.ContentEnd) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward)
                    == TextPointerContext.Text)
                {
                    string runText = pointer.GetTextInRun(LogicalDirection.Forward);
                    int index = 0;

                    while ((index = runText.IndexOf(
                                keyword,
                                index,
                                StringComparison.OrdinalIgnoreCase)) >= 0)
                    {
                        TextPointer start = pointer.GetPositionAtOffset(index);
                        TextPointer end = start.GetPositionAtOffset(keyword.Length);

                        new TextRange(start, end)
                            .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);

                        index += keyword.Length;
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }

            _isHighlighted = true;
        }

        // ================= 줄 이동 =================

        private void MoveCaretToLine(int lineNumber)
        {
            TextPointer pointer = TxtPreview.Document.ContentStart;
            int currentLine = 1;

            while (pointer != null)
            {
                if (currentLine == lineNumber)
                {
                    TxtPreview.CaretPosition = pointer;
                    TxtPreview.Focus();
                    return;
                }

                TextPointer next = pointer.GetLineStartPosition(1);
                if (next == null)
                    break;

                pointer = next;
                currentLine++;
            }
        }
    }
}
