using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Renderer
{
    interface ObjectSet
    {
        List<uint> GetVisibleObjects(ICamera camera);
    }
}
