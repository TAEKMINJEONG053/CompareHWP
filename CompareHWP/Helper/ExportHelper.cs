using CompareHWP.Common;
using DevExpress.XtraPrinting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareHWP.Helper
{
    public class ExportHelper
    {
        public static void ExportData(DevExpress.Xpf.Grid.GridControl grid, ExportType exportType)
        {
            var info = GetSaveFileDialogInfo(exportType);

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = info.filter,
                FileName = info.fileName,
            };

            if (dlg.ShowDialog() != true)
                return;

            switch (exportType)
            {
                case ExportType.Excel:
                    grid.View.ExportToXlsx(dlg.FileName);
                    return;
                case ExportType.Pdf:
                    grid.View.ExportToPdf(dlg.FileName);
                    return;
                case ExportType.Csv:
                    grid.View.ExportToCsv(dlg.FileName);
                    return;
                case ExportType.Html:
                    grid.View.ExportToHtml(dlg.FileName);
                    return;
                case ExportType.Image:
                    var options = new ImageExportOptions
                    {
                        Format = System.Drawing.Imaging.ImageFormat.Png,
                        //  고해상도 설정
                        Resolution = 300
                    };

                    if (dlg.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        options.Format = System.Drawing.Imaging.ImageFormat.Jpeg;

                    grid.View.ExportToImage(dlg.FileName, options);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportType), exportType, null);
            }
        }

        private static (string filter, string fileName) GetSaveFileDialogInfo(ExportType exportType)
        {
            string filter = string.Empty;
            string fileName = "보고서 유사성 검토 결과_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            switch (exportType)
            {
                case ExportType.Excel:
                    filter = "Excel (*.xlsx)|*.xlsx";
                    fileName += ".xlsx";
                    break;
                case ExportType.Pdf:
                    filter = "PDF (*.pdf)|*.pdf";
                    fileName += ".pdf";
                    break;
                case ExportType.Csv:
                    filter = "CSV (*.csv)|*.csv";
                    fileName += ".csv";
                    break;
                case ExportType.Html:
                    filter = "HTML (*.html)|*.html";
                    fileName += ".html";
                    break;
                case ExportType.Image:
                    filter = "Image (*.png)|*.png";
                    fileName += ".png";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportType), exportType, null);
            }
            return (filter, fileName);
        }
    }
}
