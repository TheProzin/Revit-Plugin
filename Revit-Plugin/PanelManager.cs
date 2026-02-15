using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FlPlugin
{
    internal class PanelManager
    {
        public void CreatePanel(UIControlledApplication app)
        {
            //Cria um painel na aba de Add-ins
            RibbonPanel ribbonPanel = app.CreateRibbonPanel("FL Plugin");
            CreateEletricalTagsButton(ribbonPanel);
        }

        public void CreateEletricalTagsButton(RibbonPanel ribbonPanel)
        {
            //Cria o botão para inserir tags (Electrical Tags)
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData("cmdHelloWorld",
                               "Electrical Tags", thisAssemblyPath, "FlPlugin.ElectricalTags");

            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Automatic insert tags for eletrical devices.";
            pushButton.LongDescription = "Automatic insert tags for eletrical devices.";

            Uri uriImage = new Uri("C:\\Users\\telle\\OneDrive\\Documents\\Projetos\\FlPlugin\\FlPlugin\\Resources\\hello.png");
            pushButton.LargeImage = new BitmapImage(uriImage);

        }

    }
}
