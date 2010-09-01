using System;
using System.Runtime.InteropServices;


namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                             HBProgram                             **
    /// ** Entry point for windows application                               **
    /// ** Game flow, http://blog.nickgravelyn.com/images/xna-game-flow.jpg  **
    /// ** and http://blog.nickgravelyn.com/2008/11/life-of-an-xna-game/     **
    /// ***********************************************************************
    /// </summary>
    static class HBProgram
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
                using (HBGame game = new HBGame()) // Initialization of our HBGame singleton class
                {
                    game.Run();
                }
            }
            catch (Exception e)
            {
                HBProgram.MessageBox(new IntPtr(0), "ERROR: " + e.ToString(), "Hungry Bee Error",0);  // Wont work on XBOX
            }
        }

    }
}

