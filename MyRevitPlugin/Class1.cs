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

namespace MyRevitPlugin
{
    public class Class1 : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("My Plugin");
            //Create a push button

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData("cmdHelloWorld",
                               "Hello World", thisAssemblyPath, "MyRevitPlugin.MyTest");

            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Say hello to the world.";
            pushButton.LongDescription = "This is a long description for the Hello World command.";
            
            Uri uriImage = new Uri("C:\\Users\\telle\\OneDrive\\Documents\\Projetos\\MyRevitPlugin\\MyRevitPlugin\\Resources\\hello.png");
            pushButton.LargeImage = new BitmapImage(uriImage);

            return Result.Succeeded;

        }

    }

    [Transaction(TransactionMode.Manual)]
    public class MyTest : IExternalCommand
    {
        struct tagPosition
        {
            public double x;
            public double y;
            public double z;
        }

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
                foreach (ElementId selectedId in uidoc.Selection.GetElementIds())
                {
                    Element selectedElement = doc.GetElement(selectedId);
                    FamilyInstance familyInstance = selectedElement as FamilyInstance;

                    if (selectedElement.Category.Name == "Lighting Devices")
                    {
                        var i = 0;
                        tagPosition tagPosition = new tagPosition
                        {
                            x = 0.45,
                            y = 0.25,
                            z = 1.0
                        };

                        foreach (var subComponentId in familyInstance.GetSubComponentIds())
                        {
                            var subComponent = doc.GetElement(subComponentId);

                            if (subComponent.Name.Contains("Módulo de Tecla Interrupor não listavel"))
                            {

                                ProcessSelectedElement(subComponent, tagPosition);

                                bool isEven = i % 2 == 0;
                                if (!isEven)
                                {
                                    tagPosition.x = 0.45;
                                    tagPosition.y += 0.45;
                                }
                                else
                                {
                                    tagPosition.x += 0.7;
                                }
                                i++;
                            }
                        }

                        if (i == 0)
                        {
                            ProcessSelectedElement(selectedElement, tagPosition);
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Erro", "O elemento selecionado não é um dispositivo de iluminação.");
                    }

                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }   

            return Result.Succeeded;
        }

        private void ProcessSelectedElement(Element selectedElement, tagPosition tagPosition)
        {
            string switchIDValue = GetNextSwitchId();
            if (string.IsNullOrEmpty(switchIDValue))
            {
                TaskDialog.Show("Erro", "Não foi possível obter o próximo Switch ID.");
                return;
            }

            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;

            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            InsertSwitchId(selectedElement, switchIDValue);
            CreateTag(selectedElement, tagLocation);
        }

        private void CreateTag(Element selectedElement, XYZ tagPosition)
        {
            FamilySymbol tagSymbol = GetTagSymbol(847296);

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Criar Tag");
                IndependentTag tag = IndependentTag.Create(doc, doc.ActiveView.Id, new Reference(selectedElement), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagPosition);
                tag.ChangeTypeId(tagSymbol.Id);
                transaction.Commit();
            }
        }

        private void InsertSwitchId (Element selectedElement, string switchIDValue)
        {

            if (selectedElement.Category.Id.Value == (int)BuiltInCategory.OST_LightingDevices)
            {
                Parameter switchIDParameter = selectedElement.LookupParameter("Switch ID");

                //verficar se a propriedade Switch ID está disponível e não é somente leitura e nao esta preechida
                if (switchIDParameter != null && !switchIDParameter.IsReadOnly)
                {
                    string currentSwitchID = switchIDParameter.AsString();
                    if (string.IsNullOrEmpty(currentSwitchID))
                    {
                        using (Transaction transaction = new Transaction(doc))
                        {
                            transaction.Start("Definir Switch ID");
                            switchIDParameter.Set(switchIDValue);
                            transaction.Commit();
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Informação", "O Switch ID já possui um valor salvo. Nenhuma alteração necessária.");
                    }
                }
                else
                {
                    TaskDialog.Show("Erro", "A propriedade Switch ID não está disponível ou é somente leitura.");
                }
            }
            else
            {
                TaskDialog.Show("Erro", "O elemento selecionado não é um interruptor.");
            }
        }

        private FamilySymbol GetTagSymbol(int tagId)
        {
            // Obtém todos os tipos de família de tag disponíveis no documento
            FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_LightingDeviceTags)
                .WhereElementIsElementType();

            // Filtra os tipos de família de tag para obter o desejado
            FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>()
                .FirstOrDefault(x => x.Id.Value == tagId);

            return tagSymbol;
        }

        private string GetNextSwitchId()
        {
            SwitchIdManager manager = new SwitchIdManager();
            return manager.GetNextSwitchId(doc);
        }

    }

    public class SwitchIdManager
    {
        public string GetNextSwitchId(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_LightingDevices)
                .WhereElementIsNotElementType();

            var switchIds = collector.SelectMany(instance =>
            {
                Parameter switchIdParam = instance.LookupParameter("Switch ID");
                if (switchIdParam != null)
                {
                    string switchId = switchIdParam.AsString();
                    if (!string.IsNullOrEmpty(switchId))
                        return new[] { switchId };
                }
                return Enumerable.Empty<string>();
            }).ToList();

            switchIds.Sort();

            if (switchIds.Count == 0)
                return "a";

            string lastSwitchId = switchIds.Last();

            string nextSwitchId = IncrementSwitchId(lastSwitchId);

            return nextSwitchId;
        }

        private string IncrementSwitchId(string switchId)
        {
            char lastChar = switchId.Last();

            // Se o último caractere for 'z', verifica o penúltimo caractere
            if (lastChar == 'z')
            {
                // Se o penúltimo caractere for 'z', incrementa-o para 'a' e retorna
                if (switchId.Length > 1 && switchId[switchId.Length - 2] == 'z')
                {
                    return IncrementSwitchId(switchId.Substring(0, switchId.Length - 1)) + 'a';
                }
                // Se o penúltimo caractere for uma letra de 'a' a 'y', incrementa-o e retorna
                else if (switchId.Length > 1 && switchId[switchId.Length - 2] >= 'a' && switchId[switchId.Length - 2] < 'z')
                {
                    return switchId.Remove(switchId.Length - 2) + (char)(switchId[switchId.Length - 2] + 1) + 'a';
                }
            }

            // Se o último caractere for uma letra de 'a' a 'y', incrementa-o e retorna
            if (lastChar >= 'a' && lastChar < 'z')
            {
                return switchId.Remove(switchId.Length - 1) + (char)(lastChar + 1);
            }

            // Se o último caractere for 'z' e não houver penúltimo caractere, retorna 'a'
            if (lastChar == 'z' && switchId.Length == 1)
            {
                return "aa";
            }

            // Se o último caractere não for uma letra de 'a' a 'z', retorna uma sequência inválida
            return "Sequência inválida";
        }

    }
}
