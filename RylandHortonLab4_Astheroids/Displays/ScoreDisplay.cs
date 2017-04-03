//*****************************************************************************************
//Class:        ScoreDisplay
//Description:  This class holds score information and displays it.
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
    class ScoreDisplay : RenderableObj
    {
        //declare fields
        protected int _currentScore;    //the player's score
        protected Font _myFont;         //font to display with
        
        //returns score to game instance
        public int CurrentScore { get { return _currentScore; } }   

        public ScoreDisplay(PointF location, int size)
        {
            _size = size;
            _myFont = new Font(FontFamily.GenericSansSerif, _size);
            _currentScore = 0;
        }

        //Renders string of score to screen.
        public override void Render(Graphics gr)
        {
            gr.DrawString("Score: " + _currentScore.ToString(), _myFont, new SolidBrush(Color.White), _location);
        }
        
        //self-explanatory add to score method
        public void AddToScore(int add)
        {
            _currentScore += add;
        }
    }
}
