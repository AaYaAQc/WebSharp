using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WebSharpJs.Script;
using WebSharpJs.DOM;

namespace GeoLocation
{
    // https://dev.w3.org/geo/api/spec-source.html#navi-geo
    public sealed class GeoLocationAPI : HtmlObject
    {
        
        static DOMObjectProxy scriptProxy;

        internal static async Task<GeoLocationAPI> Instance()
        {
            var proxy = new GeoLocationAPI();
            await proxy.Initialize();
            return proxy;
        }

        private async Task Initialize()
        {
            if (scriptProxy == null)
            {
                scriptProxy = new DOMObjectProxy("navigator.geolocation");
            }

            await scriptProxy.GetProxyObject();
            ScriptObjectProxy = scriptProxy;

        }

        public async Task GetCurrentPosition(ScriptObjectCallback<GeoPosition> success,
              ScriptObjectCallback<PositionError> error = null,
              PositionOptions options = null      )
        {
            await Invoke<GeoPosition>("getCurrentPosition", success, error, options);
        }
    }

    [ScriptableType]
    public class GeoPosition
    {
        [ScriptableMember(ScriptAlias = "coords")]
        public Coordinates Coordinates { get; private set; }
        [ScriptableMember(ScriptAlias = "timestamp")]
        public double Timestamp { get; private set; }
    }

    [ScriptableType]
    public class Coordinates
    {
        [ScriptableMember(ScriptAlias = "latitude")]
        public double Latitude { get; private set; }

        [ScriptableMember(ScriptAlias = "longitude")]
        public double Longitude { get; private set; }
        
        [ScriptableMember(ScriptAlias = "altitude")]
        public double Altitude { get; private set; }
        
        [ScriptableMember(ScriptAlias = "accuracy")]
        public double Accuracy { get; private set; }
        
        [ScriptableMember(ScriptAlias = "altitudeAccuracy")]
        public double AltitudeAccuracy { get; private set; }
        
        [ScriptableMember(ScriptAlias = "heading")]
        public double Heading { get; private set; }
        
        [ScriptableMember(ScriptAlias = "speed")]
        public double Speed { get; private set; }
    }

    [ScriptableType]
    public class PositionOptions {

        [ScriptableMember(ScriptAlias = "enableHighAccuracy")]
        public bool EnableHighAccuracy { get; set; }
        [ScriptableMember(ScriptAlias = "timeout")]
        public ulong Timeout { get; set; } = 0xFFFFFFFF;
        [ScriptableMember(ScriptAlias = "maximumAge")]
        public ulong MaximumAge { get; set; } = 0;
    }

    [ScriptableType]
    public class PositionError {

        [ScriptableMember(ScriptAlias = "code")]
        public int Code { get; private set; }
        
        public PositionErrorCode ErrorCode 
        { 
            get 
            {
                return (PositionErrorCode)Code;
            }
        }

        [ScriptableMember(ScriptAlias = "message")]
        public string Message { get; private set; }
    }

    public enum PositionErrorCode
    {
        PermissionDenied = 1,
        PositionUnAvailable = 2,
        TimeOut = 3
    }


}