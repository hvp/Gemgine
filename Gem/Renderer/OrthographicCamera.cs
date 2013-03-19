using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Renderer
{
    public class OrthographicCamera// : ICamera
    {
        public Vector2 focus;
		public float zoom = 1.0f;
        public Viewport Viewport { get; set; }
		public Vector2 viewportDimensions;
        public float rotation = 0.0f;

        public OrthographicCamera(Viewport viewport)
        {
            this.Viewport = viewport;
            this.viewportDimensions = new Vector2(viewport.Width, viewport.Height);
        }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreateOrthographicOffCenter(-viewportDimensions.X / 2, viewportDimensions.X / 2,
                    viewportDimensions.Y / 2, -viewportDimensions.Y / 2, -32, 32);
            }
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateTranslation(-focus.X, -focus.Y, 0.0f) 
                    * Matrix.CreateRotationZ(rotation) 
                    * Matrix.CreateScale(zoom);
            }
        }

        public Matrix World
        {
            get
            {
                return Matrix.Identity;
            }
        }

        /// <summary>
        /// Transforms a position from screen space to world space.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 Unproject(Vector3 vec)
        {
            return Viewport.Unproject(vec, Projection, View, World);
        }

        /// <summary>
        /// Transforms a position from world space to screen space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 Project(Vector3 vec)
        {
            return Viewport.Project(vec, Projection, View, World);
        }


        internal void confineInRect(int x, int y, int gwidth, int gheight)
        {
            if (focus.X < x) focus.X = x;
            if (focus.X > x + gwidth) focus.X = x + gwidth;
            if (focus.Y < y) focus.Y = y;
            if (focus.Y > y + gheight) focus.Y = y + gheight;
        }
    }

}
