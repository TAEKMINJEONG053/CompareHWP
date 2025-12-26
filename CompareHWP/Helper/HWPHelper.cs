using CompareHWP.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CompareHWP
{
    public class HWPHelper
    {
        public static List<string> SelectHwpFiles()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "HWP Files (*.hwp)|*.hwp",
                Multiselect = true,
                Title = "HWP 파일 다중 선택"
            };

            return dialog.ShowDialog() == true
                ? dialog.FileNames.ToList()
                : new List<string>();
        }

        public static string ReadAllTextFromHwp(string hwpPath)
        {
            Type hwpType = Type.GetTypeFromProgID("HWPFrame.HwpObject");
            dynamic hwp = Activator.CreateInstance(hwpType);

            try
            {
                hwp.RegisterModule("FilePathCheckDLL", "SecurityModule");
                hwp.Open(hwpPath, "", "forceopen:true;readonly:true");

                return hwp.GetTextFile("TEXT", "");
            }
            finally
            {
                hwp.Quit();
                Marshal.ReleaseComObject(hwp);
            }
        }

        public static List<DocumentText> LoadDocuments(List<string> filePaths, string marker)
        {
            var docs = new List<DocumentText>();

            foreach (var path in filePaths)
            {
                docs.Add(new DocumentText
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    Text = ExtractTextAfter(ReadAllTextFromHwp(path), marker),
                });
            }

            return docs;
        }

        public static string ExtractTextAfter(string fullText, string marker)
        {
            if (string.IsNullOrWhiteSpace(fullText))
                return string.Empty;

            int index = fullText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return fullText; // 기준 문자열 없으면 전체 반환 (또는 빈값)

            return fullText.Substring(index + marker.Length);
        }

        public static List<DocumentPair> BuildAllPairs(List<DocumentText> docs)
        {
            var pairs = new List<DocumentPair>();

            for (int i = 0; i < docs.Count; i++)
            {
                for (int j = i + 1; j < docs.Count; j++)
                {
                    pairs.Add(new DocumentPair
                    {
                        DocA = docs[i],
                        DocB = docs[j]
                    });
                }
            }

            return pairs;
        }
    }
}
