//*****************************************************************************************
//Class:        LifeDisplay
//Description:  This class tracks and displays the number of alives available to a player.
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
    class LifeDisplay : RenderableObj
    {

        //declare vars
        protected int _currentLives;    //the number of lives the player has
        protected Font _myFont;         //font to display in

        //gives life count to game instance
        public int CurrentLives { get { return _currentLives; } }

        public LifeDisplay(PointF location, int lives, int size)
        {
            _currentLives = lives;
            _location = location;
            _size = size;
            _myFont = new Font(FontFamily.GenericSansSerif, _size);
        }
        //render method displays lives as ellipses side to side
        public override void Render(Graphics gr)
        {
            gr.DrawString("Lives: ", _myFont, new SolidBrush(Color.White), _location);
            for (int i = 0; i < _currentLives; ++i)
            {
                gr.FillEllipse(new SolidBrush(Color.Red), _location.X + 100 + (i % 4) * _size, _location.Y + 15 + (15 * (i / 4)), 10, 10);
            }
        }
        public void TookDamage()
        {
            _currentLives -= 1;
        }
        public void BonusLife()
        {
            _currentLives += 1;
        }

    }
}
