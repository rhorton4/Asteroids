//*****************************************************************************************
//Class:        Ship
//Description:  Represents a player's ship character in the game.  
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RylandHortonLab4_Astheroids
{
    public enum Direction { Forward, Backward, None }   //the types of thrust that can be applied
    public enum RotationType { Left, Right }            //how a ship can rotate
    class Ship : MovingObj
    {

        //declare vars
        protected const float _maxSpeed = 5;                    //ship's max movement in 1 frame
        protected const double _accel = 0.2f;                   //amount ship accelerates in 1 frame
        protected const int _maxFramesOfInvincibility = 100;    //how many frames of invincibility a ship gets
        protected const int _shipSize = 40;                     //approx how big the ship is
        protected bool _invincibleState;                        //if the ship is invincible or not
        protected int _framesOfCurrentInvincibility;            //how long the current invincibilty period has been

        //accessor for if the ship is invincible
        public bool IsInvincible { get { return _invincibleState; } }  
        public Ship(Point loc)
        {
            _col = Color.Red;

            //generate the ship's model
            _model = new GraphicsPath();
            //body
            _model.AddRectangle(new Rectangle(-10, -10, 20, 20));
            //nose
            _model.AddLine(new Point(-10, -10), new Point(0, -30));
            _model.AddLine(new Point(0, -30), new Point(10, -10));
            _model.CloseAllFigures();
            //left wing
            _model.AddLine(new Point(-10, -10), new Point(-25, 10));
            _model.AddLine(new Point(-25, 10), new Point(-10, 10));
            _model.AddLine(new Point(-10, 5), new Point(-10, -10));
            //right wing
            _model.AddLine(new Point(10, -10), new Point(25, 10));
            _model.AddLine(new Point(25, 10), new Point(10, 10));
            _model.AddLine(new Point(10, 10), new Point(10, -10));
            
            _location = loc;
            _size = _shipSize;
            _invincibleState = true;
        }

        //*****************************************************************************************
        //Method name:  Move(Direction forOrBack)
        //Description:  Changes a 
        //Parameters:   Firection forOrBack - which direction thrust is being applied
        //Returns:      void
        //*****************************************************************************************
        public void Move(Direction forOrBack)
        {
            //ship moves based on its current heading and type of thrust being applied.
            if (forOrBack != Direction.None)
            {
                _xSpeed = (forOrBack == Direction.Forward) ? _xSpeed + Math.Sin(_rotation) * _accel : _xSpeed - Math.Sin(_rotation) * _accel;
                _ySpeed = (forOrBack == Direction.Forward) ? _ySpeed - Math.Cos(_rotation) * _accel : _ySpeed + Math.Cos(_rotation) * _accel;

                //makes sure ship cannot accelerate past max speed
                if (Math.Abs(_xSpeed) > _maxSpeed)
                    _xSpeed = (_xSpeed * -1 < 0) ? _maxSpeed : -_maxSpeed;
                if (Math.Abs(_ySpeed) > _maxSpeed)
                    _ySpeed = (_ySpeed * -1 < 0) ? _maxSpeed : -_maxSpeed;
            }       
        }

        //rotates the ship based on direction of rotation
        public void Rotate(RotationType leftOrRight)
        {
            _rotation = (leftOrRight == RotationType.Left) ? _rotation - (float)_accel : _rotation + (float)_accel; 
        }

        //resets ship speed and location to a given point
        public void SetPosition(Point position)
        {
            _location = position;
            _rotation = 0;
            _xSpeed = 0;
            _ySpeed = 0;
        }

        //tick override
        //handles invincibility frames.
        public override void Tick(Rectangle client)
        {
            base.Tick(client);
            if (_invincibleState)
            {
                ++_framesOfCurrentInvincibility;
                if (_framesOfCurrentInvincibility > (_maxFramesOfInvincibility * 0.60) && _framesOfCurrentInvincibility % 5 == 0)
                {
                    _col = (_col == Color.Red) ? Color.LightBlue : Color.Red;
                }
                if (_framesOfCurrentInvincibility >= _maxFramesOfInvincibility)
                {
                    _invincibleState = false;
                    _col = Color.Red;
                }                   
            }
        }

        //Method for game instance to activate invincibility.
        public void ActivateInvincibility()
        {
            _invincibleState = true;
            _framesOfCurrentInvincibility = 0;
            _col = Color.LightBlue;
        }
    } 
}
