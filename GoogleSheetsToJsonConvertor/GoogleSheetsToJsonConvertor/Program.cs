using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using GoogleSheetsToJsonConvertor.Extensions;
using Newtonsoft.Json.Linq;

namespace GoogleSheetsToJsonConvertor
{
    public class Program
    {
        private static readonly string[] _scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static readonly string _applicationName = "Google Sheets API .NET Quickstart";

        public static void Main(string[] args)
        {
            args.ForEach(Go);
            Console.Write("Success");
            Console.Read();
        }

        private static void Go(string instructionsPath)
        {
            var instructionsDir = Path.GetDirectoryName(instructionsPath);
            var instructions = JObjectX.FromFile(instructionsPath);
            var propertyInstructions = ((JArray)instructions["Properties"]).Select(x => new PropertyInstruction((JObject)x)).ToList();
            var itemProcessor = new JObjectBuilder(propertyInstructions);
            var spreadsheetID = instructions.GetPropertyValue("SpreadSheetID");
            var range = instructions.GetPropertyValue("Range");
            var unprocessedItems = GetDataFromGoogleSheets(spreadsheetID, range);
            var items = unprocessedItems.Select(x => itemProcessor.Build(x)).ToList();
            var jArray = new JArray(items);
            dynamic jObject = new JObject();
            jObject.Items = jArray;
            var output = instructions.GetPropertyValue("Output");
            File.WriteAllText(PathX.Build(instructionsDir, output), jObject.ToString());
        }

        private static List<List<string>> GetDataFromGoogleSheets(string spreadsheetId, string range)
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            return values.Select(rowValue => rowValue.Select(cellValue => cellValue.ToString()).ToList()).ToList();
        }
    }
}
