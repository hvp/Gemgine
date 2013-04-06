using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem.Common;
using Gem.Renderer;
using GeometryGeneration;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class SceneNode : List<SceneNode>, IRenderable
    {
        public Matrix localTransformation = Matrix.Identity;
        public Matrix worldTransformation = Matrix.Identity;
        public CompiledModel leaf = null;

        public Matrix World
        {
            get { return worldTransformation; }
        }

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m;
            foreach (var child in this)
                child.UpdateWorldTransform(worldTransformation * localTransformation);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
        {
            if (leaf != null)
            {

            }
        }

        public void DrawEx(RenderContext context)
        {
            if (leaf != null)
            {
                context.World = worldTransformation * localTransformation;
                context.Draw(leaf);
            }

            foreach (var child in this)
                child.DrawEx(context);
        }
    }

    public class SceneGraphRoot : Component, Renderer.IRenderable
    {
        public SceneNode rootNode;

        public SceneGraphRoot() { rootNode = new SceneNode(); }

        private SpacialComponent _spacial = null;

        public override void AssociateSiblingComponents(IEnumerable<Component> siblings)
        {
            foreach (var sibling in siblings)
                if (sibling is SpacialComponent) _spacial = sibling as SpacialComponent;
        }

        public Matrix World
        {
            get { throw new NotImplementedException(); }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
        {
            throw new NotImplementedException();
        }

        public void DrawEx(RenderContext context)
        {
            var worldTransform = _spacial.Transform;
            rootNode.UpdateWorldTransform(worldTransform);
            rootNode.DrawEx(context);
        }
    }

    //public class ModelComponent : Component, Renderer.IRenderable
    //{
    //    private SpacialComponent _spacial = null;
    //    private CompiledModel model = null;
    //    public Matrix transformation = Matrix.Identity;

    //    public ModelComponent(CompiledModel model)
    //    {
    //        this.model = model;
    //    }

    //    public override void AssociateSiblingComponents(IEnumerable<Component> siblings)
    //    {
    //        foreach (var sibling in siblings)
    //            if (sibling is SpacialComponent) _spacial = sibling as SpacialComponent;
    //        if (_spacial != null && model != null)
    //            _spacial.BoundingVolume = model.boundingSphere;
    //    }

    //    public Matrix World { get { return Matrix.Multiply(Matrix.CreateFromQuaternion(_spacial.Orientation),
    //        Matrix.CreateTranslation(_spacial.Position)); } }

    //    public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
    //    {
    //        model.Draw(GraphicsDevice);
    //    }

    //    public void DrawEx(RenderContext context)
    //    {
    //        context.World = transformation;
    //        context.Draw(model);
    //    }

    //}
}
