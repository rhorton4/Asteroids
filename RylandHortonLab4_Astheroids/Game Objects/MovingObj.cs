//*****************************************************************************************
//Class:        MovingObj
//Description:  The base class for all other game object classes. Represents a game 
//              object that can move around in the game space.
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
    abstract class MovingObj : RenderableObj
    {
        //declare fields
        protected double _xSpeed;                                   //x-speed of the object
        protected double _ySpeed;                                   //y-speed of the object
        protected Color _col;                                       //color to draw the object
        protected List<PointF> _myMirrors = new List<PointF>();     //a list of points at which the object is mirrored across the screen
        public List<PointF> AllLocs                                 //a combined list of all locations at which the object has a model (mirrors + real)
        {
            get
            {
                List<PointF> whereIam = new List<PointF>();
                whereIam.AddRange(_myMirrors);
                whereIam.Add(_location);
                return whereIam;
            }
        }

        //*****************************************************************************************
        //Method name:  Tick(Rectangle client)
        //Purpose:      Represents the behaviour of an object across one frame. Generally
        //              this is movement and collision checks.
        //Parameters:   Rectangle client - the rectangle of the graphics area
        //Returns:      none
        //*****************************************************************************************
        public virtual void Tick(Rectangle client)
        {
            bool xMir = false;
            bool yMir = false;
            float x = _location.X + (float)_xSpeed;
            float y = _location.Y + (float)_ySpeed;

            //move object to other side of the screen if it would be off of it.
            if (x > client.Width)
                x = 0;
            if (x < 0)
                x = client.Width;
            if (y > client.Height)
                y = 0;
            if (y < 0)
                y = client.Height;

            //add mirrors to list of mirror points
            _myMirrors.Clear();
            if (x - _size < 0 || x + _size > client.Width)
                xMir = true;            
            if (y - _size < 0 || y + _size > client.Height)
                yMir = true;                
            if (xMir)
                _myMirrors.Add(new PointF((x - _size < 0) ? _location.X + client.Width : _location.X - client.Width, _location.Y));
            if (yMir)
                _myMirrors.Add(new PointF(_location.X, (y - _size < 0) ? _location.Y + client.Height : _location.Y - client.Height));
            if (xMir && yMir)
                _myMirrors.Add(new PointF((x - _size < 0) ? _location.X + client.Width : _location.X - client.Width,
                                            (y - _size < 0) ? _location.Y + client.Height : _location.Y - client.Height));

            _location = new PointF(x, y);
        }
        //*****************************************************************************************
        //Method name:  Render(Graphics gr)
        //Description:  Method that describes how the object should draw itself to the canvas. Also draws
        //              its mirrors.
        //Parameters:   Graphics gr - the graphics area to draw to
        //Returns:      none
        //*****************************************************************************************
        public override void Render(Graphics gr)
        {
            GraphicsPath currModel = GetPath(_location);           
            gr.DrawPath(new Pen(_col), currModel);
            _myMirrors.ForEach(o =>            
            {
                currModel = GetPath(o);
                gr.DrawPath(new Pen(_col), currModel);
            });
        }
        //*****************************************************************************************
        //Method name:  IsClose(MovingObj other)
        //Description:  Checks if two objects are close enough to begin actual hit detection
        //Parameters:   MovingObj other - the other object to be checked
        //Returns:      bool - if they are close or not
        //*****************************************************************************************
        public bool IsClose(MovingObj other)
        {
            bool retVal = false;
            other.AllLocs.ForEach(o =>
            {
                if (Math.Sqrt(Math.Pow(_location.X - o.X, 2) + Math.Pow(_location.Y - o.Y, 2)) < _size * 2 + other._size * 2)
                    retVal = true;
            });
            return retVal;
        }
        //*****************************************************************************************
        //Method name:  IsColliding(MovingObj other, Graphics gr)
        //Description:  Checks if two object's regions are colliding.
        //Parameters:   MovingObj other - the object to check against
        //              Graphics gr - the graphics area on which they are drawn
        //Returns:      bool - if they are colliding or not
        //*****************************************************************************************
        public bool IsColliding(MovingObj other, Graphics gr)
        {
            Region myRegion;
            Region othRegion;
            bool retVal = false;
            other.AllLocs.ForEach(o =>
            {
                myRegion = new Region(GetPath(_location));
                othRegion = new Region(other.GetPath(o));
                myRegion.Intersect(othRegion);
                if (!myRegion.IsEmpty(gr))
                    retVal = true;
            });
            return retVal;
        }
    }
}
