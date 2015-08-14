
namespace TankardDB.Core.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SekvapLanguage
    {
        public SekvapLanguage()
        {
        }

        public bool IsMatch(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return value != null;
        }

        public void ResolveVariables()
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, string>> Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (!this.IsMatch(value))
                return null;

            var result = new List<KeyValuePair<string, string>>();
            bool isStart = true, isKey = false, isValue = false, isEnd = false;
            string capturedKey = null;
            int captureStartIndex = 0, captureEndIndex, captureLength;
            for (int i = 0; i <= value.Length; i++)
            {
                char c, cp1;
                if (i == value.Length)
                {
                    c = char.MinValue;
                    cp1 = char.MinValue;
                    isEnd = true;
                    captureEndIndex = i - 1;
                    captureLength = i - captureStartIndex;
                }
                else
                {
                    c = value[i];
                    cp1 = (i+1) < value.Length ? value[i+1] : char.MinValue;
                    captureEndIndex = i;
                    captureLength = i - captureStartIndex;
                }

                if (isStart)
                {
                    if (c == ';' && cp1 == ';')
                    {
                        i++;
                    }
                    else if (c == ';' && cp1 != ';' || isEnd)
                    {
                        // end of start part
                        AddToResult(result, "Value", value.Substring(captureStartIndex, captureLength));
                        i++;
                        isStart = false;
                        isKey = true;
                        captureStartIndex = i;
                    }
                }
                else if (isKey)
                {
                    if (c == '=' && cp1 == '=')
                    {
                        i++;
                    }
                    else if (c == '=' && cp1 != '=' || isEnd)
                    {
                        // end of start part
                        capturedKey = value.Substring(captureStartIndex, captureLength);
                        i++;
                        isKey = false;
                        isValue = true;
                        captureStartIndex = i;
                    }
                }
                else if (isValue)
                {
                    if (c == ';' && cp1 == ';')
                    {
                        i++;
                    }
                    else if (c == ';' && cp1 != ';' || isEnd)
                    {
                        // end of start part
                        var capturedValue = value.Substring(captureStartIndex, captureLength);
                        AddToResult(result, capturedKey, capturedValue);
                        i++;
                        isStart = false;
                        isKey = true;
                        captureStartIndex = i;
                    }
                }
            }

            return result;
        }

        public static void AddToResult(List<KeyValuePair<string, string>> collection, string key, string value)
        {
            collection.Add(new KeyValuePair<string, string>(key, value));
        }

        public string Write(IEnumerable<KeyValuePair<string, string>> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            var sb = new StringBuilder();
            foreach (var item in values)
            {
                if (item.Key == "Value")
                {
                    sb.Insert(0, Escape(item.Value));
                }
                else
                {
                    sb.Append(";");
                    sb.Append(item.Key);
                    sb.Append("=");
                    sb.Append(Escape(item.Value));
                }
            }

            return sb.ToString();
        }

        private string Unescape(string value)
        {
            return value.Replace(";;", ";");
        }

        private string Escape(string value)
        {
            return value.Replace(";", ";;");
        }
    }
}
