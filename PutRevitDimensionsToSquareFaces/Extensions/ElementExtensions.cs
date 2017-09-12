using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace PutRevitDimensionsToSquareFaces.Extensions
{
    public static class ElementExtensions
    {
        public static IEnumerable<Solid> GetSolids(this Element element, bool excludeEmpty = true)
        {
            var geometry = element
                .get_Geometry(new Options {ComputeReferences = true});
            if (geometry == null)
                return Enumerable.Empty<Solid>();

            return GetSolids(geometry)
                .Where(x => !excludeEmpty || x.Volume.IsAlmostEqualToOrMoreThan(0));
        }

        private static IEnumerable<Solid> GetSolids(IEnumerable<GeometryObject> geometryElement)
        {
            foreach (var geometry in geometryElement)
            {
                var solid = geometry as Solid;
                if (solid != null)
                    yield return solid;

                var instance = geometry as GeometryInstance;
                if (instance != null)
                    foreach (var instanceSolid in GetSolids(instance.GetInstanceGeometry()))
                        yield return instanceSolid;

                var element = geometry as GeometryElement;
                if (element != null)
                    foreach (var elementSolid in GetSolids(element))
                        yield return elementSolid;
            }
        }
    }
}