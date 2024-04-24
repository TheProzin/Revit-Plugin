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

        public void ProcessElements(Document doc, ICollection<ElementId> selectedElements)
        {
            //para todos os elementos selecionados vai conferir a categoria do conjunto
            foreach (ElementId selectedId in selectedElements)
            {
                Element selectedElement = doc.GetElement(selectedId);

                if (selectedElement.Category.Name == "Lighting Devices")
                {
                    ProcessLightingDeviceGroupCategory(doc, selectedElement);
                }
                else if (selectedElement.Category.Name == "Electrical Fixtures")
                {
                    ProcessElectricalFixtureGroupCategory(doc, selectedElement);
                }
                else if (selectedElement.Category.Name == "Lighting Fixtures")
                {
                    ProcessLightingFixtureGroupCategory(doc, selectedElement);
                }
                else
                {
                    TaskDialog.Show("Erro", "O elemento selecionado não é um dispositivo de iluminação.");
                }

            }
        }

        private void ProcessLightingDeviceGroupCategory(Document doc, Element selectedElement)
        {
            //para o conjunto, verifica os subcomponentes e insere as tags
            FamilyInstance familyInstance = selectedElement as FamilyInstance;

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

                //Se o subcomponente for um módulo de interruptor não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tecla Interrupor não listavel"))
                {

                    (new ElectricDevices()).ProcessLightingDeviceElementCategory(doc, subComponent, lightingTagPosition);

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

                //Se o subcomponente for um módulo de tomada não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tomada não listavel"))
                {

                    (new ElectricDevices()).ProcessElectricalFixtureElementCategory(doc, subComponent, lightingTagPosition);

                }
            }

            //Se não houver subcomponentes, insere a tag no conjunto pois provavelmente é um módulo de interruptor
            if (i == 0)
            {
                (new ElectricDevices()).ProcessLightingDeviceElementCategory(doc, selectedElement, lightingTagPosition);
            }
        }

        private void ProcessLightingFixtureGroupCategory(Document doc, Element selectedElement)
        {
            //para o conjunto insere as tags

            TagPosition lightingTagPosition = new TagPosition
            {
                x = 0.45,
                y = 0.25,
                z = 1.0
            };
            (new ElectricDevices()).ProcessLightingFixtureElementCategory(doc, selectedElement, lightingTagPosition);
        }

        private void ProcessElectricalFixtureGroupCategory(Document doc, Element selectedElement)
        {
            //para o conjunto, verifica os subcomponentes e insere as tags

            FamilyInstance familyInstance = selectedElement as FamilyInstance;

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

                //Se o subcomponente for um módulo de interruptor não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tecla Interrupor não listavel"))
                {

                    (new ElectricDevices()).ProcessLightingDeviceElementCategory(doc, subComponent, lightingTagPosition);

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

                //Se o subcomponente for um módulo de tomada não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tomada não listavel"))
                {

                    (new ElectricDevices()).ProcessElectricalFixtureElementCategory(doc, subComponent, lightingTagPosition);
                    i++;
                }
            }

            //Se não houver subcomponentes, insere a tag no conjunto, poir provavelmente é um módulo de tomada
            if (i == 0)
            {
                (new ElectricDevices()).ProcessElectricalFixtureElementCategory(doc, selectedElement, lightingTagPosition);
            }
        }

        private void ProcessLightingDeviceElementCategory(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            (new SwitchIdManager()).InsertSwitchId(doc, selectedElement);
            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Interruptor);
        }

        private void ProcessLightingFixtureElementCategory(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Iluminacao);
        }

        private void ProcessElectricalFixtureElementCategory(Document doc, Element selectedElement, TagPosition tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            IList<Parameter> socketPotency = familyInstanceSubComponent.GetParameters("Potência Aparente (VA)");

            if (socketPotency.Count > 0)
            {
                // Acessar o primeiro parâmetro encontrado
                Parameter potency = socketPotency[0];

                if (potency.AsValueString() != "100 VA")
                {
                    TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.PotenciaTomada);
                }
            }

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Tomada);
        }

    }
}
