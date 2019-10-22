using System;

namespace イカP_1on1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                /*game.Window.ClientBounds.Left = 200;
                game.Window.ClientBounds.Top = 100;*/
                game.Run();
            }
        }
    }
#endif
}

