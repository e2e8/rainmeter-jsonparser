using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rainmeter;

namespace JsonParserPlugin
{
    class Measure
    {
        public API api;

        public string source = "";
        public string query = "";

        public string result;
        public double numeric;

        public IntPtr buffer = IntPtr.Zero;

        private bool logNoMatch = true;
        private bool logEmptySource = true;

        private string locale = "";

        public Measure(API api)
        {
            this.api = api;
        }

        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }

        public double Update()
        {
            return numeric;
        }

        public void Reload(API api, double maxValue)
        {
            this.api = api;
            ConfigureLogging(api.ReadString("Logging", ""));

            ConfigureLocale(api.ReadString("Locale", ""));

            source = api.ReadString("Source", "");
            query = api.ReadString("Query", "");

            api.Log(API.LogType.Debug, $"Source: {source}");
            api.Log(API.LogType.Debug, $"Query: {query}");

            result = Parse(source, query);
            numeric = double.TryParse(result, out double numericResult) ? numericResult : 0.0;
        }

        public delegate string ResultFunc(JToken x);

        public string StringResult(JToken token)
        {
            if (token is JValue value)
            {
                return value.ToString(new CultureInfo(locale));
            }
            else
            {
                return token.ToString(Formatting.None);
            }
        }

        public string LengthResult(JToken token)
        {
            if (token is JArray)
            {
                return ((JArray)token).Count.ToString();
            }
            else
            {
                api.Log(API.LogType.Debug, "Length: not an Array");
                return "1";
            }
        }

        public string Parse(string source, string query)
        {
            return Parse(source, query, StringResult);
        }

        public string Parse(string source, string query, ResultFunc f)
        {
            var result = "";
            try
            {
                var jsonReader = new JsonTextReader(new StringReader(source))
                {
                    DateParseHandling = DateParseHandling.None
                };
                var json = JToken.Load(jsonReader);

                var token = json.SelectToken(query);
                if (token == null)
                {
                    if (logNoMatch)
                    {
                        api.Log(API.LogType.Warning, "Query did not match any token");
                    }
                }
                else
                {
                    result = f(token);
                }
            }
            catch (JsonReaderException e)
            {
                if (!string.IsNullOrEmpty(source) || logEmptySource)
                {
                    api.Log(API.LogType.Error, e.Message);
                }
            }
            catch (JsonException e)
            {
                api.Log(API.LogType.Error, e.Message);
            }
            api.Log(API.LogType.Debug, $"Result: {result}");
            return result;
        }

        private void ConfigureLogging(string loggingConf)
        {
            api.Log(API.LogType.Debug, $"Log Config: {loggingConf}");

            if (!string.IsNullOrEmpty(loggingConf))
            {
                var configs = loggingConf.Split(',');
                foreach (var config in configs)
                {
                    var temp = config.Trim().ToLower();
                    var parts = temp.Split(':');
                    if (parts.Length == 2 && (parts[1].Equals("0") || parts[1].Equals("1")))
                    {
                        switch (parts[0])
                        {
                            case "nomatch": logNoMatch = parts[1].Equals("1") ? true : false; break;
                            case "emptysource": logEmptySource = parts[1].Equals("1") ? true : false; break;
                            default: api.Log(API.LogType.Debug, $"Log Config: Invalid Option: {parts[0]}"); break;
                        }
                    }
                    else
                    {
                        api.Log(API.LogType.Debug, $"Log Config: Invalid Config: {config}");
                    }
                }
            }
            else
            {
                logNoMatch = true;
                logEmptySource = true;
            }
            api.Log(API.LogType.Debug, $"Log Config: Log NoMatch: {logNoMatch}");
            api.Log(API.LogType.Debug, $"Log Config: Log EmptySource: {logEmptySource}");
        }

        private void ConfigureLocale(string locale)
        {
            api.Log(API.LogType.Debug, $"Locale Config: {locale}");

            if (!string.IsNullOrEmpty(locale))
            {
                try
                {
                    var _ = new CultureInfo(locale);
                    this.locale = locale;
                    api.Log(API.LogType.Debug, $"Locale Set.");
                }
                catch (ArgumentException e)
                {
                    this.locale = "";
                    api.Log(API.LogType.Debug, $"Locale Invalid: {e.Message}");
                }
            }
            else
            {
                this.locale = "";
            }
        }
    }

    public class Plugin
    {

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(rm)));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = data;
            measure.Reload(rm, maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = data;
            return measure.Update();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            measure.buffer = Marshal.StringToHGlobalUni(measure.result);
            return measure.buffer;
        }

        [DllExport]
        public static IntPtr Query(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = data;
            string query;
            string source;

            if (argc == 1)
            {
                source = measure.source;
                query = argv[0];
            }
            else if (argc == 2)
            {
                query = argv[0];
                source = argv[1];
            }
            else
            {
                measure.api.Log(API.LogType.Error, $"Section Variable Query: Invalid arg list: {string.Join(",", argv)}");
                return IntPtr.Zero;
            }

            measure.api.Log(API.LogType.Debug, $"Section Variable Query");
            measure.api.Log(API.LogType.Debug, $"Source: {source}");
            measure.api.Log(API.LogType.Debug, $"Query: {query}");

            var result = measure.Parse(source, query);

            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            measure.buffer = Marshal.StringToHGlobalUni(result);
            return measure.buffer;
        }

        [DllExport]
        public static IntPtr Length(IntPtr data, int argc,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = data;
            string query;
            string source;

            if (argc == 1)
            {
                source = measure.source;
                query = argv[0];
            }
            else if (argc == 2)
            {
                query = argv[0];
                source = argv[1];
            }
            else
            {
                measure.api.Log(API.LogType.Error, $"Section Variable Length: Invalid arg list: {string.Join(",", argv)}");
                return IntPtr.Zero;
            }

            measure.api.Log(API.LogType.Debug, $"Section Variable Length");
            measure.api.Log(API.LogType.Debug, $"Source: {source}");
            measure.api.Log(API.LogType.Debug, $"Query: {query}");

            var result = measure.Parse(source, query, measure.LengthResult);

            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }

            measure.buffer = Marshal.StringToHGlobalUni(result);
            return measure.buffer;
        }
    }
}

