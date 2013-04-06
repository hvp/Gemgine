using System;

namespace Gem
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (Gem.Main main = new Gem.Main(""))
            {
                main.Run();
            }
        }
    }
#endif
}

