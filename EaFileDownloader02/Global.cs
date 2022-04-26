using System.Runtime.CompilerServices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NLog;

namespace EaFileDownloader02
{
    internal static class Global
    {
        public static WindsorContainer Container { get; set; }
        public static Logger Logger { get; set; }

        static Global()
        {
            Logger = LogManager.GetCurrentClassLogger();
            Container =  new WindsorContainer();
        }
    }
}