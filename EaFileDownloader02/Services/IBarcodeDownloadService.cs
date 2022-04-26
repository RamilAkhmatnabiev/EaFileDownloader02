using System.Collections.Generic;

namespace EaFileDownloader02
{
    public interface IBarcodeDownloadService
    {
        void DownloadFilesToStorage(IList<string> barcodes);

        void FindBarcodesOnRemoteStorage(IList<string> barcodes);
    }
}