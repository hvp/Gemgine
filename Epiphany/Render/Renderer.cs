using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Epiphany.Render
{
    public class Renderer
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Camera Camera;
        public BasicEffect Effect { get; private set; }
        public Texture2D White { get; private set; }

        public Renderer(GraphicsDevice device)
        {
            this.GraphicsDevice = device;
            Effect = new BasicEffect(device);
            Camera = new Camera(device.Viewport);
            Effect.TextureEnabled = true;

            White = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            White.SetData(new Color[] { new Color(255, 255, 255, 255) });
            Effect.Texture = White;
        }

        public void ApplyEffect(Matrix world)
        {
            Effect.World = world;
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public void Draw(ISceneNode scene, RenderTarget2D target)
        {
            scene.UpdateWorldTransform(Camera.World);

            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

            Effect.CurrentTechnique = Effect.Techniques[0];
            Effect.Projection = Camera.Projection;
            Effect.View = Camera.View;
            
            scene.Draw(this);
        }
    }
}
