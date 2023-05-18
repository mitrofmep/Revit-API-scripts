import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')

from Autodesk.Revit.DB import FilteredElementCollector, ViewFamilyType, ViewType, ViewSet, ViewSetIterator
from pyrevit import revit, DB, forms

doc = revit.doc

def create_view_sheet():
    view_sheet_type = FilteredElementCollector(doc).OfClass(ViewFamilyType).WhereElementIsElementType().FirstElement()
    
    with DB.Transaction(doc, 'Create View Sheet') as t:
        t.Start()
        view_sheet = DB.ViewSheet.Create(doc, view_sheet_type.Id)
        t.Commit()
    
    return view_sheet

def add_view_to_sheet(view_sheet, view):
    with DB.Transaction(doc, 'Add View to Sheet') as t:
        t.Start()
        view_sheet.AddView(view)
        t.Commit()

view_collector = FilteredElementCollector(doc).OfClass(DB.View).WhereElementIsNotElementType()
available_views = list(filter(lambda v: v.ViewType == DB.ViewType.FloorPlan, view_collector))

new_view_sheet = create_view_sheet()

for view in available_views:
    add_view_to_sheet(new_view_sheet, view)

with DB.Transaction(doc, 'Set View Visibility') as t:
    t.Start()
    view_set = new_view_sheet.GetAllPlacedViews()
    view_set_iter = view_set.ForwardIterator()
    
    while view_set_iter.MoveNext():
        view = view_set_iter.Current
        view.IsHidden(ViewSet.FindByName(doc, 'View Filters'), False)
    
    t.Commit()

filename = forms.save_file(file_ext='rvt')
if filename:
    options = DB.SaveAsOptions()
    options.Compact = True
    options.OverwriteExistingFile = True
    doc.SaveAs(filename, options)

OUT = new_view_sheet
