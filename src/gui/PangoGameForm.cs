using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Pango;

// $Id$

namespace gui
{
    public partial class PangoGameForm : Form
    {
        public PangoGameForm() {
            InitializeComponent();
        }

        private void processKeyboardInput(object sender, KeyEventArgs e) {
            Game game = Game.Instance;
            switch(game.State) {
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
                    game.endGame();
                    break;
            }
            refreshStatusLabels(this, new EventArgs());
        }

        public void repaintMapLabel(object sender, EventArgs e) {
            if (Game.Instance.Map != null) {
                mapLabel.Text = Game.Instance.Map.ToString();
                mapLabel.Refresh();
            }
            
        }

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

        public void setMapPictureBoxSize(object sender, EventArgs e) {
            if (Game.Instance.Map == null) { return; }
            // TODO: put these constants into config
            mapPictureBox.Height = Game.Instance.Map.Height * 32;
            mapPictureBox.Width = Game.Instance.Map.Width * 32;
            // TODO: set window size
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