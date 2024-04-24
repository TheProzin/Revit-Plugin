using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlPlugin
{
    public class SwitchIdManager
    {
        private string GetNextSwitchId(Document doc)
        {
            //Busca todos os elementos de categoria OST_LightingDevices que possuem a propriedade Switch ID preenchida
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

        public void InsertSwitchId(Document doc, Element selectedElement)
        {
            // Verifica se o elemento selecionado é um dispositivo de iluminação
            if (selectedElement.Category.Id.Value == (int)BuiltInCategory.OST_LightingDevices)
            {
                string switchIdValue = GetNextSwitchId(doc);

                if (string.IsNullOrEmpty(switchIdValue))
                {
                    TaskDialog.Show("Erro", "Não foi possível obter o próximo Switch ID.");
                    return;
                }
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
                            switchIDParameter.Set(switchIdValue);
                            transaction.Commit();
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Erro", "A propriedade Switch ID já está preenchida.");
                        return;
                    }
                }
                else
                {
                    TaskDialog.Show("Erro", "A propriedade Switch ID não está disponível ou é somente leitura.");
                    return;
                }
            }
            else
            {
                TaskDialog.Show("Erro", "O elemento selecionado não é um interruptor.");
                return;
            }
        }
    }
}
