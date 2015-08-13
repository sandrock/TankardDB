
namespace TankardDB.Core.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MainIndexRow
    {
        private const string idKey = "Value";
        private const string objectStoreBeginIndexKey = "Str.Beg";
        private const string objectStoreEndIndexKey = "Str.End";
        private const string isDeletedKey = "Del";

        private readonly string id;
        private readonly long? objectStoreBeginIndex;
        private readonly long? objectStoreEndIndex;
        private readonly bool? isDeleted;

        public MainIndexRow(string id, long objectStoreBeginIndex, long objectStoreEndIndex)
        {
            this.id = id;
            this.objectStoreBeginIndex = objectStoreBeginIndex;
            this.objectStoreEndIndex = objectStoreEndIndex;
        }

        public MainIndexRow(string id, bool isDeleted)
        {
            this.id = id;
            this.isDeleted = isDeleted;
        }

        public MainIndexRow(IList<Tuple<string, string>> kevaps)
        {
            long longValue;
            bool boolValue;
            foreach (var item in kevaps)
            {
                var key = item.Item1;
                var value = item.Item2;
                if (key == idKey)
                {
                    this.id = item.Item2;
                }
                else if (key == objectStoreBeginIndexKey)
                {
                    if (long.TryParse(value, out longValue))
                    {
                        this.objectStoreBeginIndex = longValue;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid value '" + value + "' for " + key);
                    }
                }
                else if (key == objectStoreEndIndexKey)
                {
                    if (long.TryParse(value, out longValue))
                    {
                        this.objectStoreEndIndex = longValue;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid value '" + value + "' for " + key);
                    }
                }
                else if (key == isDeletedKey)
                {
                    if (bool.TryParse(value, out boolValue))
                    {
                        this.isDeleted = boolValue;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid value '" + value + "' for " + key);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid key '" + key + "'");
                }
            }
        }

        public string Id
        {
            get { return this.id; }
        }

        public long? ObjectStoreBeginIndex
        {
            get { return this.objectStoreBeginIndex; }
        }

        public long? ObjectStoreEndIndex
        {
            get { return this.objectStoreEndIndex; }
        }

        public bool? IsDeleted
        {
            get { return isDeleted; }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public string ToSekvap()
        {
            var lang = new SekvapLanguage();
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("Value", this.id));
            if (this.isDeleted != null)
                values.Add(new KeyValuePair<string, string>(isDeletedKey, this.isDeleted.Value.ToString()));
            if (this.objectStoreBeginIndex != null)
                values.Add(new KeyValuePair<string, string>(objectStoreBeginIndexKey, this.objectStoreBeginIndex.Value.ToString()));
            if (this.objectStoreEndIndex != null)
                values.Add(new KeyValuePair<string, string>(objectStoreEndIndexKey, this.objectStoreEndIndex.Value.ToString()));
            return lang.Write(values);
        }
    }
}
