import clr
clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

# Function to get all the duct elements in the active document
def get_ducts():
    ducts = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsNotElementType().ToElements()
    return ducts

# Function to get the length of a duct element
def get_duct_length(duct):
    return duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble()

# Function to get the width of a rectangular duct element
def get_duct_width(duct):
    width_param = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)
    if width_param:
        return width_param.AsDouble()
    else:
        return None

# Function to set the width of a rectangular duct element
def set_duct_width(duct, width):
    width_param = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)
    if width_param:
        width_param.Set(width)

# Example usage
doc = __revit__.ActiveUIDocument.Document

# Get all the ducts in the active document
ducts = get_ducts()

# Print the length and width of each duct
for duct in ducts:
    length = get_duct_length(duct)
    width = get_duct_width(duct)
    print(f"Duct ID: {duct.Id}, Length: {length}, Width: {width}")

# Set the width of a specific duct
if ducts:
    # Let's set the width of the first duct to 0.5 meters
    set_duct_width(ducts[0], 0.5)
    print("Duct width set successfully.")
else:
    print("No ducts found.")
