using Autodesk.Revit.DB;

namespace PutRevitDimensionsToSquareFaces.Extensions
{
    public static class EdgeExtensions
    {
        public static Face GetOtherFace(this Edge edge, Face face)
        {
            return edge.GetFace(0) != face
                ? edge.GetFace(0)
                : edge.GetFace(1);
        }
    }
}