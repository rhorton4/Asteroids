//*****************************************************************************************
//Class:        RenderableObj
//Description:  Abstract class representing any piece of the game that needs to be rendered.  
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
    abstract class RenderableObj
    {
        //declare vars
        protected static Random _rng = new Random();        //random number generator
        protected GraphicsPath _model;                      //the model of the object
        protected PointF _location;                         //where the model exists on the canvas
        protected int _size = 1;                            //the relative size of the object's model
        protected double _rotation;                         //the amount to rotate the object's model

        //accessor for object's location
        public PointF Pos { get { return _location; } }
        //accessor for object's rotation
        public double Heading { get { return _rotation; } }
        //abstract method describes how a particular object will render itself
        public abstract void Render(Graphics gr);

        //************************************************************************
        //Method name:  GetPath(PointF loc)
        //Purpose:      Creates a graphics path of the object's model rotated and
        //              translated to a specific location.
        //Parameters:   PointF loc - the point to translate the model to.
        //Returns:      void
        //************************************************************************
        public GraphicsPath GetPath(PointF loc)
        {
            Matrix mat = new Matrix();
            mat.Translate(loc.X, loc.Y);
            mat.Rotate((float)(_rotation * (180 / Math.PI)), MatrixOrder.Prepend);
            GraphicsPath currModel = (GraphicsPath)_model.Clone();
            currModel.Transform(mat);
            return currModel;
        }
        
    }
}
