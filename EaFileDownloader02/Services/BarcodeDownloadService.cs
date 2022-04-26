using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Castle.Core.Internal;
using Newtonsoft.Json.Linq;

namespace EaFileDownloader02.Services
{
    class BarcodeDownloadService : EaConnectionBase, IBarcodeDownloadService
    {
        /// <summary>
        /// Получить Uri поиска
        /// </summary>
        protected override string GetSearchUri()
        {
            return Utils.CombinePath(base.baseUri, "/_api/web/lists/GetByTitle('скан-образы')/items?$select=FileLeafRef,FileRef&$filter=FileLeafRef eq");
        }
        
        public IFileReader FileReader { get; set; }

        public void DownloadFilesToStorage(IList<string> barcodes)
        {
            Global.Logger.Info($"Штрих-кодов на скачивание {barcodes.Count}");
            
            var exceptionsOnDownload = new HashSet<string>();

            var foundFiles = this.GetFileRoutes(barcodes);
            
            foreach (var foundFile in foundFiles)
            {
                var barcode = Path.GetFileNameWithoutExtension(foundFile);
                try
                {
                    Global.Logger.Info($"Скачиваю файл из ЭА по пути: {foundFile}");
                    var data = base.GetFileBytes(foundFile);

                    var filePath = this.GetFilePath(barcode);

                    var directoryPath = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    File.WriteAllBytes(filePath, data);
                    this.FileReader.WriteDownloadedBarcode(barcode);
                    Global.Logger.Info($"Успешно скачал  {barcode}");
                }
                catch (Exception e)
                {
                    Global.Logger.Error($"Ошибка при скачивании файла {barcode}. {e.Message}");
                    exceptionsOnDownload.Add(barcode);
                }
            }

            this.FileReader.WriteExceptionOnDownloadBarcodes(exceptionsOnDownload);
        }

        public void FindBarcodesOnRemoteStorage(IList<string> barcodes)
        {
            this.GetFileRoutes(barcodes, true);
            // var foundBarcodes = this.GetFileRoutes(barcodes)
            //     .Select(Path.GetFileNameWithoutExtension)
            //     .ToHashSet();
            //
            // if (foundBarcodes.IsNullOrEmpty())
            // {
            //     return;
            // }
            //
            // string path = System.Configuration.ConfigurationManager.AppSettings["FoundedBarcodesInEa"];
            //
            // var alreadyFoundedBarcodes = new List<string>();
            //     
            // try
            // {
            //     alreadyFoundedBarcodes.AddRange(this.FileReader.ReadLines(path));
            // }
            // catch (Exception e)
            // {
            //     Global.Logger.Error(e.Message);
            // }

            //this.FileReader.WriteLinesOnFile(foundBarcodes.Except(alreadyFoundedBarcodes), path);
        }

        /// <summary>
        /// Сравниваем из файла уже полученные пути и пишем в файл не запрошенные
        /// ВСЕГДА! Найденные пути и значения записывает в файлы
        /// </summary>
        /// <param name="barcodes"></param>
        /// <param name="exceptChecked">Проверить уже скачанные среди проверенных. Если false то исключаем конкретно найденные пути. Если true то исключаем все ранее папавшие в запрос</param>
        /// <returns></returns>
        private HashSet<string> GetFileRoutes(IList<string> barcodes, bool exceptChecked = false)
        {
            Global.Logger.Info($"Штрих-кодов на проверку наличия в ЭА {barcodes.Count}");
            var foundFiles = new HashSet<string>();

            try
            {
                var founded = exceptChecked
                    ? this.FileReader.ReadLines(
                        System.Configuration.ConfigurationManager.AppSettings["CheckedToExistance"])
                    : GetAlreadyFoundetRoutes(barcodes);
                
                // Только те, которые еще не искали
                var barcodesForSearch = barcodes.Except(founded).ToList();
                Global.Logger.Info($"Среди переданных штрих-кодов не проверено наличие в ЭА {barcodesForSearch.Count}");
                
            
                string path = System.Configuration.ConfigurationManager.AppSettings["FileRoutesInEa"];
            
                //Пишем в файл все, что нашли
                for (var skip = 0; skip < barcodesForSearch.Count; skip += this.batchSize)
                {
                    var foundedBatch = this.SearchFiles(barcodesForSearch.Skip(skip).Take(this.batchSize).ToArray());
                    foundFiles.UnionWith(foundedBatch);

                    this.FileReader.WriteLinesOnFile(foundedBatch, path);
                }
                
                Global.Logger.Info($"В итоге, нашел в ЭА {foundFiles.Count}");
            
                var barcodeReader = this.container.Resolve<IFileReader>();
                try
                {
                    // Считываем из файла те, которые входят в список barcodes
                    return barcodeReader.ReadLines(path)
                        .Where(x => barcodes.Contains(Path.GetFileNameWithoutExtension(x)))
                        .ToHashSet();
                }
                finally
                {
                    this.container.Release(barcodeReader);
                }
            }
            catch (Exception e)
            {
                Global.Logger.Fatal($"Пал при получении путей, но не при запросе в ЕА. {e.Message}");
                throw;
            }
        }

