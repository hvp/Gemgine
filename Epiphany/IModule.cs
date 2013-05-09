using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Epiphany
{
    public interface IModule
    {
        double UpdateInterval { get; }

        void BeginSimulation(Simulation sim);
        void EndSimulation();
        void AddComponents(List<Component> components);
        void RemoveEntities(List<UInt32> entities);
        void Update(GameTime time);
    }
}
