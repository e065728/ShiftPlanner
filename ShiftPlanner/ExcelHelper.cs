using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace ShiftPlanner
{
    /// <summary>
    /// Excel(XML)形式での入出力を補助するクラス
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// シート名とデータリストを受け取りExcel XML形式で保存します。
        /// </summary>
        public static void エクスポート(Dictionary<string, IList> データ, string 保存先)
        {
            if (string.IsNullOrWhiteSpace(保存先) || データ == null)
            {
                return;
            }

            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            using var writer = XmlWriter.Create(保存先, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
            writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
            writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
            writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");

            foreach (var kv in データ)
            {
                string sheetName = kv.Key;
                var list = kv.Value ?? new ArrayList();

                writer.WriteStartElement("Worksheet");
                writer.WriteAttributeString("ss", "Name", null, sheetName);
                writer.WriteStartElement("Table");

                var first = list.Count > 0 ? list[0] : null;
                if (first != null)
                {
                    var props = first.GetType().GetProperties();
                    // ヘッダ行
                    writer.WriteStartElement("Row");
                    foreach (var p in props)
                    {
                        writer.WriteStartElement("Cell");
                        writer.WriteStartElement("Data");
                        writer.WriteAttributeString("ss", "Type", null, "String");
                        writer.WriteString(p.Name);
                        writer.WriteEndElement(); // Data
                        writer.WriteEndElement(); // Cell
                    }
                    writer.WriteEndElement(); // Row

                    // データ行
                    foreach (var item in list)
                    {
                        writer.WriteStartElement("Row");
                        foreach (var p in props)
                        {
                            object? val = p.GetValue(item, null);
                            string str = 値を文字列に変換(val);
                            writer.WriteStartElement("Cell");
                            writer.WriteStartElement("Data");
                            writer.WriteAttributeString("ss", "Type", null, "String");
                            writer.WriteString(str);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement(); // Table
                writer.WriteEndElement(); // Worksheet
            }

            writer.WriteEndElement(); // Workbook
            writer.WriteEndDocument();
        }

        /// <summary>
        /// Excel XMLを読み込み、シートごとの行データを取得します。
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> インポート(string ファイル)
        {
            var result = new Dictionary<string, List<Dictionary<string, string>>>();
            if (string.IsNullOrWhiteSpace(ファイル) || !File.Exists(ファイル))
            {
                return result;
            }

            var doc = new XmlDocument();
            doc.Load(ファイル);
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");

            // シート一覧を取得。存在しない場合は空の結果を返す
            var sheetNodes = doc.SelectNodes("//ss:Worksheet", ns);
            if (sheetNodes == null)
            {
                return result;
            }

            foreach (XmlNode sheet in sheetNodes)
            {
                var nameAttr = sheet.Attributes?["ss:Name"];
                string sheetName = nameAttr?.Value ?? "Sheet";
                var rows = new List<Dictionary<string, string>>();
                var table = sheet.SelectSingleNode("ss:Table", ns);
                if (table == null)
                {
                    continue;
                }
                var rowNodes = table.SelectNodes("ss:Row", ns);
                if (rowNodes == null || rowNodes.Count == 0)
                {
                    result[sheetName] = rows;
                    continue;
                }

                var headerCells = rowNodes[0].SelectNodes("ss:Cell/ss:Data", ns);
                var headers = new List<string>();
                if (headerCells != null)
                {
                    foreach (XmlNode cell in headerCells)
                    {
                        headers.Add(cell.InnerText);
                    }
                }

                for (int i = 1; i < rowNodes.Count; i++)
                {
                    var row = rowNodes[i];
                    var dataCells = row.SelectNodes("ss:Cell/ss:Data", ns);
                    var dict = new Dictionary<string, string>();
                    int col = 0;
                    if (dataCells != null)
                    {
                        foreach (XmlNode cell in dataCells)
                        {
                            if (col < headers.Count)
                            {
                                dict[headers[col]] = cell.InnerText;
                            }
                            col++;
                        }
                    }
                    rows.Add(dict);
                }

                result[sheetName] = rows;
            }

            return result;
        }

        /// <summary>
        /// 行データからオブジェクトのリストへ変換します。
        /// </summary>
        public static List<T> 行データからオブジェクトへ変換<T>(List<Dictionary<string, string>> 行) where T : new()
        {
            var list = new List<T>();
            if (行 == null)
            {
                return list;
            }

            var props = typeof(T).GetProperties();
            foreach (var row in 行)
            {
                var obj = new T();
                foreach (var p in props)
                {
                    if (row.TryGetValue(p.Name, out var s))
                    {
                        object? val = 文字列を値に変換(s, p.PropertyType);
                        if (val != null)
                        {
                            p.SetValue(obj, val);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        private static string 値を文字列に変換(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            switch (value)
            {
                case DateTime dt:
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");
                case TimeSpan ts:
                    return ts.ToString();
                case Enum e:
                    return e.ToString();
                case IList list when !(value is string):
                    return JsonSerialize(value, value.GetType());
                default:
                    if (!value.GetType().IsPrimitive && !(value is string))
                    {
                        return JsonSerialize(value, value.GetType());
                    }
                    return Convert.ToString(value) ?? string.Empty;
            }
        }

        private static object? 文字列を値に変換(string str, Type type)
        {
            if (type == typeof(string))
            {
                return str;
            }
            if (type == typeof(int))
            {
                int.TryParse(str, out int v);
                return v;
            }
            if (type == typeof(double))
            {
                double.TryParse(str, out double d);
                return d;
            }
            if (type == typeof(bool))
            {
                bool.TryParse(str, out bool b);
                return b;
            }
            if (type == typeof(DateTime))
            {
                DateTime.TryParse(str, out DateTime dt);
                return dt;
            }
            if (type == typeof(TimeSpan))
            {
                TimeSpan.TryParse(str, out TimeSpan ts);
                return ts;
            }
            if (type.IsEnum)
            {
                try
                {
                    return Enum.Parse(type, str);
                }
                catch
                {
                    return Activator.CreateInstance(type);
                }
            }
            if (typeof(IList).IsAssignableFrom(type) || (!type.IsPrimitive && type != typeof(string)))
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return Activator.CreateInstance(type);
                }
                return JsonDeserialize(str, type);
            }
            return null;
        }

        private static string JsonSerialize(object value, Type type)
        {
            var serializer = new DataContractJsonSerializer(type);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, value);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static object? JsonDeserialize(string json, Type type)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(type);
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                return serializer.ReadObject(ms);
            }
            catch
            {
                return Activator.CreateInstance(type);
            }
        }
    }
}
