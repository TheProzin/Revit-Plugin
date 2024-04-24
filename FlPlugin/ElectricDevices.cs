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

            ElectricDevices ElectricDevices = new ElectricDevices();

            var i = 0;
            TagXYZ tagPosition = new TagXYZ
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

                    ElectricDevices.ProcessLightingDeviceElementCategory(doc, subComponent, tagPosition);

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

            //Se não houver subcomponentes, insere a tag no conjunto pois provavelmente é um módulo de interruptor
            if (i == 0)
            {
                ElectricDevices.ProcessLightingDeviceElementCategory(doc, selectedElement, tagPosition);
            }
        }

        private void ProcessLightingFixtureGroupCategory(Document doc, Element selectedElement)
        {
            //para o conjunto insere as tags

            TagXYZ tagPosition = new TagXYZ
            {
                x = 0.5,
                y = 0.25,
                z = 1.0
            };
            (new ElectricDevices()).ProcessLightingFixtureElementCategory(doc, selectedElement, tagPosition);
        }

        private void ProcessElectricalFixtureGroupCategory(Document doc, Element selectedElement)
        {
            //para o conjunto, verifica os subcomponentes e insere as tags

            FamilyInstance familyInstance = selectedElement as FamilyInstance;

            ElectricDevices ElectricDevices = new ElectricDevices();

            var iLighting = 0;
            var iElectrical = 0;
            TagXYZ tagPositionLighting = new TagXYZ
            {
                x = 0.7,
                y = 0.25,
                z = 1.0
            };

            TagXYZ tagPositionElectrical = new TagXYZ
            {
                x = -0.8,
                y = 0.1,
                z = 1.0
            };

            foreach (var subComponentId in familyInstance.GetSubComponentIds())
            {

                var subComponent = doc.GetElement(subComponentId);

                //Se o subcomponente for um módulo de interruptor não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tecla Interrupor não listavel"))
                {

                    ElectricDevices.ProcessLightingDeviceElementCategory(doc, subComponent, tagPositionLighting);

                    bool isEven = iLighting % 2 == 0;
                    if (!isEven)
                    {
                        tagPositionLighting.x = 0.7;
                        tagPositionLighting.y += 0.45;
                    }
                    else
                    {
                        tagPositionLighting.x += 0.45;
                    }
                    iLighting++;
                }

                //Se o subcomponente for um módulo de tomada não listável, insere a tag
                if (subComponent.Name.Contains("Módulo de Tomada não listavel"))
                {
                    ElectricDevices.ProcessElectricalFixtureElementCategory(doc, subComponent, tagPositionElectrical);
                    if (iElectrical == 2)
                    {
                        tagPositionElectrical.x = -1.6;
                        tagPositionElectrical.y = 0.1;
                    }
                    else
                    {
                        tagPositionElectrical.y += 0.45;
                    }
                    iElectrical++;
                }
            }

            //Se não houver subcomponentes, insere a tag no conjunto, poir provavelmente é um módulo de tomada
            if (iElectrical == 0)
            {
                ElectricDevices.ProcessElectricalFixtureElementCategory(doc, selectedElement, tagPositionElectrical);
            }
        }

        private void ProcessLightingDeviceElementCategory(Document doc, Element selectedElement, TagXYZ tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            (new SwitchIdManager()).InsertSwitchId(doc, selectedElement);
            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Interruptor);
        }

        private void ProcessLightingFixtureElementCategory(Document doc, Element selectedElement, TagXYZ tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Iluminacao);
        }

        private void ProcessElectricalFixtureElementCategory(Document doc, Element selectedElement, TagXYZ tagPosition)
        {
            FamilyInstance familyInstanceSubComponent = selectedElement as FamilyInstance;
            LocationPoint locationPoint = familyInstanceSubComponent.Location as LocationPoint;
            XYZ location = locationPoint.Point;
            XYZ tagLocation = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);

            TagManager.CreateTag(doc, selectedElement, tagLocation, TagManager.TagsId.Tomada);

            IList<Parameter> socketPotency = familyInstanceSubComponent.GetParameters("Potência Aparente (VA)");

            if (socketPotency.Count > 0)
            {
                // Acessar o primeiro parâmetro encontrado
                Parameter potency = socketPotency[0];

                if (potency.AsValueString() != "100 VA")
                {
                    tagPosition.y += 0.45;
                    XYZ tagLocationPotency = new XYZ(location.X + tagPosition.x, location.Y + tagPosition.y, location.Z + tagPosition.y);
                    TagManager.CreateTag(doc, selectedElement, tagLocationPotency, TagManager.TagsId.PotenciaTomada);
                }
            }

        }

    }
}
