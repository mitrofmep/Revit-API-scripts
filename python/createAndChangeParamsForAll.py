import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')

from Autodesk.Revit.DB import FilteredElementCollector, BuiltInCategory, Transaction

clr.AddReference('RevitServices')
import RevitServices
from RevitServices.Persistence import DocumentManager

doc = DocumentManager.Instance.CurrentDBDocument

def create_family_parameter(family, name, parameter_type):
    symbol = family.GetFamilySymbolIds().FirstOrDefault()
    if symbol:
        symbol = doc.GetElement(symbol)

        parameter = symbol.LookupParameter(name)
        if not parameter:
            with Transaction(doc, "Create Family Parameter") as t:
                t.Start()
                parameter = symbol.AddParameter(name, parameter_type, False)
                t.Commit()

        return parameter

collector = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType()
elements = list(collector)

parameters = []
for element in elements:
    parameter_value = element.LookupParameter("Height").AsDouble()
    parameters.append(parameter_value)

with Transaction(doc, "Modify Parameter Values") as t:
    t.Start()
    for element, parameter_value in zip(elements, parameters):
        element.LookupParameter("Height").Set(parameter_value * 2)
    t.Commit()

updated_parameters = []
for element in elements:
    parameter_value = element.LookupParameter("Height").AsDouble()
    updated_parameters.append(parameter_value)

OUT = updated_parameters
