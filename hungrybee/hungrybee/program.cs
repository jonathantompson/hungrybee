using System;
using System.Runtime.InteropServices;


namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                              program                              **
    /// ** Entry point for windows application                               **
    /// ** Game flow, http://blog.nickgravelyn.com/images/xna-game-flow.jpg  **
    /// ** and http://blog.nickgravelyn.com/2008/11/life-of-an-xna-game/     **
    /// ***********************************************************************
    /// </summary>
    static class program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        /// <summary>
        /// Main --> Entry Point
        /// ***********************************************************************
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                using (game game = new game()) // Initialization of our game singleton class
                {
                    game.Run();
                }
            }
            catch (Exception e)
            {
                program.MessageBox(new IntPtr(0), "ERROR: " + e.ToString(), "Hungry Bee Error",0);  // Wont work on XBOX
            }
        }

    }
}

