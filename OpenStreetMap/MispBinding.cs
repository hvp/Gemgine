using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MISP;

namespace OSM
{
    public class Binding : ILibraryInterface
    {
        public bool BindLibrary(MISP.Engine engine)
        {
            var database = new DatabaseService();

            engine.AddFunction("osm-query-id", "Query an entry from the OSM database.",
                (context, arguments) =>
                {
                    var r = database.Query(Convert.ToInt64(arguments[0]));
                    database.CommitChanges();
                    return r;
                }, Arguments.Arg("id"));

            engine.AddFunction("osm-query-name", "Query an entry from the OSM database.",
                (context, arguments) =>
                {
                    var r = database.Query(Convert.ToString(arguments[0]));
                    database.CommitChanges();
                    return r;
                }, Arguments.Arg("name"));

            var geoMath = AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(GeographicMath));
            engine.AddGlobalVariable("geo", c => geoMath);

            return true;
        }
    }
}
