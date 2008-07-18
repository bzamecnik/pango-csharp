using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Pango;

namespace gui
{
    static class Program
    {
        static Timer timer = new Timer();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            PangoGameForm form = new PangoGameForm();
            initializeGame(form);
            Application.Run(form);
        }
        private static void initializeGame(PangoGameForm form) {
            //string[] maplines = { "XXXXX", "XX#@X", "XQ  X", "XH$LX", "XX", "XXXXX" };
            //string maptext = string.Join("\n", maplines);

            Map map = MapPersistence.FromString(MapPersistence.readMapFromFile("../../../testing/testmap.txt"));
            Game game = Game.Instance;
            game.loadMap(map);
            game.loopStep += new EventHandler(form.repaintMap);
            game.loopStep += new EventHandler(form.refreshStatusLabels);
            game.onPause += new EventHandler(gamePause);
            game.onStart += new EventHandler(gameStart);
        }
        private static void gamePause(object sender, EventArgs e) {
            timer.Stop();
        }
        private static void gameStep(object sender, EventArgs e) {
            timer.Stop();
            if (Game.Instance.step()) {
                gameStart(sender, new EventArgs());
            }
        }
        public static void gameStart(object sender, EventArgs e) {
            // Wait some time not to make the game so fast.
            timer = new Timer();
            timer.Tick += new EventHandler(gameStep);
            timer.Interval = 250;
            timer.Start();
            // Think of how to make the turns last the same time.
        }
    }
}