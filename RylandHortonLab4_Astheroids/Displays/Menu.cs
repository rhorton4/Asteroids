//*****************************************************************************************
//Class:        Menu
//Description:  This class represents a menu being displayed to the user.
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RylandHortonLab4_Astheroids
{
    class Menu : RenderableObj
    {
        //declare vars
        public enum MenuOption { Start, Resume, Restart, Exit } //describes possible menu options
        protected GameInstance.GameState _currentState;         //the state of the game instance
        protected int _selectedIndex = 0;                       //the index of the menu selected
        protected List<MenuOption> _availableOptions;           //a list of available options
        protected string _menuTitle;                            //title displayed above menu
        protected PointF _centerOfScreen;                       //center of screen to draw to
        protected Font _myFont;                                 //font to display in

        protected const float _widthOfButton = 200;             //how wide a button should be
        protected const float _heightOfButton = 40;             //height of buttons

        public Menu(GameInstance.GameState state, PointF center)
        {
            _currentState = state;
            _centerOfScreen = center;
            _availableOptions = new List<MenuOption>();

            //adds options to menu based on game state
            switch (state)
            {
                case GameInstance.GameState.PostGame:
                    _menuTitle = "Game over!";
                    _availableOptions.Add(MenuOption.Restart);
                    break;
                case GameInstance.GameState.PreGame:
                    _menuTitle = "Welcome to Astheroids!";
                    _availableOptions.Add(MenuOption.Start);
                    break;
                case GameInstance.GameState.PauseGame:
                    _menuTitle = "Paused";
                    _availableOptions.Add(MenuOption.Resume);
                    _availableOptions.Add(MenuOption.Restart);
                    break;
            }

            //exit is always available
            _availableOptions.Add(MenuOption.Exit);
            _myFont = new Font(FontFamily.GenericSansSerif, 24);     
        }

        //describes how to render menu to screen. Highlights currently selected item.
        public override void Render(Graphics gr)
        {
            StringFormat textStr = new StringFormat();
            textStr.Alignment = StringAlignment.Center;
            for (int i = 0; i < _availableOptions.Count; ++i)
            {
                gr.FillRectangle(new SolidBrush((i == _selectedIndex) ? Color.Purple : Color.Red),
                    new RectangleF(new PointF(_centerOfScreen.X - (_widthOfButton / 2), _centerOfScreen.Y - (_heightOfButton / 2) + (i * _heightOfButton * 2)),
                    new SizeF(_widthOfButton, _heightOfButton)));
                if (i == _selectedIndex)
                {
                    gr.DrawRectangle(new Pen(Color.Gold), new Rectangle(new Point((int)(_centerOfScreen.X - (_widthOfButton / 2)), (int)(_centerOfScreen.Y - (_heightOfButton / 2) + (i * _heightOfButton * 2))),
                            new Size((int)_widthOfButton, (int)_heightOfButton)));
                }
                gr.DrawString(_availableOptions[i].ToString(), _myFont, new SolidBrush((i == _selectedIndex) ? Color.Gold : Color.White),
                    new PointF(_centerOfScreen.X, _centerOfScreen.Y - _heightOfButton + (i * _heightOfButton * 2) + 20), textStr);
                gr.DrawString(_menuTitle, _myFont, new SolidBrush(Color.Yellow),
                    new PointF(_centerOfScreen.X, _centerOfScreen.Y - 75), textStr);    
            }                
        }

        //Method returns currently selected option to game instance.
        public MenuOption UserSelected()
        {
            return _availableOptions[_selectedIndex];
        }

        //Method moves menu cursor down
        public void MenuDown()
        {
            if (_selectedIndex == _availableOptions.Count - 1)
                _selectedIndex = 0;
            else
                ++_selectedIndex;
        }
        
        //Method moves menu cursor up
        public void MenuUp()
        {
            if (_selectedIndex == 0)
                _selectedIndex = _availableOptions.Count - 1;
            else
                --_selectedIndex;
        }
    }
}
