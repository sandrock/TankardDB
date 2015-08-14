
namespace TankardDB.Core.Serialization
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;

    public class DefaultTankardSerializer
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            DateFormatHandling= DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.None,
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            TypeNameHandling = TypeNameHandling.All,
        };

        public DefaultTankardSerializer()
        {
        }

        internal byte[] Serialize(object item)
        {
            var json = JsonConvert.SerializeObject(item, Formatting.None, this.settings);
            var bytes = this.encoding.GetBytes(json);
            return bytes;
        }

        internal object Deserialize(byte[] serialized)
        {
            var json = this.encoding.GetString(serialized, 0, serialized.Length);
            var obj = JsonConvert.DeserializeObject(json, this.settings);
            return obj;
        }
    }
}
