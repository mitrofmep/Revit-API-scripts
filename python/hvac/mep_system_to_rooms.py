import clr
clr.AddReference('RevitAPI')  
clr.AddReference('RevitAPIUI')  
import Autodesk
from Autodesk.Revit.DB import *
from Autodesk.Revit.UI import *

doc = __revit__.ActiveUIDocument.Document

category_names = ["Механические оборудование", "Воздуховоды"]
categories = [FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).ToElements(),
              FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves).ToElements()]

system_name = "Система вентиляции"
air_flow_parameter_name = "Приток воздуха"

spaces = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).ToElements()

connector_points = []

for space in spaces:
    for category, category_name in zip(categories, category_names):
        family_symbol = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces).WhereElementIsElementType() \
            .FirstOrDefault(lambda fs: fs.FamilyName == category_name)
        if family_symbol is not None:
            connector = family_symbol.GetFamily().GetFamilySymbolIds().Select(lambda symbolId: doc.GetElement(symbolId)) \
                .FirstOrDefault().MEPModel.ConnectorManager.Connectors
            connector_points.extend([connector[i].Origin for i in range(connector.Size)])

with Transaction(doc, "Создание системы вентиляции") as trans:
    trans.Start()
    
    mechanical_system = MechanicalSystem.Create(doc, connector_points)

    mechanical_system.Name = system_name

    air_flow_param_id = mechanical_system.GetParameters(air_flow_parameter_name)[0].Id
    for duct in mechanical_system.DuctNetwork.GetDucts():
        duct.LookupParameter(air_flow_param_id).Set(1000)  

    trans.Commit()

TaskDialog.Show("Revit Python Script", "Система вентиляции успешно создана.")
