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
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("FL Plugin");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData("cmdHelloWorld",
                               "Electrical Tags", thisAssemblyPath, "FlPlugin.ElectricalTags");

            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Automatic insert tags for eletrical devices.";
            pushButton.LongDescription = "Automatic insert tags for eletrical devices.";
            
            Uri uriImage = new Uri("C:\\Users\\telle\\OneDrive\\Documents\\Projetos\\FlPlugin\\FlPlugin\\Resources\\hello.png");
            pushButton.LargeImage = new BitmapImage(uriImage);

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
                ElectricDevices electricDevices = new ElectricDevices();
                foreach (ElementId selectedId in uidoc.Selection.GetElementIds())
                {
                    Element selectedElement = doc.GetElement(selectedId);
                    FamilyInstance familyInstance = selectedElement as FamilyInstance;

                    electricDevices.ProcessGroupCategory(doc, familyInstance);

                }
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
