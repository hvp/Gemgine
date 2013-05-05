using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace XNAConsole
{
    public class Scene
    {
        private OctCell octTreeRoot;
        private float minimumLeafSize;
        public Dictionary<int, OctNode> allEntities = new Dictionary<int, OctNode>();

        public Scene(BoundingBox worldBounds, float leafSize)
        {
            octTreeRoot = new OctCell(worldBounds);
            this.minimumLeafSize = leafSize;

        }

        public void AddModel(ModelComponent model)
        {
            allEntities.Upsert(model.id, new OctNode { spacial = model, bounds = model.BoundingVolume, boundsChanged = true });
            model.OnBoundsChanged += (m) => { if (allEntities.ContainsKey(m.id)) allEntities[m.id].boundsChanged = true; };
        }

        public void RemoveModel(int id)
        {
                if (allEntities.ContainsKey(id))
                {
                    OctTree.RemoveNode(octTreeRoot, allEntities[id]);
                    allEntities.Remove(id);
                }
        }

        public void Update()
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

#region queries
        private void queryAction(List<ModelComponent> r, OctCell cell)
        {
            if (cell.contents != null) r.AddRange(cell.contents.Select((n) => { return n.spacial; }));
        }

        public List<ModelComponent> Query(BoundingBox box)
        {
            var r = new List<ModelComponent>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<ModelComponent> Query(BoundingSphere box)
        {
            var r = new List<ModelComponent>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<ModelComponent> Query(BoundingFrustum box)
        {
            var r = new List<ModelComponent>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }

        public List<ModelComponent> Query(Ray box)
        {
            var r = new List<ModelComponent>();
            OctTree.VisitTree(octTreeRoot, box, (cell) => queryAction(r, cell));
            return r;
        }
#endregion
    }
}
