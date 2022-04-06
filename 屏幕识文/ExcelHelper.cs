using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 屏幕识文
{
    static class ExcelHelper
    {
        public static string ToName(int index)
        {
            if (index < 0) { throw new Exception("invalid parameter"); }

            List<string> chars = new List<string>();
            do
            {
                if (chars.Count > 0) index--;
                chars.Insert(0, ((char)(index % 26 + 'A')).ToString());
                index = (index - index % 26) / 26;
            } while (index > 0);

            return string.Join(string.Empty, chars.ToArray());
        }

        public static int ToIndex(string columnName)
        {
            if (!Regex.IsMatch(columnName.ToUpper(), @"[A-Z]+")) { throw new Exception("invalid parameter"); }

            int index = 0;
            char[] chars = columnName.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                index += ((int)chars[i] - (int)'A' + 1) * (int)Math.Pow(26, chars.Length - i - 1);
            }
            return index - 1;
        }

        public static void BaiduAI_AddCells(IXLWorksheet worksheet, JArray cells, bool isBlod = false)
        {
            for (var j = 0; j < cells.Count; j++)
            {
                var cellData = cells[j].Value<JObject>();
                var columnIndex = cellData["column"][0].Value<int>();
                var columnRef = ToName(columnIndex); // 会自动加1
                var rowIndex = cellData["row"][0].Value<uint>() + 1;
                var word = cellData["word"].Value<string>();

                var cell = worksheet.Cell(columnRef + rowIndex);
                cell.Value = word;
                cell.Style.Font.SetBold(isBlod);
            }
        }
    }
}
