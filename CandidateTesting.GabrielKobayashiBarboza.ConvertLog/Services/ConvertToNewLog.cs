using CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Configuration;
using CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Models;
using RestSharp;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Services
{
    public class ConvertToNewLog
    {
        public ConvertToNewLog()
        {
            LogConfiguration.ConfigLog();
        }

        public void ConvertLog()
        {
            try
            {
                var contentSplit = ReadUrlAndPathLogs();

                var url = contentSplit[1];
                Log.Information($"URL com modelo antigo do log: {url}");

                var path = contentSplit[2];
                Log.Information($"Caminho para novo log: {path}");

                Log.Debug($"Capturando log antigo da URL: {url}");
                var log = GetLogUrl(url);

                if (log.Contains("Erro ao acessar URL."))
                    throw new Exception(log);

                Log.Debug($"Log antigo capturado:\n{log}");

                Log.Debug("Gerando novo log.");
                var newLog = GenerateNewLog(log);

                SaveNewLog(newLog, path);
            }
            catch (Exception e)
            {
                Log.Error($"Mensagem: {e.Message}");
            }
            finally
            {
                Console.WriteLine("\n\nPress any key to close.");
                Console.ReadKey();
            }
        }

        private static string[] ReadUrlAndPathLogs()
        {
            Log.Information("==================== Iniciando =======================");
            Log.Information("\nPara iniciar a conversão de log, siga o modelo: ");
            Log.Information("convert URL(Digite a URL com o log antigo) Caminho da maquina e nome do arquivo(Digite o local onde será salvo o novo log na máquina e nome do arquivo)\n");
            var urlAndPath = Console.ReadLine();

            var contentSplit = urlAndPath.Split(" ");

            if (contentSplit.Count() != 3 || !contentSplit[0].ToLower().Equals("convert"))
                throw new Exception("Entrada fora do padrão, favor seguir modelo de entrada.");

            return contentSplit;
        }

        public string GetLogUrl(string url)
        {
            try
            {
                RestClient client;
                client = new RestClient(url);

                var request = new RestRequest();
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    return response.Content;
                else
                    return $"Erro ao acessar URL. StatusCode: {response.StatusCode}. Mensagem: {response.ErrorMessage}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao capturar log.\nMensagem: {ex.Message}.\n StackTrace: {ex.StackTrace}");
            }
        }

        public string GenerateNewLog(string log)
        {
            StringBuilder newLog = new StringBuilder();
            newLog.AppendLine("#Version: 1.0");
            newLog.AppendLine($"#Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            newLog.AppendLine("#Fields: provider http-method status-code uri-path time-taken response - size cache - status");

            foreach (var lineLog in log.Split('\n'))
            {
                if (string.IsNullOrEmpty(lineLog))
                    continue;

                var infoLog = SeparateInfoLog(lineLog);
                var formattedLog = FormatLog(infoLog);

                newLog.AppendLine(formattedLog);
            }

            Log.Debug("Novo log gerado.");
            Log.Debug($"{newLog.ToString()}");

            return newLog.ToString();
        }

        public LogModelNow SeparateInfoLog(string log)
        {
            try
            {
                Log.Debug("Capturando informações do modelo antigo do log...");
                var getInfo = log.Split('|');

                var isLogValid = ValidateInfo(getInfo);

                if (!isLogValid)
                    throw new Exception("Log antigo fora do padrão.");

                var httpMethod = getInfo[3].Split("/")[0].Replace("\"", "").Trim();
                var uriPath = getInfo[3].Split(" ")[1];
                var statusCode = int.Parse(getInfo[1]);
                var responseSize = int.Parse(getInfo[0]);
                var cacheStatus = getInfo[2].Equals("INVALIDATE") ? "REFRESH_HIT" : getInfo[2];

                var replaceTimeTaken = getInfo[4].Replace("\r", "").Replace(".", ",");
                var timeTaken = Convert.ToInt32(float.Parse(replaceTimeTaken));

                var logModelNow = new LogModelNow(httpMethod, statusCode, uriPath, timeTaken, responseSize, cacheStatus);

                return logModelNow;
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private bool ValidateInfo(string[] getInfo)
        {
            if (getInfo.Length != 5)
                return false;
            if (string.IsNullOrEmpty(getInfo[0]) || !Regex.IsMatch(getInfo[0], @"^[0-9]+$"))
                return false;
            if (string.IsNullOrEmpty(getInfo[1]) || !Regex.IsMatch(getInfo[1], @"^[0-9]+$"))
                return false;
            if (string.IsNullOrEmpty(getInfo[2]) || !Regex.IsMatch(getInfo[2], @"^[a-zA-Z]+$"))
                return false;
            if (string.IsNullOrEmpty(getInfo[3]) || getInfo[3].Split(" ").Length != 3)
                return false;
            if (string.IsNullOrEmpty(getInfo[4]) || !Regex.IsMatch(getInfo[4], @"^[0-9]+([,.][0-9]{1})*$"))
                return false;

            return true;
        }

        public string FormatLog(LogModelNow logModel)
        {
            Log.Debug("Formatando linha com informações do modelo antigo do log para modelo novo.");
            var modelFormated = $"\"{logModel.Provider}\" {logModel.HttpMethod} {logModel.StatusCode} {logModel.UriPath} {logModel.TimeTaken} {logModel.ResponseSize} {logModel.CacheStatus}";

            return modelFormated;
        }

        public void SaveNewLog(string newLog, string path)
        {
            Log.Debug("Capturado caminho e nome do arquivo para novo log.");

            try
            {
                var fileName = path.Split('/').Last();
                var filePath = string.Empty;

                if (string.IsNullOrEmpty(fileName))
                {
                    Log.Debug("Nome do arquivo não informado, salvando arquivo com nome padrão: NewLogNow.txt.");
                    fileName = "NewLogNow.txt";
                    filePath = path;
                }
                else 
                    filePath = path.Replace(fileName, "");

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                if (File.Exists(filePath + fileName))
                    File.Delete(filePath + fileName);

                Log.Debug("Salvando log com novo modelo.");
                using (StreamWriter writer = new StreamWriter(filePath + fileName, true))
                    writer.WriteLine(newLog);

                Log.Debug($"Log salvo com sucesso em {filePath + fileName}");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }  
        }
    }
}
