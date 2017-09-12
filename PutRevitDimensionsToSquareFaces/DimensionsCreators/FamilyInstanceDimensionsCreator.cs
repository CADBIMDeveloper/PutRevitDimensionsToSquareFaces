using System.Collections.Generic;
using Autodesk.Revit.DB;
using PutRevitDimensionsToSquareFaces.Extensions;

namespace PutRevitDimensionsToSquareFaces.DimensionsCreators
{
    public class FamilyInstanceDimensionsCreator : IDimensionCreator
    {
        private readonly Face squaredFace;
        private readonly FamilyInstance familyInstance;

        public FamilyInstanceDimensionsCreator(FamilyInstance familyInstance, Face squaredFace)
        {
            this.squaredFace = squaredFace;
            this.familyInstance = familyInstance;
        }

        public void CreateDimension(IList<Edge> edges, XYZ dimensionOrigin)
        {
            var document = familyInstance.Document;

            var view = document.ActiveView;

            var firstEdgeLine = (Line)edges[0].AsCurve();

            var dimensionLineDirection = firstEdgeLine.Direction.CrossProduct(view.ViewDirection);

            var dimensionLine = Line.CreateUnbound(dimensionOrigin, dimensionLineDirection);

            var referenceArray = new ReferenceArray();

            foreach (var edge in edges)
                referenceArray.Append(FindFaceReference(edge));

            document.Create.NewDimension(view, dimensionLine, referenceArray);
        }

        private Reference FindFaceReference(Edge edge)
        {
            var document = familyInstance.Document;
            var referencedFace = edge.GetOtherFace(squaredFace);

            var reference = $"{familyInstance.UniqueId}:0:INSTANCE:" + referencedFace.Reference.ConvertToStableRepresentation(document);

            return Reference.ParseFromStableRepresentation(document, reference);
        }
    }
}