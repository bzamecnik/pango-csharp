using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Pango;

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
                    Program.stop();
                    game.endGame();
                    break;
            }
            refreshStatusLabels(this, new EventArgs());
        }
        public void repaintMap(object sender, EventArgs e) {
            mapLabel.Text = Game.Instance.Map.ToString();
            mapLabel.Refresh();
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
            repaintMap(this, new EventArgs());
            refreshStatusLabels(this, new EventArgs());
        }
    }
}