//*****************************************************************************************
//Class:        Rock
//Description:  Represents a rock object to be shot by the player in the game.  
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
    class Rock : MovingObj
    {

        //declare vars
        public enum RockSize { Big = 1, Medium, Small }     //size of the rock
        protected const int _minSides = 6;                  //min number of sides in the rock's model
        protected const int _maxSides = 14;                 //max sides
        protected const int _minSpeed = -3;                 //rock's minimum speed in any direction
        protected const int _maxSpeed = 3;                  //rock's max speed
        protected const double _maxRot = 0.04;              //the most a rock can rotate in one frame
        protected const int _baseSize = 80;                 //base size of a rock
        protected readonly double _rotDelta;                //the amount this particular rock rotates in one frame
        protected readonly float _variance;                 //the variance in distance from center of rock to vertex
        protected int _fadeAmt = 0;                         //the amount the rock has faded into existence
        protected RockSize _mySizeReadable;                 //how big this rock is

        //accessor for if the rock is fully faded in
        public bool IsFadingIn { private set; get; }
        //if the rock should be removed from renderable list
        public bool FlaggedForRemoval { private set; get; }
        //accessor for how big the rock is
        public RockSize CurrentSize { get { return _mySizeReadable; } }

        //ctor
        public Rock(RockSize size, PointF location, bool IsNew)
        {
            _model = new GraphicsPath();
            _col = Color.Green;
            _location = location;
            _mySizeReadable = size;
            switch (size)
            {
                case RockSize.Small:
                    _size = _baseSize / 4;
                    break;
                case RockSize.Medium:
                    _size = _baseSize / 2;
                    break;
                case RockSize.Big:
                    _size = _baseSize;
                    break;
            }

            //generate rock model
            _variance = (float)(_size * 0.5);
            List<PointF> rockModel = GetPoly(_rng.Next(_minSides,_maxSides), _size, _variance);
            rockModel.ForEach(o => _model.AddLine(o, (o == rockModel.Last()) ? rockModel.First() : rockModel[rockModel.IndexOf(o) + 1]));

            //generate rock speed & rotation
            _xSpeed = _rng.Next(_minSpeed, _maxSpeed + 1);
            _ySpeed = _rng.Next(_minSpeed, _maxSpeed + 1);
            _rotDelta = _maxRot * _rng.NextDouble() * ((_rng.Next(1,3) > 1) ? 1 : -1);

            IsFadingIn = IsNew;
        }
        //*****************************************************************************************
        //Method name:  GetPoly(int vertices, float radius, float variance)
        //Description:  Generates a list of points to be turned into a rock's model
        //Parameters:   int vertices - the number of corners a rock has
        //              float radius - the max distance of a vertex from the center
        //              float variance - the variance in radial distance
        //Returns:      List<PointF> - a list of points to be turned into a graphics path
        //*****************************************************************************************
        public static List<PointF> GetPoly(int vertices, float radius, float variance)
        {
            List<PointF> returnList = new List<PointF>();
            double angle = 0;
            for (int step = 0; step < vertices; ++step, angle += (Math.PI * 2 / vertices))
            {
                PointF temp = new PointF();
                temp.X = (float)(Math.Sin(angle) * (radius - (_rng.NextDouble() * variance)));
                temp.Y = (float)(Math.Cos(angle) * (radius - (_rng.NextDouble() * variance)));
                returnList.Add(temp);
            }
            return returnList;
        }

        //allows main game thread to kill a rock
        public void Kill()
        {
            FlaggedForRemoval = true;
        }

        //tick for rock
        //handles rotation and rock fading
        public override void Tick(Rectangle client)
        {
            base.Tick(client);
            _rotation += _rotDelta;
            if (IsFadingIn && _fadeAmt < 255)
            {
                _fadeAmt += 5;
                _col = Color.FromArgb(_fadeAmt, 0, 100, 255);                
            }
            else if (_fadeAmt >= 255)
            {
                _col = Color.Green;
                IsFadingIn = false;
            }
        }
    }

    //TestRock class. Has no speed so mirroring can be tested easily.
    class TestRock : Rock
    {
        public TestRock(RockSize size, PointF loc, bool IsNew)
            : base(size, loc, IsNew)
        {
            _xSpeed = 0;
            _ySpeed = 0;
        }
    }
}
