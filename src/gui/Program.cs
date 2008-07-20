﻿using System;
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
            // TODO: break this function into two
            // TODO: put the filename info config
            Config.Instance["Game.map"] = MapPersistence.readMapFromFile("../../../gui/testmap.txt");
            Game game = Game.Instance;
            form.refresh();
            game.loopStep += new EventHandler(form.repaintMap);
            game.loopStep += new EventHandler(form.refreshStatusLabels);
            game.onPause += new EventHandler(gamePause);
            game.onStart += new EventHandler(gameStart);
            game.onEnd += new EventHandler(form.repaintMap);
            game.onEnd += new EventHandler(form.refreshStatusLabels);
        }
        public static void stop() {
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
            timer.Interval = Config.Instance.getInt("Game.StepInterval");
            timer.Start();
            // Think of how to make the turns last the same time.
        }
    }
}