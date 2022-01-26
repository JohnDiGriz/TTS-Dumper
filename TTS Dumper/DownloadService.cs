using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Mime;

namespace TTS_Dumper
{
    public static class DownloadService
    {

        private static Dictionary<string, string> DefaultExtensions = new Dictionary<string, string>()
                            {
                                { "Models", "obj" },
                                { "PDF", "pdf" },
                                { "Assetbundles", "unity3d" },
                                { "Audio", "mp3" }
                            };

        public static string DownloadMods(string modsFolder, string finalFolder)
        {
            var res = "";
            var files = Directory.EnumerateFiles(modsFolder, "*.json");
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var model = ParseService.ParseModFile(json);
                if (!GenerateModFolder(finalFolder, model, new FileInfo(file), new FileInfo($"{file.Substring(0, file.Length-4)}png")))
                {
                    res += $"{model.Name} - some errors occured, please look at the log file\n";
                }
            }
            return res;
        }


        private static bool GenerateModFolder(string finalFolder, ModModel model, FileInfo json, FileInfo thumb)
        {
            var rootFolder = Directory.CreateDirectory($"{finalFolder}/{Regex.Replace(model.Name, @"[:\|]*", string.Empty)}").FullName;
            var workshopFolder = Directory.CreateDirectory($"{rootFolder}/Workshop").FullName;
            json.CopyTo($"{workshopFolder}/{json.Name}");
            thumb.CopyTo($"{workshopFolder}/{thumb.Name}");
            var log = String.Empty;
            using (var client = new WebClient())
            {
                foreach (var type in model.Data.Keys)
                {
                    var folder = Directory.CreateDirectory($"{rootFolder}/{type}").FullName;
                    foreach (var file in model.Data[type])
                        DownloadFile(client, (file.StartsWith("http")?file:$"http://{file}"), rootFolder, type, ref log);
                }
                foreach (var file in model.UnknownFiles)
                    DownloadFile(client, file, rootFolder, null, ref log);
            }
            if (!String.IsNullOrEmpty(log))
                File.WriteAllText($"{rootFolder}/log.txt", log);
            return String.IsNullOrEmpty(log);
        }

        private static void DownloadFile(WebClient client, string link, string rootFolder, string type, ref string log)
        {
            if (link.Contains("41C6772F5C9ABA05FF8A2959C38A9E10EF1B7FC6"))
            {
                int i = 0;
            }
            try
            {
                var data = client.DownloadData(new Uri(link));
                var fileName = Regex.Replace(link, @"[^\w]", String.Empty);
                var extension = GetExtension(client, link, type).ToLower();
                var folder = string.Empty;
                if (!string.IsNullOrWhiteSpace(type))
                    folder = type;
                else if (extension == "obj")
                    folder = "Models";
                else if (extension == "unity3d")
                    folder = "Assetbundles";
                else if (extension == "pdf")
                    folder = "PDF";
                else
                    folder = "Images";
                File.WriteAllBytes($"{rootFolder}/{folder}/{fileName}.{extension}", data);
            }
            catch (Exception ex)
            {
                log += $"Exception in file {link} - {ex.Message}\n";
            }

        }

        private static string GetExtension(WebClient client, string uri, string type)
        {
            if (!string.IsNullOrWhiteSpace(type) && DefaultExtensions.ContainsKey(type))
                return DefaultExtensions[type];
            if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                return client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].LastIndexOf('.') + 1)
                                        .Replace("\"", String.Empty).Replace(";", String.Empty);
            else if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Type"]))
            {
                if (client.ResponseHeaders["Content-Type"].Contains("text/plain"))
                    return "obj";
                else if (client.ResponseHeaders["Content-Type"].Contains("image/"))
                    return Regex.Match(client.ResponseHeaders["Content-Type"], @"/[\w]*").Value.Substring(1);
                else
                    return uri.Substring(uri.LastIndexOf('.') + 1);
            }
            else
                return uri.Substring(uri.LastIndexOf('.') + 1);
        }
    }
}
