using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Renderer;
using Gem.Common;
using Gem.Gui;
using GeometryGeneration;

namespace Gem
{
    public class GuiSceneNode : SceneNode
    {
        public GuiModule module = null;
        public OrthographicCamera uiCamera = null;
        public Gui.UIItem uiRoot = null;
        public RenderTarget2D renderTarget = null;
        public GeometryGeneration.CompiledModel quadModel = null;

        public bool MouseHover = false;
        public int LocalMouseX = 0;
        public int LocalMouseY = 0;
        
        public GuiSceneNode(int width, int height, GuiModule module)
        {
            this.module = module;

            uiCamera = new OrthographicCamera(new Viewport(0,0, width, height));
            uiRoot = new UIItem(new Rectangle(0,0,width,height));
            uiRoot.defaults = module.defaultSettings;
            uiRoot.settings = new MISP.GenericScriptObject();
            
            uiCamera.focus = new Vector2(width / 2, height / 2);
        }

        public void ClearUI() { uiRoot.children.Clear(); }

        public void HandleInput(Input input)
        {
            uiRoot.Update(input, module);
        }

        public override void PreDraw(GraphicsDevice device, RenderContext context)
        {
            if (renderTarget == null)
            {
                renderTarget = new RenderTarget2D(device, uiCamera.Viewport.Width, uiCamera.Viewport.Height);
                var rawGuiQuad = Gen.CreateTexturedQuad();
                Gen.Colorize(rawGuiQuad, Vector4.One);
                rawGuiQuad = Gen.FacetCopy(rawGuiQuad);
                quadModel = CompiledModel.CompileModel(new RawModel(rawGuiQuad), device);
            }

            module.DrawRoot(uiRoot, uiCamera, renderTarget);
        }

        public override void DrawEx(RenderContext context)
        {
            if (renderTarget != null)
            {
                context.Texture = renderTarget;
                context.World = localTransformation;
                context.DrawTexturedFullbright(quadModel);
            }
        }

        public static float ScalarProjection(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(A, B) / B.Length();
        }

        public override void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            MouseHover = false;

            var verts = new Vector3[3];
            verts[0] = new Vector3(-0.5f, -0.5f, 0);
            verts[1] = new Vector3(0.5f, -0.5f, 0);
            verts[2] = new Vector3(-0.5f, 0.5f, 0);

            for (int i = 0; i < 3; ++i)
                verts[i] = Vector3.Transform(verts[i], localTransformation);

            debug(new VertexPositionColor(verts[0], Color.Red), new VertexPositionColor(verts[1], Color.Red));
            debug(new VertexPositionColor(verts[0], Color.Green), new VertexPositionColor(verts[2], Color.Green));

            var distance = mouseRay.Intersects(new Plane(verts[0], verts[1], verts[2]));
            if (distance == null || !distance.HasValue) return;
            if (distance.Value < 0) return; //GUI plane is behind camera
            var interesectionPoint = mouseRay.Position + (mouseRay.Direction * distance.Value);

            debug(new VertexPositionColor(verts[0], Color.Blue), new VertexPositionColor(interesectionPoint, Color.Blue));

            var x = ScalarProjection(interesectionPoint - verts[0], verts[1] - verts[0]) / (verts[1] - verts[0]).Length();
            var y = ScalarProjection(interesectionPoint - verts[0], verts[2] - verts[0]) / (verts[2] - verts[0]).Length();

            LocalMouseX = (int)(x * uiCamera.viewportDimensions.X);
            LocalMouseY = (int)(y * uiCamera.viewportDimensions.Y);

            MouseHover = true;
        }
    }
}
