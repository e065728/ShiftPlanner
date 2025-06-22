using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace ShiftPlanner
{
    /// <summary>
    /// Excel(xlsx)形式での入出力を補助するクラス
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// シート名とデータリストを受け取りExcel(xlsx)形式で保存します。
        /// </summary>
        public static void エクスポート(Dictionary<string, IList> データ, string 保存先)
        {
            if (string.IsNullOrWhiteSpace(保存先) || データ == null)
            {
                return;
            }

            var dir = Path.GetDirectoryName(保存先);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var stream = File.Create(保存先);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

            var sheetInfos = new List<(string Name, string RelId, string Path)>();
            int sheetIndex = 1;

            foreach (var kv in データ)
            {
                string sheetName = kv.Key;
                var list = kv.Value ?? new ArrayList();

                string sheetPath = $"xl/worksheets/sheet{sheetIndex}.xml";
                string relId = $"rId{sheetIndex}";
                sheetInfos.Add((sheetName, relId, $"worksheets/sheet{sheetIndex}.xml"));

                var entry = archive.CreateEntry(sheetPath);
                var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
                using (var writer = XmlWriter.Create(entry.Open(), writerSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                    writer.WriteStartElement("sheetData");

                    object? first = list.Count > 0 ? list[0] : null;
                    if (first != null)
                    {
                        var props = first.GetType().GetProperties();

                        writer.WriteStartElement("row");
                        foreach (var p in props)
                        {
                            writer.WriteStartElement("c");
                            writer.WriteAttributeString("t", "str");
                            writer.WriteElementString("v", p.Name);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        foreach (var item in list)
                        {
                            writer.WriteStartElement("row");
                            foreach (var p in props)
                            {
                                object? val = p.GetValue(item);
                                string str = 値を文字列に変換(val);
                                writer.WriteStartElement("c");
                                writer.WriteAttributeString("t", "str");
                                writer.WriteElementString("v", str);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                sheetIndex++;
            }

            // workbook.xml
            var workbookEntry = archive.CreateEntry("xl/workbook.xml");
            using (var writer = XmlWriter.Create(workbookEntry.Open(), new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("workbook", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                writer.WriteStartElement("sheets");
                int sid = 1;
                foreach (var info in sheetInfos)
                {
                    writer.WriteStartElement("sheet");
                    writer.WriteAttributeString("name", info.Name);
                    writer.WriteAttributeString("sheetId", sid.ToString());
                    writer.WriteAttributeString("r", "id", null, info.RelId);
                    writer.WriteEndElement();
                    sid++;
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            // workbook relationships
            var relEntry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
            using (var writer = XmlWriter.Create(relEntry.Open(), new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
                foreach (var info in sheetInfos)
                {
                    writer.WriteStartElement("Relationship");
                    writer.WriteAttributeString("Id", info.RelId);
                    writer.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
                    writer.WriteAttributeString("Target", info.Path);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            // root relationships
            var rootEntry = archive.CreateEntry("_rels/.rels");
            using (var writer = XmlWriter.Create(rootEntry.Open(), new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
                writer.WriteStartElement("Relationship");
                writer.WriteAttributeString("Id", "rId1");
                writer.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
                writer.WriteAttributeString("Target", "xl/workbook.xml");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            // content types
            var contentEntry = archive.CreateEntry("[Content_Types].xml");
            using (var writer = XmlWriter.Create(contentEntry.Open(), new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Types", "http://schemas.openxmlformats.org/package/2006/content-types");
                writer.WriteStartElement("Default");
                writer.WriteAttributeString("Extension", "rels");
                writer.WriteAttributeString("ContentType", "application/vnd.openxmlformats-package.relationships+xml");
                writer.WriteEndElement();
                writer.WriteStartElement("Default");
                writer.WriteAttributeString("Extension", "xml");
                writer.WriteAttributeString("ContentType", "application/xml");
                writer.WriteEndElement();
                writer.WriteStartElement("Override");
                writer.WriteAttributeString("PartName", "/xl/workbook.xml");
                writer.WriteAttributeString("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
                writer.WriteEndElement();
                int sid = 1;
                foreach (var info in sheetInfos)
                {
                    writer.WriteStartElement("Override");
                    writer.WriteAttributeString("PartName", $"/xl/worksheets/sheet{sid}.xml");
                    writer.WriteAttributeString("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
                    writer.WriteEndElement();
                    sid++;
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Excel(xlsx)を読み込み、シートごとの行データを取得します。
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> インポート(string ファイル)
        {
            var result = new Dictionary<string, List<Dictionary<string, string>>>();
            if (string.IsNullOrWhiteSpace(ファイル) || !File.Exists(ファイル))
            {
                return result;
            }

            using var archive = new ZipArchive(File.OpenRead(ファイル), ZipArchiveMode.Read);

            var workbookEntry = archive.GetEntry("xl/workbook.xml");
            if (workbookEntry == null)
            {
                return result;
            }

            var workbookDoc = new XmlDocument();
            using (var ws = workbookEntry.Open())
            {
                workbookDoc.Load(ws);
            }

            var ns = new XmlNamespaceManager(workbookDoc.NameTable);
            ns.AddNamespace("d", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            ns.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

            var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            var relsMap = new Dictionary<string, string>();
            if (relsEntry != null)
            {
                var relDoc = new XmlDocument();
                using (var rs = relsEntry.Open())
                {
                    relDoc.Load(rs);
                }
                var relNs = new XmlNamespaceManager(relDoc.NameTable);
                relNs.AddNamespace("r", "http://schemas.openxmlformats.org/package/2006/relationships");
                var relNodes = relDoc.SelectNodes("//r:Relationship", relNs);
                if (relNodes != null)
                {
                    foreach (XmlNode rel in relNodes)
                    {
                        var id = rel.Attributes?["Id"]?.Value;
                        var target = rel.Attributes?["Target"]?.Value;
                        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target))
                        {
                            relsMap[id] = target;
                        }
                    }
                }
            }

            var sheetNodes = workbookDoc.SelectNodes("//d:sheet", ns);
            if (sheetNodes == null)
            {
                return result;
            }

            foreach (XmlNode sheet in sheetNodes)
            {
                string name = sheet.Attributes?["name"]?.Value ?? "Sheet";
                string rid = sheet.Attributes?["r:id"]?.Value ?? string.Empty;
                if (!relsMap.TryGetValue(rid, out var target))
                {
                    continue;
                }
                string path = "xl/" + target.TrimStart('/');
                var entry = archive.GetEntry(path);
                if (entry == null)
                {
                    continue;
                }
                var sheetDoc = new XmlDocument();
                using (var ss = entry.Open())
                {
                    sheetDoc.Load(ss);
                }
                var sheetNs = new XmlNamespaceManager(sheetDoc.NameTable);
                sheetNs.AddNamespace("d", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

                var rowNodes = sheetDoc.SelectNodes("//d:sheetData/d:row", sheetNs);
                var rows = new List<Dictionary<string, string>>();
                if (rowNodes == null || rowNodes.Count == 0)
                {
                    result[name] = rows;
                    continue;
                }

                var headerCells = rowNodes[0].SelectNodes("d:c/d:v", sheetNs);
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
                    var dataCells = row.SelectNodes("d:c/d:v", sheetNs);
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

                result[name] = rows;
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
