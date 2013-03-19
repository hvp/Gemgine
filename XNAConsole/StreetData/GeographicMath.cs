using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XNAConsole.StreetData
{
    public static class GeographicMath
    {
        public const double radiusKm = 6371.0;
        public const double diameterKm = 40075.16;
        public const double kmDegree = 40075.16 / 360.0;
        public const double mileDegree = (40075.16 / 1.609344) / 360.0;
        public const double degreeMile = 1.0 / 49.0;

        public static double deg2rad(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double rad2deg(double angle)
        {
            return angle * 180.0 / Math.PI;
        }

        public static double m2geo(double m)
        {
            return m / 49.0;
        }

        public static double lon2vmile(double lon)
        {
            return lon * mileDegree;
        }

        public static double vmile2lon(double vmile)
        {
            return vmile / mileDegree;
        }

        public static double lat2vmile(double lat)
        {
            return lat * mileDegree;
        }

        public static double vmile2lat(double vmile)
        {
            return -(vmile / mileDegree);
        }

        public static double k2m(double k)
        {
            return k / 1.609344;
        }

        public static double m2k(double m)
        {
            return m * 1.609344;
        }

        public static double geoDistance(double lon1, double lat1, double lon2, double lat2)
        {
            var dLat = deg2rad(lat2 - lat1);
            var dLon = deg2rad(lon2 - lon1);
            lat1 = deg2rad(lat1);
            lat2 = deg2rad(lat2);

            var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                    Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return c * radiusKm;
        }

        public static double geoDistance(PointF a, PointF b)
        {
            return geoDistance(a.X, a.Y, b.X, b.Y);
        }

        public static double distance(PointF a, PointF b)
        {
            var delta = new PointF(b.X - a.X, b.Y - a.Y);
            return Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }

        public static double length(List<PointF> path)
        {
            double accum = 0.0;
            if (path.Count <= 1) return accum;
            var first = path[0];
            for (int i = 1; i < path.Count; ++i)
            {
                accum += distance(first, path[i]);
                first = path[i];
            }
            return accum;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dist"></param>
        /// <returns>Tuple containing the point and the tangent at that point</returns>
        public static Tuple<PointF, PointF> pointOnPath(List<PointF> path, double dist)
        {
            if (path.Count == 0) return new Tuple<PointF, PointF>(new PointF(0, 0), new PointF(0, 0));
            if (path.Count == 1) return new Tuple<PointF, PointF>(path[0], new PointF(0, 0));
            double d = 0.0;
            int i = 1;

            PointF segmentStart = new PointF(0, 0);
            PointF segmentEnd = new PointF(0, 0);
            double segmentLength = 1.0;
            double excess = 0.0;

            while (dist > d)
            {
                segmentLength = distance(path[i - 1], path[i]);
                if (dist < d + segmentLength || i == path.Count - 1)
                {
                    excess = dist - d;
                    segmentStart = path[i - 1];
                    segmentEnd = path[i];
                    break;
                }

                d += segmentLength;
                ++i;
            }

            var vec = new PointF(segmentEnd.X - segmentStart.X, segmentEnd.Y - segmentStart.Y);
            vec.X /= (float)segmentLength;
            vec.Y /= (float)segmentLength;
            return new Tuple<PointF, PointF>(
                new PointF((float)(segmentStart.X + vec.X * excess), (float)(segmentStart.Y + vec.Y * excess)),
                vec);
        }
    }
}
