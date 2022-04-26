using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using Castle.Windsor;
using NLog;

namespace EaFileDownloader02
{
    abstract class EaConnectionBase
    {
        private readonly bool CreateFilesWithDateStructure;
        private readonly ICredentials credentials;
        protected readonly string baseUri;
        protected readonly string searchUri;
        protected readonly int batchSize;

        private readonly RemoteCertificateValidationCallback certValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;

        protected readonly IWindsorContainer container;
        private readonly string filesDirectory;

        /// <summary>
        /// Расширение файла по умолчанию (pdf)
        /// </summary>
        public static string DefaultFileExtension => "pdf";

        /// <inheritdoc />
        public string FileExtension => DefaultFileExtension;

        public Logger Logger => Global.Logger;

        /// <summary>
        /// .ctor
        /// </summary>
        protected EaConnectionBase()
        {
            this.container = Global.Container;
            
            if (string.IsNullOrEmpty(BarcodeSettingsConfig.Link)
                || string.IsNullOrEmpty(BarcodeSettingsConfig.Domain)
                || string.IsNullOrEmpty(BarcodeSettingsConfig.Login)
                || string.IsNullOrEmpty(BarcodeSettingsConfig.Password))
            {
                var except = "Не заполнены настройки подключения к ЭА";
                this.Logger.Error(except);
                throw new Exception(except);
            }

            this.baseUri = Utils.CombinePath(BarcodeSettingsConfig.Link);
            this.credentials = new NetworkCredential(BarcodeSettingsConfig.Login, BarcodeSettingsConfig.Password,
                BarcodeSettingsConfig.Domain);
            this.searchUri = this.GetSearchUri();

            this.filesDirectory = System.Configuration.ConfigurationManager.AppSettings["FilesDirectory"];

            var batchSize = System.Configuration.ConfigurationManager.AppSettings["BatchSize"];
            
            if (!Int32.TryParse(batchSize,out this.batchSize))
            {
                this.batchSize = 25;
            }
            
            this.CreateFilesWithDateStructure = System.Configuration.ConfigurationManager.AppSettings["CreateFilesWithDateStructure"] == "true";
        }

        protected string GetFilePath(string barcode, bool withDatesStructure = false)
        {
            if (withDatesStructure)
            {
                return Path.Combine(this.filesDirectory, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(),
                    $"{barcode}.{this.FileExtension}");
            }

            return Path.Combine(this.filesDirectory, $"{barcode}.{this.FileExtension}");
        }

        protected abstract string GetSearchUri();

        protected void ConfigureGetRequest(HttpWebRequest endpointRequest)
        {
            endpointRequest.Method = "GET";
            endpointRequest.ServerCertificateValidationCallback = this.certValidationCallback;
            endpointRequest.Accept = "application/json;odata=verbose";
            endpointRequest.Credentials = this.credentials;
        }

        protected byte[] GetFileBytes(string fileUrl)
        {
            var normalizedUrl = fileUrl.StartsWith(this.baseUri)
                ? fileUrl
                : Utils.CombinePath(this.baseUri, fileUrl);
            var endpointRequest = (HttpWebRequest)WebRequest.Create(normalizedUrl);
            endpointRequest.Method = "GET";
            endpointRequest.ServerCertificateValidationCallback = this.certValidationCallback;
            endpointRequest.Credentials = this.credentials;

            using (var webResponse = endpointRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var sr = new StreamReader(stream))
            using (var ms = new MemoryStream())
            {
                sr.BaseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        protected MemoryStream GetStream(string fileUrl)
        {
            var normalizedUrl = fileUrl.StartsWith(this.baseUri)
                ? fileUrl
                : Utils.CombinePath(this.baseUri, fileUrl);
            var endpointRequest = (HttpWebRequest)WebRequest.Create(normalizedUrl);
            endpointRequest.Method = "GET";
            endpointRequest.ServerCertificateValidationCallback = this.certValidationCallback;
            endpointRequest.Credentials = this.credentials;

            using (var webResponse = endpointRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var sr = new StreamReader(stream))
            using (var ms = new MemoryStream())
            {
                sr.BaseStream.CopyTo(ms);
                return ms;
            }
        }

        protected void ConfigurePostRequest(HttpWebRequest endpointRequest)
        {
            endpointRequest.Method = "POST";
            endpointRequest.ContentType = "application/json; charset=utf-8";
            endpointRequest.Credentials = this.credentials;
        }

        //protected FileInfo GetFileInfo(BaseParams baseParams)
        //{
        //    var url = baseParams.Params.GetAs<string>("url");
        //    var extension = baseParams.Params.GetAs<string>("extension");
        //    var title = baseParams.Params.GetAs<string>("title");

        //    var fileManager = this.container.Resolve<IFileManager>();

        //    using (this.container.Using(fileManager))
        //    {
        //        var bytes = this.GetFileBytes(url);

        //        return fileManager.SaveFile(Path.GetFileNameWithoutExtension(title), extension, bytes);
        //    }
        //}
    }
}