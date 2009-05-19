using System;
using System.Collections.Generic;
using System.Windows.Forms;

// $Id$

namespace Pango
{
    static class Program
    {
        static Timer timer = new Timer();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            PangoGameForm form = new PangoGameForm();
            initializeGame(form, args);
            Application.Run(form);
        }

        private static void initializeGame(PangoGameForm form, string[] args) {
            // the first command line argument is the map file name
            if (args.Length > 0) {
                Config.Instance["Game.mapFile"] = args[0];
            }
            string fileName = Config.Instance["Game.mapFile"];
            string map = string.Empty; // TODO: delete this as it is never used
            try {
                MapPersistence.loadMapsFromFile(fileName);
            }
            catch (System.IO.FileNotFoundException) {
                MessageBox.Show(string.Format("Map file {0} not found.", fileName), "Pango",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1); // OK? Why Application.Exit() doesn't work
            }
            Game game = Game.Instance;

            game.onLoadMap += new EventHandler(form.setWindowSize);
            //game.onLoopStep += new EventHandler(form.repaintMapLabel); // for text map
            game.onLoopStep += new EventHandler(form.repaintMapPictureBox);
            game.onLoopStep += new EventHandler(form.refreshStatusLabels);
            game.onPause += new EventHandler(gamePause);
            game.onStart += new EventHandler(gameStart);
            //game.onEnd += new EventHandler(form.repaintMapLabel); // for text map
            game.onEnd += new EventHandler(form.repaintMapPictureBox);
            game.onEnd += new EventHandler(form.refreshStatusLabels);

            game.loadMap();
            form.refresh();
        }

        private static void stop() {
            if (timer != null) { timer.Stop(); }
        }

        private static void gamePause(object sender, EventArgs e) {
            Program.stop();
        }

        private static void gameStep(object sender, EventArgs e) {
            Program.stop();
            if (Game.Instance.step()) {
                if (timer != null) {
                    timer.Start();
                }
            }
        }

        public static void gameStart(object sender, EventArgs e) {
            // Wait some time not to make the game so fast.
            // Think of how to make the turns last exactly the same time.
            timer = new Timer();
            timer.Tick += new EventHandler(gameStep);
            timer.Interval = Config.Instance.getInt("Game.stepInterval");
            timer.Start();
        }
    }
}