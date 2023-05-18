using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitScript
{
    [Transaction(TransactionMode.Manual)]
    public class ModifyElementParametersScript : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Selection sel = uiDoc.Selection;

            IList<Reference> selectedRefs = sel.PickObjects(ObjectType.Element, "Select elements");

            using (Transaction trans = new Transaction(doc, "Modify Element Parameters"))
            {
                if (trans.Start() == TransactionStatus.Started)
                {
                    foreach (Reference reference in selectedRefs)
                    {
                        Element element = doc.GetElement(reference.ElementId);

                        Parameter parameter = element.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                        if (parameter != null && parameter.IsReadOnly == false)
                        {
                            Level level = doc.GetElement(new ElementId(12345)) as Level;
                            if (level != null)
                            {
                                parameter.Set(level.Id);
                            }
                        }
                    }

                    trans.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
