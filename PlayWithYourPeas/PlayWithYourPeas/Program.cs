using System;

namespace PlayWithYourPeas
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PeasGame game = new PeasGame())
            {
                game.Run();
            }
        }
    }
#endif
}

