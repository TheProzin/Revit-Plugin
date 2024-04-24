using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using TextBox = System.Windows.Controls.TextBox;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;

namespace FlPlugin
{
    public class FlElectric : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            PanelManager panelManager = new PanelManager();
            panelManager.CreatePanel(application);

            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Manual)]
    public class ElectricalTags : IExternalCommand
    {

        private UIApplication app;
        private UIDocument uidoc;
        private Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            app = commandData.Application;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            try
            {
                ICollection<ElementId> selectedElements = uidoc.Selection.GetElementIds();
                ElectricDevices electricDevices = new ElectricDevices();
                electricDevices.ProcessElements(doc, selectedElements);
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }   

            return Result.Succeeded;
        }

    }
}
