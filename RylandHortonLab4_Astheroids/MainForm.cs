//*************************************************************************
//Program:      Lab 4 - Astheroids
//Description:  The classic Asteroids game, the player pilots their ship and
//              navigates treacherous asteroids while destroying them with their
//              ship's weapon. Asteroids spawn faster as the player destroys more.
//              The player has a minimal number of deaths before they lose. The
//              game can be paused and resumed at any time.
//Author:       Ryland Horton
//Class:        CMPE2800
//Instructor:   Simon Walker
//**************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InputLayerSpace;
using System.Threading;
using IrrKlang;
using System.Diagnostics;

namespace RylandHortonLab4_Astheroids
{
    public partial class 
        MainForm : Form
    {
        //declare vars
        InputLayer _gameInputs = new InputLayer();      //input layer which reads player input
        GameInstance _currentGame;                      //an instance of the game to initialize

        //on form load, initialize a new game.
        public MainForm()
        {
            InitializeComponent();
            _currentGame = new GameInstance(CreateGraphics(), ClientRectangle, _gameInputs, this);
        }

        //when form closes, shut down any threads running in current game and input layer
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _currentGame.EndGame();
            _gameInputs.FormEnding();
        }


        //Key down/up events give information to input layer
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            _gameInputs.GiveEvent(e.KeyData, KeyState.Down);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            _gameInputs.GiveEvent(e.KeyData, KeyState.Up);
        }
    }
}
