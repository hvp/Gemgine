using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeometryGeneration;
using Microsoft.Xna.Framework;

namespace XNAConsole
{
    public class ModelComponent
    {
        public int id;

        public Action<ModelComponent> OnBoundsChanged;

        private Vector3 _position;
        private Quaternion _orientation;
        private BoundingSphere _boundingVolume = new BoundingSphere(Vector3.Zero, 1.0f);


        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _boundingVolume.Center = _position;
                if (OnBoundsChanged != null) OnBoundsChanged(this);
            }
        }

        public Quaternion Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public BoundingSphere BoundingVolume
        {
            get { return _boundingVolume; }
            set
            {
                _boundingVolume = value;
                _boundingVolume.Center = _position;
                if (OnBoundsChanged != null) OnBoundsChanged(this);
            }
        }
        private CompiledModel model = null;

        public ModelComponent(CompiledModel model)
        {
            this.model = model;
            this._boundingVolume = model.boundingSphere;
        }
        
        public Matrix World { get { return Matrix.Multiply(Matrix.CreateFromQuaternion(Orientation),
            Matrix.CreateTranslation(Position)); } }

        public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
        {
            model.Draw(GraphicsDevice);
        }

    }
}
