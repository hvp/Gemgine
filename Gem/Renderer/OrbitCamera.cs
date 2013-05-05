﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Renderer
{
    /// <summary>
    /// This camera orbits the position
    /// </summary>
    public class OrbitCamera : ICamera
    {
        public Vector3 Position { get; set; }
        public Vector3 GetPosition() { return Position - (Forward * OrbitDistance); }
        private Vector3 Forward;
        private Vector3 Up;
        public Viewport Viewport { get; set; }
        public float Fov = 45.0f;
        public float NearPlane = 1.0f;
        public float FarPlane { get; set; }
        public float OrbitDistance { get; set; }

        public OrbitCamera(Vector3 Position, Vector3 Forward, Vector3 Up, float OrbitDistance)
        {
            this.Position = Position;
            this.Forward = Forward;
            this.Up = Up;

            this.Forward.Normalize();
            this.Up.Normalize();
            FarPlane = 1000.0f;

            this.OrbitDistance = OrbitDistance;
        }

        public void Yaw(float f)
        {
            var Q = Quaternion.CreateFromAxisAngle(Up, f);
            Forward = Vector3.Transform(Forward, Q);
        }

        public void Pitch(float f)
        {
            var Q = Quaternion.CreateFromAxisAngle(Vector3.Cross(Forward, Up), f);
            Forward = Vector3.Transform(Forward, Q);
            Up = Vector3.Transform(Up, Q);
        }

        public void Roll(float f)
        {
            var Q = Quaternion.CreateFromAxisAngle(Forward, f);
            Up = Vector3.Transform(Up, Q);
        }

        public void Pan(float X, float Y, float speed)
        {
            Position += Forward * Y * speed;
            Position += Vector3.Cross(Forward, Up) * X * speed;
        }

        public void Zoom(float d)
        {
            OrbitDistance += d;
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(Position - (Forward * OrbitDistance), Position, Up);
            }
        }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspective((float)Viewport.Width / (float)Viewport.Height, 1.0f, NearPlane, FarPlane);
            }
        }

        public Vector3 Unproject(Vector3 Pos)
        {
            return Viewport.Unproject(Pos, Projection, View, Matrix.Identity);
        }

        public Vector3 Project(Vector3 vec)
        {
            return Viewport.Project(vec, Projection, View, Matrix.Identity);
        }

        public Matrix GetSinglePixelProjection(Vector2 Pixel)
        {
            var NP0 = Viewport.Unproject(new Vector3(Pixel, 0), Projection, Matrix.Identity, Matrix.Identity);
            var NP1 = Viewport.Unproject(new Vector3(Pixel + Vector2.One, 0), Projection, Matrix.Identity, Matrix.Identity);
            var Min = new Vector2(System.Math.Min(NP0.X, NP1.X), System.Math.Min(NP0.Y, NP1.Y));
            var Max = new Vector2(System.Math.Max(NP0.X, NP1.X), System.Math.Max(NP0.Y, NP1.Y));
            return Matrix.CreatePerspectiveOffCenter(Min.X, Max.X, Min.Y, Max.Y, NearPlane, FarPlane);
        }

        public BoundingFrustum GetFrustum()
        {
            return new BoundingFrustum(View * Projection);
        }
    }
}
