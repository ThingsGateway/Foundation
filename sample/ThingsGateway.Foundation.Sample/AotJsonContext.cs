using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using ThingsGateway.Foundation.Common;

namespace ThingsGateway.Foundation.Sample
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
    [JsonSerializable(typeof(MachineInfo))]
    internal partial class AotJsonContext : JsonSerializerContext
    {
        static AotJsonContext()
        {
            // replace default context
            Default = new AotJsonContext(CreateJsonSerializerOptions(Default));
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions(AotJsonContext defaultContext)
        {
            var options = new JsonSerializerOptions(defaultContext.GeneratedSerializerOptions!)
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return options;
        }
    }
}
