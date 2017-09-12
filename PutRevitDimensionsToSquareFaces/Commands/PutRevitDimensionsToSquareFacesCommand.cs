using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PutRevitDimensionsToSquareFaces.DimensionsCreators;
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

            var familyInstance = doc.GetElement(faceReference) as FamilyInstance;

            var dimensionsCreator = familyInstance != null
                ? (IDimensionCreator) new FamilyInstanceDimensionsCreator(familyInstance, squaredFace)
                : new BuiltInFamilyDimensionCreator(doc);

            using (var transaction = new Transaction(doc, "create dimensions"))
            {
                transaction.Start();

                dimensionsCreator.CreateDimension(new[] { outerEdges[0], outerEdges[2] }, dimensionOrigin);

                dimensionsCreator.CreateDimension(new[] { outerEdges[1], outerEdges[3] }, dimensionOrigin);
                
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
                .Single(x => IsTheSameReference(doc, faceReference, x));
        }

        private static bool IsTheSameReference(Document doc, Reference faceReference, Face face)
        {
            return faceReference.ConvertToStableRepresentation(doc).EndsWith(face.Reference.ConvertToStableRepresentation(doc));
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
    }
}
