using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContentManifestExtensions
{
    public enum ContentType
    {
        Unknown,
        Texture,
        Map,
        XML,
        Conversation,
        ParticleEffect,
        Model,
        Animation,
    }

    public class Entry
    {
        public ContentType Type { get; set; }
        public String Name { get; set; }
    }

    public class Manifest
    {
        public List<Entry> Entries = new List<Entry>();

    }
}
