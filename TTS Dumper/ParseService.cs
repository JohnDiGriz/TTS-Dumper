using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TTS_Dumper
{
    public static class ParseService
    {

        private static Dictionary<string, List<string>> SearchTerms = new Dictionary<string, List<string>>()
                            {
                                { "Images", new List<string>(){ "BackURL", "FaceURL", "DiffuseURL", "ImageURL", "ImageSecondaryURL", "URL", "TableURL", "SkyURL", "NormalURL" } },
                                { "Models", new List<string>(){ "MeshURL", "ColliderURL" } },
                                { "PDF", new List<string>(){ "PDFUrl" } },
                                { "Assetbundles", new List<string>(){ "AssetbundleURL", "AssetbundleSecondaryURL" } },
                                { "Audio", new List<string>(){ } }
                            };

        public static ModModel ParseModFile(string jsonFile)
        {
            var root = JObject.Parse(jsonFile);
            var model = new ModModel();
            model.Name = root.SelectToken("SaveName").ToString();
            model.Data = new Dictionary<string, IEnumerable<string>>();
            foreach (var type in SearchTerms.Keys)
            {
                model.Data.Add(type, root.Descendants()
                                            .Where(x => x.Type == JTokenType.Property && SearchTerms[type].Contains(((JProperty)x).Name) && ((JProperty)x).Value.ToString() != String.Empty)
                                            .Select(p => Regex.Replace(((JProperty)p).Value.ToString(), @"\{.*\}", string.Empty)).Distinct());

            }
            model.UnknownFiles = FindScriptLinks(jsonFile);
            return model;
        }
        private static IEnumerable<string> FindScriptLinks(string json)
        {
            var matches = Regex.Matches(json, @"('http[^']*'|\\""http:[^\\]*\\"")");
            return matches.Select(x => x.Value.Trim('\'', '"', '\\')).Distinct();
        }
    }
}
