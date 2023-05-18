using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitScript
{
    [Transaction(TransactionMode.Manual)]
    public class ModifyFamilyParametersScript : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Reference selectedElementRef = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element);
            Element selectedElement = doc.GetElement(selectedElementRef.ElementId);

            if (selectedElement is FamilyInstance familyInstance)
            {
                FamilySymbol familySymbol = familyInstance.Symbol;

                FamilyParameterSet familyParameters = familySymbol.Parameters;

                foreach (FamilyParameter parameter in familyParameters)
                {
                    // Change param here
                    if (parameter.Definition.Name == "Height")
                    {
                        using (Transaction trans = new Transaction(doc, "Modify Parameter"))
                        {
                            if (trans.Start() == TransactionStatus.Started)
                            {
                                familyInstance.get_Parameter(parameter.Id).Set(300.0);
                                trans.Commit();
                            }
                        }
                    }
                }
            }
            else
            {
                TaskDialog.Show("Error", "Please select a family instance.");
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
