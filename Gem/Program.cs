using System;

namespace Gem
{
#if WINDOWS || XBOX
    static class Program
    {
        static void UnhandledHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            System.IO.File.AppendAllText("log.txt", e.Message + "\n" + e.StackTrace);
        }

        static void FirstChanceHandler(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
        {
            Exception e = (Exception)args.Exception;
            System.IO.File.AppendAllText("log.txt", e.Message + "\n" + e.StackTrace);
        }

        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledHandler);
            //AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(FirstChanceHandler);
            Console.SetError(System.IO.File.AppendText("log.txt"));
            using (Gem.Main main = new Gem.Main("start-server-session 8669 \"Content\""))
            {
                main.Run();
            }
            while (true) ;
        }
    }
#endif
}

