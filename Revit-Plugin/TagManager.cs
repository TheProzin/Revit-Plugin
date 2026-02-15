using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlPlugin
{
    internal class TagManager
    {
        public struct TagsId
        {
            public const int Interruptor = 847296; // Tag para Interruptor (Switch ID) //Lighting Device
            public const int Iluminacao = 2072026; // Tag de Luminária na Parede //Lighting Fixture
            public const int Tomada = 847034; // Tag N° do Circuito em Tomada //Electrical Fixture
            public const int PotenciaTomada = 2896981; // Tag Potência do Ponto de Tomada //Electrical Fixture
        }

        public static FamilySymbol GetTagSymbolLightingFixture(Document doc, int tagId)
        {
            // Obtém todos os tipos de família de tag disponíveis no documento
            FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_LightingFixtureTags)
                .WhereElementIsElementType();

            // Filtra os tipos de família de tag para obter o desejado
            FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>()
                .FirstOrDefault(x => x.Id.Value == tagId);

            return tagSymbol;
        }

        public static FamilySymbol GetTagSymbolLightingDevice(Document doc, int tagId)
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

        public static FamilySymbol GetTagSymbolElectricalFixture(Document doc, int tagId)
        {
            // Obtém todos os tipos de família de tag disponíveis no documento
            FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_ElectricalFixtureTags)
                .WhereElementIsElementType();

            // Filtra os tipos de família de tag para obter o desejado
            FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>()
                .FirstOrDefault(x => x.Id.Value == tagId);

            return tagSymbol;
        }

        public static void CreateTag(Document doc, Element selectedElement, XYZ tagPosition, int idTagSymbol)
        {
            FamilySymbol tagSymbol = null;

            if (idTagSymbol == TagsId.Iluminacao)
            {
                tagSymbol = GetTagSymbolLightingFixture(doc, idTagSymbol);

            } else if (idTagSymbol == TagsId.Tomada || idTagSymbol == TagsId.PotenciaTomada)
            {
                tagSymbol = GetTagSymbolElectricalFixture(doc, idTagSymbol);

            } else if (idTagSymbol == TagsId.Interruptor)
            {
                tagSymbol = GetTagSymbolLightingDevice(doc, idTagSymbol);

            }

            if (tagSymbol == null)
            {
                TaskDialog.Show("Erro", "O tipo de família de tag não foi encontrado.");
                return;
            }

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Criar Tag");
                IndependentTag tag = IndependentTag.Create(doc, doc.ActiveView.Id, new Reference(selectedElement), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagPosition);
                tag.ChangeTypeId(tagSymbol.Id);
                transaction.Commit();
            }
        }
    }
}
