using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;

namespace EaFileDownloader02.Services
{
    public class FileReader : IFileReader
    {
        public IList<string> ReadBarcodesFromFile(bool exceptAlreadyDownloadBarcodes = true)
        {
            string fileName = System.Configuration.ConfigurationManager.AppSettings["BarcodesFile"];

            try
            {
                var barcodes = this.ReadLines(fileName);

                if (!exceptAlreadyDownloadBarcodes)
                {
                    return barcodes;
                }

                var downloadedBarcodesFileName = System.Configuration.ConfigurationManager.AppSettings["DownloadBarcodesFile"];
                    
                try
                {
                    Global.Logger.Info($"В файле  {fileName} нашел {barcodes.Count} штрих кодов. Исключаю из них уже скачанные штрих-коды. Читаю из файла {downloadedBarcodesFileName}");
                    var downloadedBarcodes = this.ReadLines(downloadedBarcodesFileName);

                    if (downloadedBarcodes.Any())
                    {
                        barcodes = barcodes.Except(downloadedBarcodes).ToList();
                    }
                }
                catch (Exception e)
                {
                    Global.Logger.Error(e.Message);
                    Global.Logger.Info($"Буду брать все записи из {fileName}" );
                    return barcodes;
                }
                
                return barcodes;
            }
            catch (Exception e)
            {
                Global.Logger.Fatal(e.Message);
                throw;
            }
        }

        public List<string> ReadLines(string fileName)
        {
            return File.ReadLines(fileName)
                .Select(x => x.Trim('"').Trim())
                .Where(x => !x.IsNullOrEmpty())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet()
                .ToList();
        }
        
        public void WriteNotDownloadedBarcodes(IEnumerable<string> barcodes)
        {
            if (!barcodes.Any())
            {
                return;
            }
            
            foreach (var barcode in barcodes)
            {
                Global.Logger.Info($"Не нашел в ЭА {barcode}.");
            }

            string path = System.Configuration.ConfigurationManager.AppSettings["NotDownloadBarcodesFile"];

            this.WriteLinesOnFile(barcodes, path);
        }
        
        public void WriteExceptionOnDownloadBarcodes(IEnumerable<string> barcodes)
        {
            if (!barcodes.Any())
            {
                return;
            }

            string path = System.Configuration.ConfigurationManager.AppSettings["ExceptionOnDownloadBarcodesFile"];

            this.WriteLinesOnFile(barcodes, path);
        }

        public void WriteLinesOnFile(IEnumerable<string> barcodes, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                    }
                }

                using (StreamWriter sw = File.AppendText(path))
                {
                    foreach (var barcode in barcodes)
                    {
                        sw.WriteLine(barcode);
                    }
                }
                
                Global.Logger.Info($"Записал в файл {path} {barcodes.Count()} значений.");
            }
            catch (Exception e)
            {
                Global.Logger.Error(e.Message);
                throw;
            }
        }
        
        public void WriteDownloadedBarcode(string barcode)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["DownloadBarcodesFile"];
        
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                }
            }
        
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(barcode);
            }
        }

        public void WriteFoundedBarcodesInEa(IEnumerable<string> barcodes)
        {
            try
            {
                Global.Logger.Info($"Записываю значения найденных штрих кодов");
            
                string path = System.Configuration.ConfigurationManager.AppSettings["FoundedBarcodesInEa"];
            
                this.WriteLinesOnFile(barcodes, path);
            }
            catch (Exception e)
            {
                Global.Logger.Error(e.Message);
                throw;
            }
            
        }
    }
}
