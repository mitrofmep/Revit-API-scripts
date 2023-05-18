using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitScript
{
    [Transaction(TransactionMode.Manual)]
    public class CreateViewScript : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            View activeView = doc.ActiveView;

            if (activeView.ViewType != ViewType.DrawingSheet)
            {
                TaskDialog.Show("Error", "Please select a sheet view.");
                return Result.Cancelled;
            }

            View newView = ViewDrafting.Create(doc, ElementId.InvalidElementId);

            newView.Name = "New View";

            Viewport viewport = Viewport.Create(doc, activeView.Id, newView.Id, new XYZ(0, 0, 0));

            viewport.GetPrimaryViewId().Scale = 50;

            return Result.Succeeded;
        }
    }
}
