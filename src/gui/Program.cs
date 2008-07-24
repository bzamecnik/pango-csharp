using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Pango;

// $Id$

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
            // TODO: break this function into two
            // TODO: put the filename info config
            string fileName = "../../testmap.txt";
            string map = string.Empty;
            try {
                 map = MapPersistence.readMapFromFile(fileName);
            }
            catch (System.IO.FileNotFoundException) {
                MessageBox.Show(string.Format("Map file {0} not found.", fileName), "Pango",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1); // OK? Why Application.Exit() doesn't work
            }
            Config.Instance["Game.map"] = map;
            Game game = Game.Instance;
            game.loadMap();
            form.refresh();
            game.onLoopStep += new EventHandler(form.repaintMap);
            game.onLoopStep += new EventHandler(form.refreshStatusLabels);
            game.onPause += new EventHandler(gamePause);
            game.onStart += new EventHandler(gameStart);
            game.onEnd += new EventHandler(form.repaintMap);
            game.onEnd += new EventHandler(form.refreshStatusLabels);
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
                gameStart(sender, new EventArgs());
            }
        }
        public static void gameStart(object sender, EventArgs e) {
            // Wait some time not to make the game so fast.
            timer = new Timer();
            timer.Tick += new EventHandler(gameStep);
            timer.Interval = Config.Instance.getInt("Game.stepInterval");
            timer.Start();
            // Think of how to make the turns last the same time.
        }
    }
}