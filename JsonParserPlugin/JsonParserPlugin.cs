using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rainmeter;

namespace JsonParserPlugin
{
    class Measure
    {
        public string result;
        API api;

        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        public IntPtr buffer = IntPtr.Zero;

        public double Update()
        {
            var source = api.ReadString("Source", "");
            var query = api.ReadString("Query", "");

            api.Log(API.LogType.Debug, $"Source: {source}");
            api.Log(API.LogType.Debug, $"Query: {query}");

            result = Parse(source, query);

            double numericResult;
            var isNumeric = double.TryParse(result, out numericResult);
            return isNumeric ? numericResult : 0.0;
        }
        public void Reload(API rm)
        {
            this.api = rm;
        }

        public string Parse(string source, string query)
        {
            var result = "";
            try
            {
                var json = JToken.Parse(source);
                var token = json.SelectToken(query);
                if (token == null)
                {
                    api.Log(API.LogType.Warning, "Query did not match any token");
                }
                else
                {
                    if (token is JValue)
                    {
                        result = ((JValue)token).Value.ToString();
                    }
                    else
                    {
                        result = token.ToString(Formatting.None);
                    }
                }

            }
            catch (JsonReaderException e)
            {
                api.Log(API.LogType.Error, e.Message);
            }
            api.Log(API.LogType.Debug, $"Query result string value: {result}");
            return result;
        }
    }

    public class Plugin
    {

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)data;
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
            measure.Reload(rm);
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
    }
}

