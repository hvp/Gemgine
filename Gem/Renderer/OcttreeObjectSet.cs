using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Renderer
{
    public class OcttreeObjectSet
    {
        public OctTreeModule octTreeModule;

        List<uint> GetVisibleObjects(ICamera camera)
        {
            return new List<uint>(octTreeModule.Query(camera.GetFrustum()).Distinct());
        }
    }
}
