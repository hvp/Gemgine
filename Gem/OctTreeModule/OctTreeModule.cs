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

namespace Gem
{
    public class OctTreeModule : IModule
    {
        private OctCell octTreeRoot;
        private float minimumLeafSize;
        private Dictionary<UInt32, OctNode> allEntities = new Dictionary<uint, OctNode>();

        public OctTreeModule(BoundingBox worldBounds, float leafSize)
        {
            octTreeRoot = new OctCell(worldBounds);
            this.minimumLeafSize = leafSize;

            SpacialComponent.OnBoundsChanged += (component) =>
                {
                    if (allEntities.ContainsKey(component.EntityID))
                        allEntities[component.EntityID].boundsChanged = true;
                };
        }

#region IModule members
        void IModule.BeginSimulation(Simulation sim)
        {
        }

        void IModule.EndSimulation()
        {
        }

        void IModule.AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
                var spacial = component as SpacialComponent;
                if (spacial != null)
                    allEntities.Upsert(spacial.EntityID, new OctNode { spacial = spacial, bounds = spacial.BoundingVolume });
            }
        }

        void IModule.RemoveEntities(List<UInt32> entities)
        {
            foreach (var entity in entities)
            {
                if (allEntities.ContainsKey(entity))
                {
                    OctTree.RemoveNode(octTreeRoot, allEntities[entity]);
                    allEntities.Remove(entity);
                }
            }
        }

        void IModule.Update(float elapsedSeconds)
        {
            //For each item that has moved, remove from tree and re-insert.
            foreach (var entry in allEntities)
            {
                if (entry.Value.boundsChanged)
                {
                    OctTree.RemoveNode(octTreeRoot, entry.Value);
                    entry.Value.bounds = entry.Value.spacial.BoundingVolume;
                    OctTree.InsertNode(octTreeRoot, entry.Value, minimumLeafSize);
                    entry.Value.boundsChanged = false;
                }
            }
        }

        void IModule.BindScript(MISP.Engine scriptEngine)
        {

        }
#endregion

#region queries
        private void queryAction(List<UInt32> r, OctCell cell)
        {
            if (cell.contents != null) r.AddRange(cell.contents.Select((n) => { return n.spacial.EntityID; }));
        }

        public List<UInt32> Query(BoundingBox box)
        {
            var r = new List<UInt32>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<UInt32> Query(BoundingSphere box)
        {
            var r = new List<UInt32>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<UInt32> Query(BoundingFrustum box)
        {
            var r = new List<UInt32>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<UInt32> Query(Ray box)
        {
            var r = new List<UInt32>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }
#endregion
    }
}
