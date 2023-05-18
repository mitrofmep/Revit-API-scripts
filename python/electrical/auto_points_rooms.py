import clr
clr.AddReference('RevitAPI') 
clr.AddReference('RevitAPIUI') 
import Autodesk
from Autodesk.Revit.DB import *
from Autodesk.Revit.UI import *

doc = __revit__.ActiveUIDocument.Document

outlet_family_name = "Розетка"
outlet_symbol_name = "Розетка 1"
outlet_power_parameter_name = "Мощность"

spaces = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).ToElements()

spaces_to_equip = [space for space in spaces if space.LookupParameter("Установка розеток").AsInteger() == 1]

outlet_points = []

for space in spaces_to_equip:
    room_bounding_box = space.get_BoundingBox(None)
    if room_bounding_box is not None:
        room_min_point = room_bounding_box.Min
        room_max_point = room_bounding_box.Max
        outlet_point = XYZ((room_min_point.X + room_max_point.X) / 2,
                           (room_min_point.Y + room_max_point.Y) / 2,
                           room_min_point.Z)
        outlet_points.append(outlet_point)

with Transaction(doc, "Установка розеток") as trans:
    trans.Start()

    outlet_family = FilteredElementCollector(doc).OfClass(Family).FirstOrDefault(lambda f: f.Name == outlet_family_name)

    if outlet_family is not None:
        outlet_symbol = FilteredElementCollector(doc).OfCategoryId(outlet_family.Id).OfCategory(BuiltInCategory.OST_ElectricalFixtures) \
            .FirstOrDefault(lambda s: s.Name == outlet_symbol_name)

        if outlet_symbol is not None:
            for outlet_point in outlet_points:
                new_outlet = doc.Create.NewFamilyInstance(outlet_point, outlet_symbol, space, Structure.StructuralType.NonStructural)

                power_param_id = new_outlet.LookupParameter(outlet_power_parameter_name).Id
                new_outlet.LookupParameter(power_param_id).Set(220)
    trans.Commit()

TaskDialog.Show("Revit Python Script
