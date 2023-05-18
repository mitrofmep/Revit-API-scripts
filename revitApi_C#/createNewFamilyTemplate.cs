using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitScript
{
    [Transaction(TransactionMode.Manual)]
    public class CreateFamilyWithParametersScript : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Family family = Family.Create(doc, "MyFamily");

            FamilyManager familyManager = family.FamilyManager;

            FamilyParameterType textType = FamilyParameterType.Text;
            FamilyParameter textParameter = familyManager.AddParameter("Text Parameter", textType, true);

            FamilyParameterType numberType = FamilyParameterType.Length;
            FamilyParameter numberParameter = familyManager.AddParameter("Number Parameter", numberType, true);

            FamilyParameterType yesNoType = FamilyParameterType.YesNo;
            FamilyParameter yesNoParameter = familyManager.AddParameter("Yes/No Parameter", yesNoType, true);

            familyManager.Set(textParameter, "Hello World");
            familyManager.Set(numberParameter, 10.0);
            familyManager.Set(yesNoParameter, true);

            familyManager.AssociateElementParameterToFamilyParameter(textParameter, textParameter);
            familyManager.AssociateElementParameterToFamilyParameter(numberParameter, numberParameter);
            familyManager.AssociateElementParameterToFamilyParameter(yesNoParameter, yesNoParameter);
            doc.Regenerate();

            TaskDialog dialog = new TaskDialog("Family Parameters");
            dialog.MainContent = "Text Parameter: " + familyManager.get_Parameter(textParameter.GUID).AsString() + "\n" +
                                 "Number Parameter: " + familyManager.get_Parameter(numberParameter.GUID).AsValueString() + "\n" +
                                 "Yes/No Parameter: " + familyManager.get_Parameter(yesNoParameter.GUID).AsValueString();
            dialog.Show();

            return Result.Succeeded;
        }
    }
}
