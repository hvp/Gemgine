﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public interface IModule
    {
        void BeginSimulation(Simulation sim);
        void EndSimulation();
        void AddComponents(List<Component> components);
        void RemoveEntities(List<UInt32> entities);
        void Update(float elapsedSeconds);
        void BindScript(MISP.Engine scriptEngine);
    }
}
