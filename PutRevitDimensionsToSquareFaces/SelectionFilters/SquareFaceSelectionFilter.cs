using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace PutRevitDimensionsToSquareFaces.SelectionFilters
{
    public class SquareFaceSelectionFilter : ISelectionFilter
    {
        private readonly Document document;

        public SquareFaceSelectionFilter(Document document)
        {
            this.document = document;
        }

        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            var element = document.GetElement(reference);

            var face = element.GetGeometryObjectFromReference(reference) as PlanarFace;

            if (face == null)
                return false;

            if (!IsFaceDirectionParallelToViewDirection(face))
                return false;

            var outerLoop = face.GetEdgesAsCurveLoops()
                .Single(x => x.IsCounterclockwise(face.ComputeNormal(new UV())))
                .OfType<Line>()
                .ToList();

            if (outerLoop.Count != 4)
                return false;

            return AreLinesParallel(outerLoop[0], outerLoop[2])
                   && AreLinesParallel(outerLoop[1], outerLoop[3]);
        }

        private bool IsFaceDirectionParallelToViewDirection(Face face)
        {
            return face.ComputeNormal(new UV())
                .CrossProduct(document.ActiveView.ViewDirection)
                .IsAlmostEqualTo(XYZ.Zero);
        }

        private static bool AreLinesParallel(Line first, Line second)
        {
            return first.Direction.CrossProduct(second.Direction).IsAlmostEqualTo(XYZ.Zero);
        }
    }
}