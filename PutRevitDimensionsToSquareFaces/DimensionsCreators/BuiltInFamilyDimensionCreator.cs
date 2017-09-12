using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace PutRevitDimensionsToSquareFaces.DimensionsCreators
{
    public class BuiltInFamilyDimensionCreator : IDimensionCreator
    {
        private readonly Document document;

        public BuiltInFamilyDimensionCreator(Document document)
        {
            this.document = document;
        }

        public void CreateDimension(IList<Edge> edges, XYZ dimensionOrigin)
        {
            var view = document.ActiveView;

            var firstEdgeLine = (Line)edges[0].AsCurve();

            var dimensionLineDirection = firstEdgeLine.Direction.CrossProduct(view.ViewDirection);

            var dimensionLine = Line.CreateUnbound(dimensionOrigin, dimensionLineDirection);

            var referenceArray = new ReferenceArray();

            foreach (var edge in edges)
                referenceArray.Append(edge.Reference);

            document.Create.NewDimension(view, dimensionLine, referenceArray);
        }
    }
}