        private List<string> GetAlreadyFoundetRoutes(IList<string> barcodes)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["FileRoutesInEa"];
            Global.Logger.Info($"Проверяем файл с уже проверенными путями в ЭА. {path}");

            var result = new List<string>();

            var barcodeReader = this.container.Resolve<IFileReader>();

            try
            {
                var foundedRoutes = barcodeReader.ReadLines(path);

                foreach (var route in foundedRoutes)
                {
                    var barcode = Path.GetFileNameWithoutExtension(route);

                    if (barcodes.Contains(barcode))
                    {
                        result.Add(barcode);
                    }
                }
                
                Global.Logger.Info($"Уже проверенных путей в Эа {result.Count}");
                return result;
            }
            catch(Exception e)
            {
                Global.Logger.Info(e.Message);
                return result;
            }
            finally
            {
                this.container.Release(barcodeReader);
            }
        }

        /// <inheritdoc />
        public ISet<string> CheckBarcodesUploadState(IReadOnlyCollection<string> barcodes)
        {
            var result = new HashSet<string>();
            if (barcodes.Count == 0)
            {
                return result;
            }

            try
            {
                var response = this.RequestPresentFiles(barcodes);

                try
                {
                    var jobj = JObject.Parse(response);
                    var jarr = (JArray)jobj["d"]["results"];

                    foreach (var jToken in jarr)
                    {
                        var fileRef = jToken.SelectToken("FileRef")?.ToString();
                        var fileLeafRef = jToken.SelectToken("FileLeafRef")?.ToString();

                        if (!string.IsNullOrEmpty(fileRef) && !string.IsNullOrEmpty(fileLeafRef))
                        {
                            result.Add(fileLeafRef.Substring(0, fileLeafRef.Length - this.FileExtension.Length - 1));
                        }
                    }
                }
                catch (Exception e)
                {
                    Global.Logger.Error(e.Message);
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error(e.Message);
            }

            return result;
        }

        private string GetUri(IReadOnlyCollection<string> barcodes)
        {
            var filter = string.Join(" or FileLeafRef eq ", barcodes.Select(x => $"'{x}.{this.FileExtension}'"));

            return $"{this.searchUri} {filter}";
        }

        private ISet<string> SearchFiles(IReadOnlyCollection<string> barcodes)
        {
            var result = new Dictionary<string, string>();

            if (barcodes.Count == 0)
            {
                return new HashSet<string>();
            }

            try
            {
                Global.Logger.Info($"Ищу в ЭА наличие {barcodes.Count} штрих-кодов.");
                var response = this.RequestPresentFiles(barcodes);
                
                var jobj = JObject.Parse(response);
                var jarr = (JArray)jobj["d"]["results"];

                foreach (var jToken in jarr)
                {
                    var fileRef = jToken.SelectToken("FileRef")?.ToString();

                    if (!string.IsNullOrEmpty(fileRef))
                    {
                        var barcode = Path.GetFileNameWithoutExtension(fileRef);
                        
                        Global.Logger.Info($"Нашел в ЭА {barcode}. По пути: {fileRef}");

                        if (!result.ContainsKey(barcode))
                        {
                            result[barcode] = fileRef;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error($"Ошибка при получении путей файлов в SearchFiles. Пропущено {barcodes.Count}. {e.Message}");
            }

            this.FileReader.WriteLinesOnFile(barcodes, System.Configuration.ConfigurationManager.AppSettings["CheckedToExistance"]);
            
            this.FileReader.WriteNotDownloadedBarcodes(barcodes.Except(result.Keys));
            
            this.FileReader.WriteFoundedBarcodesInEa(result.Keys);

            return result.Values.ToHashSet();
        }

        private string RequestPresentFiles(IReadOnlyCollection<string> barcodes)
        {
            var endpointRequest = (HttpWebRequest)WebRequest.Create(this.GetUri(barcodes));

            base.ConfigureGetRequest(endpointRequest);

            return endpointRequest.GetResponseString();
        }
    }
}
