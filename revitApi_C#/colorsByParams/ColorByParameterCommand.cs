using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace RevitAPI_ColorByParameter
{
    [Transaction(TransactionMode.Manual)]
    public class ColorByParameterCommand : IExternalCommand
    {
        // Define the category filter dictionary
//         private Dictionary<string, bool> categoryFilterDict = new Dictionary<string, bool>();

        // Define the parameter name and color dictionary
//         private Dictionary<string, Color> parameterColorDict = new Dictionary<string, Color>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Get the Revit UI document and application
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                UIApplication uiApp = commandData.Application;

                // Create the color palette
                List<Color> colorPalette = new List<Color>
                {
                    Colors.Red,
                    Colors.Blue,
                    Colors.Green,
                    Colors.Yellow,
                    Colors.Orange
                    // Add more colors to the palette if needed
                };

                // Get the list of all categories in the document
                List<Category> categories = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .Select(element => element.Category)
                    .Distinct()
                    .ToList();

                // Initialize the category filter dictionary
                foreach (Category category in categories)
                {
                    categoryFilterDict[category.Name] = true;
                }

                // Create the form and display it
                ColorByParameterForm form = new ColorByParameterForm(categories, categoryFilterDict, parameterColorDict, colorPalette);
                form.ShowDialog();

                // Get the selected categories and parameters
                List<Category> selectedCategories = categories
                    .Where(category => categoryFilterDict[category.Name])
                    .ToList();
                List<string> selectedParameters = parameterColorDict.Keys.ToList();

                // Create filters for the selected categories and parameters
                List<ElementId> filterIds = new List<ElementId>();
                using (Transaction transaction = new Transaction(doc, "Create Filters"))
                {
                    transaction.Start();

                    foreach (Category category in selectedCategories)
                    {
                        foreach (string parameterName in selectedParameters)
                        {
                            string filterName = $"F_{parameterName}_{category.Name}";

                            // Check if the filter already exists
                            ElementId existingFilterId = FilterHelper.GetFilterIdByName(doc, filterName);
                            if (existingFilterId != ElementId.InvalidElementId)
                            {
                                filterIds.Add(existingFilterId);
                            }
                            else
                            {
                                ParameterFilterElement filter = FilterHelper.CreateCategoryParameterFilter(doc, category, parameterName, filterName);
                                filterIds.Add(filter.Id);
                            }
                        }
                    }

                    transaction.Commit();
                }

                // Apply filters to the active view
                View activeView = uiDoc.ActiveView;
                using (Transaction transaction = new Transaction(doc, "Apply Filters"))
                {
                    transaction.Start();

                    activeView.SetFilterOverrides(filterIds);

                    transaction.Commit();
                }

                // Subscribe to the DocumentChanged event to handle updates to colors
                uiApp.ViewActivated += HandleViewActivatedEvent;
                uiApp.DocumentChanged += HandleDocumentChangedEvent;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void HandleViewActivatedEvent(object sender, ViewActivatedEventArgs args)
        {
            // Refresh the color overrides when the view is activated
            UIApplication uiApp = sender as UIApplication;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View activeView = args.CurrentActiveView;

            // Get the selected categories and parameters
            List<Category> selectedCategories = doc.Settings.Categories
                .Cast<Category>()
                .Where(category => categoryFilterDict[category.Name])
                .ToList();
            List<string> selectedParameters = parameterColorDict.Keys.ToList();

            // Create filters for the selected categories and parameters
            List<ElementId> filterIds = new List<ElementId>();

            foreach (Category category in selectedCategories)
            {
                foreach (string parameterName in selectedParameters)
                {
                    string filterName = $"F_{parameterName}_{category.Name}";

                    // Check if the filter already exists
                    ElementId existingFilterId = FilterHelper.GetFilterIdByName(doc, filterName);
                    if (existingFilterId != ElementId.InvalidElementId)
                    {
                        filterIds.Add(existingFilterId);
                    }
                    else
                    {
                        ParameterFilterElement filter = FilterHelper.CreateCategoryParameterFilter(doc, category, parameterName, filterName);
                        filterIds.Add(filter.Id);
                    }
                }
            }

            // Apply filters to the active view
            using (Transaction transaction = new Transaction(doc, "Apply Filters"))
            {
                transaction.Start();

                activeView.SetFilterOverrides(filterIds);

                transaction.Commit();
            }
        }

        private void HandleDocumentChangedEvent(object sender, DocumentChangedEventArgs args)
        {
            // Handle the color updates when the document is changed
            UIApplication uiApp = sender as UIApplication;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View activeView = uiDoc.ActiveView;

            // Get the selected categories and parameters
            List<Category> selectedCategories = doc.Settings.Categories
                .Cast<Category>()
                .Where(category => categoryFilterDict[category.Name])
                .ToList();
            List<string> selectedParameters = parameterColorDict.Keys.ToList();

            // Get the filter overrides from the active view
            ICollection<ElementId> filterOverrideIds = activeView.GetFilterOverrides();

            using (Transaction transaction = new Transaction(doc, "Update Colors"))
            {
                transaction.Start();

                foreach (ElementId filterId in filterOverrideIds)
                {
                    ParameterFilterElement filter = doc.GetElement(filterId) as ParameterFilterElement;
                    FilterHelper.UpdateFilterColors(doc, filter, parameterColorDict);
                }

                transaction.Commit();
            }
        }
    }

//     public static class FilterHelper
//     {
//         public static ElementId GetFilterIdByName(Document doc, string filterName)
//         {
//             ElementId filterId = ElementId.InvalidElementId;

//             FilteredElementCollector collector = new FilteredElementCollector(doc);
//             collector.OfClass(typeof(ParameterFilterElement));
//             IEnumerable<ParameterFilterElement> filters = collector.Cast<ParameterFilterElement>();

//             foreach (ParameterFilterElement filter in filters)
//             {
//                 if (filter.Name == filterName)
//                 {
//                     filterId = filter.Id;
//                     break;
//                 }
//             }

//             return filterId;
//         }

//         public static ParameterFilterElement CreateCategoryParameterFilter(Document doc, Category category, string parameterName, string filterName)
//         {
//             // Create a new filter rule to match the category and parameter
//             ElementId categoryId = category.Id;
//             ElementId parameterId = null;

//             // Get the parameter definition
//             FilterableValueProvider provider = new ParameterValueProvider(new ElementId(BuiltInParameter.INVALID));
//             FilterRule rule = new FilterStringRule(provider, new FilterStringEquals(), parameterName, false);
//             parameterId = new ElementId(rule.RuleFieldId);

//             // Create the filter element
//             ParameterFilterElement filter = ParameterFilterElement.Create(doc, filterName, new List<ElementId> { categoryId, parameterId }, new List<FilterRule> { rule });

//             return filter;
//         }

//         public static void UpdateFilterColors(Document doc, ParameterFilterElement filter, Dictionary<string, Color> parameterColorDict)
//         {
//             IList<ElementId> filterRuleIds = filter.GetRules();
//             ColorOverrides colorOverrides = filter.GetColorOverrides();
//             foreach (ElementId ruleId in filterRuleIds)
//             {
//                 FilterRule rule = filter.GetRule(ruleId);

//                 ElementId parameterId = new ElementId(rule.RuleFieldId);
//                 Parameter parameter = doc.GetElement(parameterId) as Parameter;
//                 string parameterName = parameter.Definition.Name;

//                 if (parameterColorDict.TryGetValue(parameterName, out Color color))
//                 {
//                     colorOverrides.SetColor(ruleId, color);
//                 }
//             }
//             filter.SetColorOverrides(colorOverrides);
//         }
//     }

//     public class ColorByParameterForm : System.Windows.Forms.Form
//     {
//         public ColorByParameterForm(List<Category> categories, Dictionary<string, bool> categoryFilterDict, Dictionary<string, Color> parameterColorDict, List<Color> colorPalette)
//         {
//         }
//     }
}
