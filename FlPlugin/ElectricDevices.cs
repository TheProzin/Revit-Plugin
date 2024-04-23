using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FlPlugin
{
    internal class ElectricDevices
    {
        public void ProcessGroupCategory(Document doc, Element element)
        {
            FamilyInstance familyInstance = element as FamilyInstance;

            if (element.Category.Name == "Lighting Devices")
            {
                var i = 0;
                TagPosition lightingTagPosition = new TagPosition
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

                        (new ElectricDevices()).ProcessLightingDevice(doc, subComponent, lightingTagPosition);

                        bool isEven = i % 2 == 0;
                        if (!isEven)
                        {
                            lightingTagPosition.x = 0.45;
                            lightingTagPosition.y += 0.45;
                        }
                        else
                        {
                            lightingTagPosition.x += 0.7;
                        }
                        i++;
                    }

                    if (subComponent.Name.Contains("Módulo de Tomada não listavel"))
                    {

                        (new ElectricDevices()).ProcessElectricalFixture(doc, subComponent, lightingTagPosition);

                    }
                }

                if (i == 0)
                {
                    (new ElectricDevices()).ProcessLightingDevice(doc, element, lightingTagPosition);
                }

            }
            else if (element.Category.Name == "Electrical Fixtures")
            {
                var i = 0;
                TagPosition lightingTagPosition = new TagPosition
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

                        (new ElectricDevices()).ProcessLightingDevice(doc, subComponent, lightingTagPosition);

                        bool isEven = i % 2 == 0;
                        if (!isEven)
                        {
                            lightingTagPosition.x = 0.45;
                            lightingTagPosition.y += 0.45;
                        }
                        else
                        {
                            lightingTagPosition.x += 0.7;
                        }
                        i++;
                    }

                    if (subComponent.Name.Contains("Módulo de Tomada não listavel"))
                    {

                        (new ElectricDevices()).ProcessElectricalFixture(doc, subComponent, lightingTagPosition);

                    }
                }

                if (i == 0)
                {
                    (new ElectricDevices()).ProcessElectricalFixture(doc, element, lightingTagPosition);
                }
            }
            else if (element.Category.Name == "Lighting Fixtures")
            {
                TagPosition lightingTagPosition = new TagPosition
                {
                    x = 0.45,
                    y = 0.25,
                    z = 1.0
                };
                (new ElectricDevices()).ProcessLightingFixture(doc, element, lightingTagPosition);

            }
            else
            {
                TaskDialog.Show("Erro", "O elemento selecionado não é um dispositivo de iluminação.");
            }
        }

        public void ProcessLightingDevice(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;

            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);
            (new SwitchIdManager()).InsertSwitchId(doc, selectedElement);
            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Interruptor);
        }

        public void ProcessLightingFixture(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;

            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Iluminacao);
        }

        public void ProcessElectricalFixture(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;

            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Tomada);
        }

    }
}
