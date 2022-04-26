using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EaFileDownloader02
{
    internal static class BarcodeSettingsConfig
    {
        /// <summary>
        /// Ссылка на сайт
        /// </summary>
        public static string Link { get; set; }

        /// <summary>
        /// Домен
        /// </summary>
        public static string Domain { get; set; }

        /// <summary>
        /// Логин
        /// </summary>
        public static string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public static string Password { get; set; }

        static BarcodeSettingsConfig()
        {
            Link = System.Configuration.ConfigurationManager.AppSettings["Link"];
            Domain = System.Configuration.ConfigurationManager.AppSettings["Domain"];
            Login = System.Configuration.ConfigurationManager.AppSettings["Login"];
            Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
        }
    }
}
