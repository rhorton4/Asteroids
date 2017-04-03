//*****************************************************************************************
//Class:        Bullet
//Description:  A bullet shot from the ship which can impact and destroy asteroids.
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
    class Bullet : MovingObj
    {
        //declare fields
        protected double _direction;                        //the angle of direction for the bullet
        protected int _framesBulletHasLivedFor = 0;         //the amount of time, in frames, that the bullet has been alive for
        protected const double _speed = 15;                 //the speed at which the bullet moves
        protected const int _framesUntilBulletDies = 25;    //the time until the bullet dies
        public bool FlaggedForRemoval { private set; get; } //checks if the bullet should be removed from working game lists
        
        //ctor creates model, and sets bullet trajectory
        public Bullet(PointF spawnLoc, double angle)
        {
            _location = spawnLoc;

            _model = new GraphicsPath();
            _model.AddEllipse(-3, -3, 6, 6);
            
            //translate bullet to nose of ship
            Matrix moveBullet = new Matrix();
            moveBullet.Translate(0, -20, MatrixOrder.Prepend);
            moveBullet.Rotate((float)(angle * (180 / Math.PI)), MatrixOrder.Append);
            _model.Transform(moveBullet);

            _direction = angle;
            _size = 1;
            _xSpeed = (Math.Sin(_direction) * _speed);
            _ySpeed = (-Math.Cos(_direction) * _speed);
        }

        //tick moves bullet, and checks to see if it has lived long enough to die
        public override void Tick(Rectangle client)
        {
            base.Tick(client);
            if (_framesBulletHasLivedFor >= _framesUntilBulletDies)
                FlaggedForRemoval = true;
            ++_framesBulletHasLivedFor;
        }

        //Kill method
        //Allows the game instance to set the bullet's flag for removal.
        public void Kill()
        {
            FlaggedForRemoval = true;
        }
 
        public override void Render(Graphics gr)
        {
            GraphicsPath currModel = GetPath(_location);  
            gr.FillPath(new SolidBrush(Color.Purple), currModel);
        }
    }
}
