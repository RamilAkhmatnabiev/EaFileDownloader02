using System.Collections.Generic;

namespace EaFileDownloader02
{
    /// <summary>
    /// Сервис получения списка штрих-кодов
    /// </summary>
    public interface IFileReader
    {
        IList<string> ReadBarcodesFromFile(bool exceptAlreadyDownloadBarcodes = true);
        List<string> ReadLines(string fileName);
        void WriteNotDownloadedBarcodes(IEnumerable<string> barcodes);
        void WriteExceptionOnDownloadBarcodes(IEnumerable<string> barcodes);
        void WriteLinesOnFile(IEnumerable<string> barcodes, string path);
        void WriteDownloadedBarcode(string barcode);
        void WriteFoundedBarcodesInEa(IEnumerable<string> barcodes);
    }
}