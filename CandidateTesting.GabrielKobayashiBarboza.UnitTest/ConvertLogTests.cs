using CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Models;
using CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Services;
using System;
using System.IO;
using Xunit;

namespace CandidateTesting.GabrielKobayashiBarboza.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void GetLogUrlTest()
        {
            string url = "https://s3.amazonaws.com/uux-itaas-static/minha-cdn-logs/input-01.txt";
            var convertToNewLog = new ConvertToNewLog();

            var oldLog = convertToNewLog.GetLogUrl(url);

            Assert.Contains("312|200|HIT|\"GET /robots.txt HTTP/1.1\"|100.2", oldLog);
        }

        [Fact]
        public void IncorrectUrlTest()
        {
            string url = "https://eusouumaurlincorreta.com";
            var convertToNewLog = new ConvertToNewLog();

            var oldLog = convertToNewLog.GetLogUrl(url);

            Assert.Contains("Erro ao acessar URL.", oldLog);
        }

        [Fact]
        public void GetInfosLogTest()
        {
            var oldLog = "312|200|INVALIDATE|\"GET /robots.txt HTTP/1.1\"|245.1";
            var convertToNewLog = new ConvertToNewLog();

            var result = convertToNewLog.SeparateInfoLog(oldLog);

            Assert.Equal("/robots.txt", result.UriPath);
            Assert.Equal("REFRESH_HIT", result.CacheStatus);
            Assert.Equal("GET", result.HttpMethod);
            Assert.Equal("MINHA CDN", result.Provider);
            Assert.Equal(312, result.ResponseSize);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(245, result.TimeTaken);
        }

        [Fact]
        public void GenerateNewLogTest()
        {
            var oldLog = "312|200|INVALIDATE|\"GET /robots.txt HTTP/1.1\"|245.1";
            var convertToNewLog = new ConvertToNewLog();

            var result = convertToNewLog.GenerateNewLog(oldLog);

            Assert.Contains("#Version: 1.0", result);
            Assert.Contains("#Date:", result);
            Assert.Contains("#Fields: provider http-method status-code uri-path time-taken response - size cache - status", result);
            Assert.Contains("\"MINHA CDN\" GET 200 /robots.txt 245 312 REFRESH_HIT", result);
        }

        [Fact]
        public void OldLogFormatedVeryLargeTest()
        {
            try
            {
                var oldLog = "Eu|Sou|um|Log|no|Formato|incorreto";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedFirstFieldIncorrectTest()
        {
            try
            {
                var oldLog = "abc|200|HIT|\"GET /robots.txt HTTP/1.1\"|100.2";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedSecondFieldIncorrectTest()
        {
            try
            {
                var oldLog = "312|abc|HIT|\"GET /robots.txt HTTP/1.1\"|100.2";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedThirdFieldWhitespaceTest()
        {
            try
            {
                var oldLog = "312|abc||\"GET /robots.txt HTTP/1.1\"|100.2";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedThirdFieldNumberTest()
        {
            try
            {
                var oldLog = "312|200|132|\"GET /robots.txt HTTP/1.1\"|100.2";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedFourthFieldWhitespaceTest()
        {
            try
            {
                var oldLog = "312|200|HIT||100.2";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedFifthFieldWhitespaceTest()
        {
            try
            {
                var oldLog = "312|200|HIT|\"GET /robots.txt HTTP/1.1\"|";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void OldLogFormatedFifthFieldIncorrectTest()
        {
            try
            {
                var oldLog = "312|200|HIT|\"GET /robots.txt HTTP/1.1\"|abc";
                var convertToNewLog = new ConvertToNewLog();

                var result = convertToNewLog.GenerateNewLog(oldLog);

                Assert.Contains("Log antigo fora do padrão.", result);
            }
            catch (Exception e)
            {
                Assert.Contains("Log antigo fora do padrão.", e.Message);
            }
        }

        [Fact]
        public void FormatedLogTest()
        {
            var logModel = new LogModelNow("GET", 200, "/robots.txt", 100, 312, "HIT");
            var convertToNewLog = new ConvertToNewLog();

            var model = convertToNewLog.FormatLog(logModel);

            Assert.Contains("\"MINHA CDN\" GET 200 /robots.txt 100 312 HIT", model);
        }

        [Fact]
        public void SaveNewLogTest()
        {
            var log = "Eu sou um teste de um novo log";
            var path = $@"C:/Teste/teste.txt";
            var convertToNewLog = new ConvertToNewLog();

            convertToNewLog.SaveNewLog(log, path);

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void PathWithoufileNameTest()
        {
            var log = "Eu sou um teste de um novo log";
            var path = @"C:/Teste/";
            var convertToNewLog = new ConvertToNewLog();

            convertToNewLog.SaveNewLog(log, path);

            Assert.True(File.Exists(path + "NewLogNow.txt"));
        }
    }
}
