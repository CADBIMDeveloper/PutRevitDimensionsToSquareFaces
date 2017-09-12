using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PutRevitDimensionsToSquareFaces.Extensions;
using PutRevitDimensionsToSquareFaces.SelectionFilters;

namespace PutRevitDimensionsToSquareFaces.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class PutRevitDimensionsToSquareFacesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var faceReference = SelectSquareFace(uidoc);

            if (faceReference == null)
                return Result.Cancelled;

            var squaredFace = FindFace(doc, faceReference);

            var outerEdges = FindOuterEdgeLoop(squaredFace);

            var dimensionOrigin = FindFaceCenter(squaredFace);

            using (var transaction = new Transaction(doc, "create dimensions"))
            {
                transaction.Start();

                CreateLinearDimension(doc, new[] {outerEdges[0], outerEdges[2]}, dimensionOrigin);

                CreateLinearDimension(doc, new[] {outerEdges[1], outerEdges[3]}, dimensionOrigin);

                transaction.Commit();
            }
            

            return Result.Succeeded;
        }

        private static PlanarFace FindFace(Document doc, Reference faceReference)
        {
            return doc.GetElement(faceReference)
                .GetSolids()
                .SelectMany(x => x.Faces.Cast<Face>())
                .OfType<PlanarFace>()
                .Single(x => x.Reference.ConvertToStableRepresentation(doc) == faceReference.ConvertToStableRepresentation(doc));
        }

        private static Reference SelectSquareFace(UIDocument uiDocument)
        {
            var doc = uiDocument.Document;

            try
            {
                return uiDocument.Selection
                    .PickObject(ObjectType.Face, new SquareFaceSelectionFilter(doc));
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private static XYZ FindFaceCenter(Face face)
        {
            var envelope = face.GetBoundingBox();

            var center = 0.5*(envelope.Min + envelope.Max);

            return face.Evaluate(center);
        }

        private static IList<Edge> FindOuterEdgeLoop(Face face)
        {
            return face
                .EdgeLoops
                .Cast<EdgeArray>()
                .Single(x => IsCounterClocwize(face, x))
                .Cast<Edge>()
                .ToList();
        }

        private static bool IsCounterClocwize(Face face, EdgeArray edgeArray)
        {
            var curveLoop = CurveLoop.Create(edgeArray.Cast<Edge>().Select(x => x.AsCurve()).ToList());

            return curveLoop.IsCounterclockwise(face.ComputeNormal(new UV()));
        }

        private static void CreateLinearDimension(Document doc, IList<Edge> edges, XYZ origin)
        {
            var view = doc.ActiveView;

            var firstEdgeLine = (Line)edges[0].AsCurve();

            var dimensionLineDirection = firstEdgeLine.Direction.CrossProduct(view.ViewDirection);

            var dimensionLine = Line.CreateUnbound(origin, dimensionLineDirection);

            var referenceArray = new ReferenceArray();

            foreach (var edge in edges)
                referenceArray.Append(edge.Reference);

            doc.Create.NewDimension(view, dimensionLine, referenceArray);
        }
    }
}
