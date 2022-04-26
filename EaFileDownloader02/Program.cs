using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.Windsor;
using EaFileDownloader02.Services;
using NLog;
using Component = Castle.MicroKernel.Registration.Component;

namespace EaFileDownloader02
{
    class Program
    {
        static void Main(string[] args)
        {
            RegisterComponents();
            Run();
        }

        private static void Run()
        {
            var container = Global.Container;
            var logger = Global.Logger;

            var downloadService = container.Resolve<IBarcodeDownloadService>();
            var barcodeReader = container.Resolve<IFileReader>();

            try
            {
                Console.WriteLine($"Все проверенные штрих-коды больше не проверяются на наличиев в ЭА. Очистите {System.Configuration.ConfigurationManager.AppSettings["FileRoutesInEa"]} если хотите проверить все коды.");
                Console.WriteLine($"Все скачанные файлы больше не скачиваются и не проверяются на наличие в ЭА. Очистите {System.Configuration.ConfigurationManager.AppSettings["DownloadBarcodesFile"]} если хотите проверить и скачать все коды.");
                Console.WriteLine();
                
                var mode = GetMode();
            
                switch (mode.Trim())
                {
                    case "1":
                        logger.Info("Старт скачивания файлов.");
                        downloadService.DownloadFilesToStorage(barcodeReader.ReadBarcodesFromFile());
                        break;
                    
                    case "2":
                        logger.Info($"Старт проверки наличия файлов в ЭА. Успешно проверенные пути запишутся в файл {System.Configuration.ConfigurationManager.AppSettings["FileRoutesInEa"]}.");
                        logger.Info($"Успешно проверенные значения штрих-кодов запишутся в файл {System.Configuration.ConfigurationManager.AppSettings["FoundedBarcodesInEa"]}.");
                        downloadService.FindBarcodesOnRemoteStorage(barcodeReader.ReadBarcodesFromFile());
                        break;
                }
            }
            finally
            {
                container.Release(downloadService);
                container.Release(barcodeReader);
            }
        }

        private static void RegisterComponents()
        {
            var container = Global.Container;

            container.Register(Component.For<IBarcodeDownloadService>().ImplementedBy<BarcodeDownloadService>());
            container.Register(Component.For<IFileReader>().ImplementedBy<FileReader>());
        }

        private static string GetMode()
        {
            Console.WriteLine("Введите 1 и нажмите Enter, если хотите скачать файлы и ЭА.");
            Console.WriteLine("Введите 2 и нажмите Enter, если хотите проверить наличие файлов в ЭА.");
            return Console.ReadLine();
        }
    }
}