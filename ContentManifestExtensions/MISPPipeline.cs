#region File Description
//-----------------------------------------------------------------------------
// ManifestPipeline.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace ContentManifestExtensions
{
    // the importer is just a passthrough that gives the processor the filepath
    [ContentImporter(".msp", DisplayName = "MISP Script Importer", DefaultProcessor = "PassThroughProcessor")]
    public class MISPImporter : ContentImporter<string>
    {
        public override string Import(string filename, ContentImporterContext context)
        {
            // just give the processor the filename needed to do the processing
            return System.IO.File.ReadAllText(filename);
        }
    }

}
