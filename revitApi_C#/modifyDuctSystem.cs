using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;

public void ModifyDuctSystem(Document doc, Duct duct)
{
    DuctSystem ductSystem = duct.MechanicalSystem as DuctSystem;
    
    IList<ElementId> fittingIds = ductSystem.GetFittingIds();
    
    foreach (ElementId fittingId in fittingIds)
    {
        FamilyInstance fitting = doc.GetElement(fittingId) as FamilyInstance;
        
        // Modify the fitting properties (example: change width parameter)
        Parameter widthParam = fitting.LookupParameter("Width");
        if (widthParam != null && widthParam.IsReadOnly == false)
        {
            widthParam.Set(12.0); // Set the width to 12.0 (modify according to your needs)
        }
        doc.Regenerate();
    }
}
