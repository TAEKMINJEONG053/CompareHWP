using CompareHWP.Common;
using CompareHWP.ViewModel;
using DevExpress.Entity.Model.Metadata;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static CompareHWP.ViewModel.CheckReportSimilarityVM;

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
            catch (Exception ex)
            {
                Log.Info2("Exception", MethodBase.GetCurrentMethod().Name, ex.ToString(), "Exception_Log", true, false);
                return string.Empty;
            }
            finally
            {
                hwp.Quit();
                Marshal.ReleaseComObject(hwp);
            }
        }

        /// <summary>
        /// TODO: 동일하게 HWP 숫자만큼 팝업창 뜨는게 동일함
        /// </summary>
        /// <param name="hwpPaths"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public static List<DocumentText> ReadAllTextFromHwps(List<string> hwpPaths, string marker)
        {
            Type hwpType = Type.GetTypeFromProgID("HWPFrame.HwpObject");
            dynamic hwp = Activator.CreateInstance(hwpType);
            var docs = new List<DocumentText>();

            try
            {
                hwp.RegisterModule("FilePathCheckDLL", "SecurityModule");

                foreach (var path in hwpPaths)
                {
                    hwp.Open(path, "", "forceopen:true;readonly:true");

                    docs.Add(new DocumentText
                    {
                        FilePath = path,
                        FileName = Path.GetFileName(path),
                        Text = ExtractTextAfter(hwp.GetTextFile("TEXT", ""), marker),
                    });

                    hwp.Clear(3); // 문서 닫기
                }

                return docs;
            }
            catch (Exception ex)
            {
                Log.Info2("Exception", MethodBase.GetCurrentMethod().Name, ex.ToString(), "Exception_Log", true, false);
                return new List<DocumentText>();
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

            //docs = ReadAllTextFromHwps(filePaths, marker);

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
