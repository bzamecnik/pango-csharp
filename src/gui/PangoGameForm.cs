using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

// $Id$

namespace Pango
{
    public partial class PangoGameForm : Form
    {
        public PangoGameForm() {
            InitializeComponent();
        }

        private void processKeyboardInput(object sender, KeyEventArgs e) {
            Game game = Game.Instance;
            switch(game.State) {
                case Game.States.Intro:
                    switch (e.KeyCode) {
                        case Keys.Space:
                            this.helpLabel.Hide();
                            this.statusStrip.Show();
                            game.start();
                            break;
                        case Keys.Escape:
                            Environment.Exit(0);
                            break;
                    }
                    break;
                case Game.States.Running:
                    if (game.Player != null) {
                        switch (e.KeyCode) {
                            case Keys.Up:
                                game.Player.requestMovement(Direction.Up);
                                break;
                            case Keys.Right:
                                game.Player.requestMovement(Direction.Right);
                                break;
                            case Keys.Down:
                                game.Player.requestMovement(Direction.Down);
                                break;
                            case Keys.Left:
                                game.Player.requestMovement(Direction.Left);
                                break;
                            case Keys.Space:
                                game.Player.requestAttack();
                                break;
                        }
                    }
                    break;
                case Game.States.Prepared:
                    switch (e.KeyCode) {
                        case Keys.Space:
                            game.start();
                            break;
                    }
                    break;
            }
            switch (e.KeyCode) {
                case Keys.P:
                    game.pause();
                    break;
                case Keys.Escape:
                    game.endGameImmediately();
                    this.helpLabel.Show();
                    //this.statusStrip.Hide();
                    break;
            }
            refreshStatusLabels(this, new EventArgs());
        }

        //public void repaintMapLabel(object sender, EventArgs e) {
        //    if (Game.Instance.Map != null) {
        //        mapLabel.Text = Game.Instance.Map.ToString();
        //        mapLabel.Refresh();
        //    }
            
        //}

        public void refreshStatusLabels(object sender, EventArgs e) {
            levelValueLabel.Text = Game.Instance.Level.ToString();
            timeValueLabel.Text = Game.Instance.Time.ToString();
            gameStateValueLabel.Text = Game.Instance.State.ToString();
            moneyValueLabel.Text = Game.Instance.Money.ToString();
            PlayerEntity player = Game.Instance.Player;
            if (player != null) {
                healthValueLabel.Text = Game.Instance.Player.Health.ToString();
                livesValueLabel.Text = Game.Instance.Player.Lives.ToString();
            }
            Refresh();
        }

        public void refresh() {
            //repaintMapLabel(this, new EventArgs());
            repaintMapPictureBox(this, new EventArgs());
            refreshStatusLabels(this, new EventArgs());
        }

        public void setWindowSize(object sender, EventArgs e) {
            Map map = Game.Instance.Map;
            if (map == null) { return; }
            Size spriteSize = entitiesImageList.ImageSize;
            // map + status strip
            Size newSize = new Size(map.Width * spriteSize.Width,
                (map.Height * spriteSize.Height) + statusStrip.Height);
            // set window's inner size
            this.ClientSize = newSize;

        }

        public void repaintMapPictureBox(object sender, EventArgs e) {
             mapPictureBox.Invalidate();
        }

        private void mapPictureBox_Paint(object sender, PaintEventArgs e) {
            Map map = Game.Instance.Map;
            if (map == null) { return; }
            
            Graphics graphics = e.Graphics;
            for (int y = 0; y < map.Width; y++) {
                for (int x = 0; x < map.Height; x++) {
                    foreach (Entity ent in map.getPlace(new Coordinates(x,y))) {
                        string entDesc = string.Format("{0}.png", ent.ToString());
                        if (entitiesImageList.Images.ContainsKey(entDesc)) {
                            graphics.DrawImage(entitiesImageList.Images[entDesc], new Point(32 * y, 32 * x));
                        }
                    }
                }
            }
        }
    }
}