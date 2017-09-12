using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace PutRevitDimensionsToSquareFaces.DimensionsCreators
{
    public interface IDimensionCreator
    {
        void CreateDimension(IList<Edge> edges, XYZ dimensionOrigin);
    }
}