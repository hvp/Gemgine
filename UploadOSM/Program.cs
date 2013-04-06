using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploadOSM
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new OSM.DatabaseService();

            try
            {
                var file = System.IO.File.Open(args[0], System.IO.FileMode.Open);
                Console.WriteLine((file.Length / 1024).ToString() + " kb");
                file.Close();

                Console.WriteLine("Uploading OSM file to database. This could take a while.");

                var cursorLeft = Console.CursorLeft;
                var cursorTop = Console.CursorTop;

                var counter = 0;
                var entries = 0;
                var spinner = 0;

                OSM.Import.ImportRawOSM(args[0], (entry) =>
                    {
                        database.Upsert(entry);
                        counter += 1;
                        entries += 1;

                        if (counter == 500)
                        {
                            spinner += 1;
                            if (spinner == 4) spinner = 0;
                            counter = 0;
                            Console.SetCursorPosition(cursorLeft, cursorTop);
                            switch (spinner)
                            {
                                case 0: Console.Write("| "); break;
                                case 1: Console.Write("/ "); break;
                                case 2: Console.Write("- "); break;
                                case 3: Console.Write("\\ "); break;
                            }
                            Console.Write(entries.ToString() + " entries uploaded.");
                        }
                    });

                Console.SetCursorPosition(cursorLeft, cursorTop);
                Console.WriteLine(entries.ToString() + " entries uploaded.");
                Console.WriteLine("Data uploaded.");
                database.CommitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
