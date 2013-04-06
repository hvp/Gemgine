using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace OSM
{
    public static class GeographicMath
    {
        public const float radiusKm = 6371.0f;
        public const float diameterKm = 40075.16f;
        public const float kmDegree = 40075.16f / 360.0f;
        public const float mileDegree = (40075.16f / 1.609344f) / 360.0f;
        public const float degreeMile = 1.0f / 49.0f;

        public static float deg2rad(float angle)
        {
            return (float)(Math.PI) * angle / 180.0f;
        }

        public static float rad2deg(float angle)
        {
            return angle * 180.0f / (float)Math.PI;
        }

        public static float geo_to_vmile(float geo)
        {
            return geo * mileDegree;
        }

        public static float vmile_to_geo(float vmile)
        {
            return vmile / mileDegree;
        }

        public static Vector2 to_vmile(Vector2 geo)
        {
            return new Vector2(geo_to_vmile(geo.X), geo_to_vmile(geo.Y));
        }
        
        public static float geoDistance(Vector2 v1, Vector2 v2)
        {
            var dLat = deg2rad(v2.Y - v1.Y);
            var dLon = deg2rad(v2.X - v1.X);
            var lat1 = deg2rad(v1.Y);
            var lat2 = deg2rad(v2.Y);

            var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                    Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return (float)c * radiusKm;
        }

        public static float geoLength(List<Vector2> path)
        {
            float accum = 0.0f;
            if (path.Count <= 1) return accum;
            var first = path[0];
            for (int i = 1; i < path.Count; ++i)
            {
                accum += geoDistance(first, path[i]);
                first = path[i];
            }
            return accum;
        }
    }
}
