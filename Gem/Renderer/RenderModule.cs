using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem;
using Gem.Renderer;
using Gem.Common;

namespace Gem.Renderer
{
    public class RenderModule : IModule
    {
        ICamera Camera = new OrbitCamera(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, 10);
        Effect drawEffect;
        DebugRenderer debug;
        Dictionary<UInt32, IRenderable> renderables = new Dictionary<uint, IRenderable>();
        GraphicsDevice device;

        Effect drawIDEffect;
        RenderTarget2D mousePickTarget;
        BlendState mousePickBlend = new BlendState();

        public RenderModule(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            debug = new DebugRenderer(device);
            drawEffect = content.Load<Effect>("draw");
            Camera.Viewport = device.Viewport;

            drawIDEffect = content.Load<Effect>("id");
            mousePickTarget = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Color, DepthFormat.Depth24);

            mousePickBlend.AlphaBlendFunction = BlendFunction.Add;
            mousePickBlend.AlphaDestinationBlend = Blend.Zero;
            mousePickBlend.AlphaSourceBlend = Blend.One;
        }

        public void Draw(OctTreeModule octTreeModule)
        {
            device.SetRenderTarget(null);
            device.Clear(Color.Black);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;

            drawEffect.Parameters["World"].SetValue(Matrix.Identity);
            drawEffect.Parameters["View"].SetValue(Camera.View);
            drawEffect.Parameters["Projection"].SetValue(Camera.Projection);
            drawEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity)));
            drawEffect.Parameters["DiffuseColor"].SetValue(new Vector4(1, 1, 0.4f, 1));
            drawEffect.Parameters["DiffuseLightDirection"].SetValue(Vector3.Normalize(new Vector3(1, 0, -1)));
            drawEffect.Parameters["FillColor"].SetValue(new Vector4(0.5f, 0.5f, 1, 1));
            drawEffect.Parameters["FillLightDirection"].SetValue(Vector3.Normalize(new Vector3(0, 1, 1)));
            drawEffect.Parameters["FogColor"].SetValue(Color.Black.ToVector4());
            drawEffect.CurrentTechnique = drawEffect.Techniques[0];
            

            var frustum = Camera.GetFrustum();
            var nodes = octTreeModule.Query(frustum).Distinct()
                .Select(id => renderables.ContainsKey(id) ? renderables[id] : null);

            foreach (var node in nodes)
            {
                drawEffect.Parameters["World"].SetValue(node.World);
                drawEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(node.World)));
                drawEffect.CurrentTechnique.Passes[0].Apply();
                if (node != null) (node as IRenderable).Draw(device);
            }
        }

        public UInt32 MousePick(OctTreeModule octTreeModule, Vector2 mouseCoordinates)
        {
            device.SetRenderTarget(mousePickTarget);
            device.Clear(ClearOptions.Target, Vector4.Zero, 0xFFFFFF, 0);
            device.BlendState = BlendState.Opaque;
            drawIDEffect.Parameters["World"].SetValue(Matrix.Identity);
            drawIDEffect.Parameters["View"].SetValue(Camera.View);
            var projection = Camera.GetSinglePixelProjection(mouseCoordinates);
            drawIDEffect.Parameters["Projection"].SetValue(projection);
            var frustum = new BoundingFrustum(Camera.View * projection);
            var nodes = octTreeModule.Query(frustum).Distinct()
                .Select(id => renderables.ContainsKey(id) ? renderables[id] : null);
            drawIDEffect.CurrentTechnique = drawIDEffect.Techniques[0];

            foreach (var node in nodes)
                if (node != null)
                {
                    drawIDEffect.Parameters["World"].SetValue(node.World);
                    //drawIDEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(node.World)));

                    var idBytes = BitConverter.GetBytes((node as Component).EntityID);
                    drawIDEffect.Parameters["ID"].SetValue(
                        new Vector4(idBytes[0] / 255.0f, idBytes[1] / 255.0f, idBytes[2] / 255.0f, idBytes[3] / 255.0f));
                    drawIDEffect.CurrentTechnique.Passes[0].Apply();
                    (node as IRenderable).Draw(device);
                }

            device.SetRenderTarget(null);
            var data = new Color[1];
            mousePickTarget.GetData(data);
            return data[0].PackedValue;
            var bytes = new byte[]
            {
                (byte)(data[0].B * 256f),
                (byte)(data[0].G * 256f),
                (byte)(data[0].R * 256f),
                (byte)(data[0].A * 256f)
            };
            return BitConverter.ToUInt32(bytes, 0);
        }

        void IModule.BeginSimulation(Simulation sim)
        {
        }

        void IModule.EndSimulation()
        {
        }

        void IModule.AddComponents(List<Component> components)
        {
            foreach (var component in components)
                if (component is IRenderable) renderables.Upsert(component.EntityID, component as IRenderable);
        }

        void IModule.RemoveEntities(List<UInt32> entities)
        {
            foreach (var id in entities) if (renderables.ContainsKey(id)) renderables.Remove(id);
        }

        void IModule.Update(float elapsedSeconds)
        {
        }

        void IModule.BindScript(MISP.Engine scriptEngine)
        {
            scriptEngine.AddFunction("c-model", "Create a model component.", (context, arguments) =>
                {
                    var model = GeometryGeneration.MispBinding.ModelArgument(arguments[0]);
                    return new ModelComponent(GeometryGeneration.CompiledModel.CompileModel(model, device));
                }, MISP.Arguments.Arg("model"));

            var meshFunction = GeometryGeneration.MispBinding.GenerateBindingObject();
            scriptEngine.AddGlobalVariable("mesh", (context) => { return meshFunction; });

            scriptEngine.AddGlobalVariable("camera", (context) => { return Camera; });
        }
    }
}
