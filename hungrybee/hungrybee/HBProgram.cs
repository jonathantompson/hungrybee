using System;

namespace hungrybee
{
    static class HBProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (HBGame game = new HBGame()) // Initialization of our HBGame singleton class
            {
                game.Run();
            }
        }

    }
}

