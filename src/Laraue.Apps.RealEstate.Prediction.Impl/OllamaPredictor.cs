using System.Collections;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Laraue.Apps.RealEstate.Prediction.Impl;

public interface IOllamaPredictor
{
    public Task<TModel> PredictAsync<TModel>(
        string model,
        string prompt,
        string base64EncodedImage,
        CancellationToken ct = default)
        where TModel : class;
    
    public Task<TModel> PredictAsync<TModel>(
        string model,
        string prompt,
        CancellationToken ct = default)
        where TModel : class;
}

public class OllamaPredictor(HttpClient client, ILogger<OllamaPredictor> logger)
    : IOllamaPredictor
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly ConcurrentDictionary<Type, FormatGenerator.OllamaSchemaProperty?> _schemas = new ();
    
    public Task<TModel> PredictAsync<TModel>(string model, string prompt, string base64EncodedImage, CancellationToken ct = default)
        where TModel : class
    {
        return PredictInternalAsync<TModel>(model, prompt, base64EncodedImage, ct);
    }

    public Task<TModel> PredictAsync<TModel>(string model, string prompt, CancellationToken ct = default)
        where TModel : class
    {
        return PredictInternalAsync<TModel>(model, prompt, null, ct);
    }
    
    private async Task<TModel> PredictInternalAsync<TModel>(
        string model,
        string prompt,
        string? base64EncodedImage,
        CancellationToken ct = default)
        where TModel : class
    {
        // semaphore
        var schema = _schemas.GetOrAdd(typeof(TModel), type =>
        {
            var schema = FormatGenerator.GetSchema(type);
            
            logger.LogInformation(
                "Ollama schema of type {Type} is {Schema}",
                typeof(TModel),
                JsonSerializer.Serialize(schema, _options));

            return schema;
        });

        var request = new Dictionary<string, object>()
        {
            ["temperature"] = 0,
            ["model"] = model,
            ["prompt"] = prompt,
            ["stream"] = false,
            ["format"] = schema!,
        };

        if (base64EncodedImage is not null)
        {
            request["images"] = new[] { base64EncodedImage };
        }
        
        using var response = await client.PostAsJsonAsync(
            "api/generate", 
            request,
            _options,
            ct);

        try
        {
            response.EnsureSuccessStatusCode();
            var ollamaResult = await response.Content.ReadFromJsonAsync<OllamaResult>(JsonSerializerOptions.Web, ct);
            return JsonSerializer.Deserialize<TModel>(ollamaResult!.Response, JsonSerializerOptions.Web)!;
        }
        catch (Exception e)
        {
            var message = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(message, e);
        }
    }


    private class OllamaResult
    {
        public required string Response { get; set; }
    }
}

public static class FormatGenerator
{
    public static OllamaSchemaProperty GetSchema(Type outputType)
    {
        var propertyType = GetOllamaType(outputType);

        return propertyType switch
        {
            OllamaSchemaPropertyType.Object => GetObjectSchema(outputType),
            OllamaSchemaPropertyType.Array => GetArraySchema(outputType),
            _ => new OllamaSchemaProperty
            {
                Type = [propertyType]
            }
        };
    }

    private static OllamaSchemaObjectProperty GetObjectSchema(Type outputType)
    {
        var resultProperties = new Dictionary<string, OllamaSchemaProperty>();
        
        var properties = outputType.GetProperties();
        foreach (var property in properties)
        {
            resultProperties.Add(property.Name, GetSchema(property.PropertyType));
        }

        return new OllamaSchemaObjectProperty
        {
            Properties = resultProperties,
            Type = [OllamaSchemaPropertyType.Object]
        };
    }
    
    private static OllamaSchemaArrayProperty GetArraySchema(Type outputType)
    {
        var elementClrType = outputType.GetElementType() ?? throw new InvalidOperationException();
        var elementType =  GetOllamaType(elementClrType);

        switch (elementType)
        {
            case OllamaSchemaPropertyType.Object:
            {
                var schema = GetObjectSchema(elementClrType);
                return new OllamaSchemaArrayProperty
                {
                    Items = new OllamaSchemaArrayItem
                    {
                        Type = [elementType],
                        Properties = schema.Properties,
                    },
                    Type = [OllamaSchemaPropertyType.Array]
                };
            }
            case OllamaSchemaPropertyType.Array:
                return new OllamaSchemaArrayProperty
                {
                    Items = new OllamaSchemaArrayItem
                    {
                        Type = [OllamaSchemaPropertyType.Array],
                    },
                    Type = [OllamaSchemaPropertyType.Array]
                };
            default:
                return new OllamaSchemaArrayProperty
                {
                    Items = new OllamaSchemaArrayItem
                    {
                        Type = [elementType],
                    },
                    Type = [OllamaSchemaPropertyType.Array]
                };
        }
    }
    
    private static OllamaSchemaPropertyType GetOllamaType(Type type)
    {
        if (type == typeof(string))
        {
            return OllamaSchemaPropertyType.String;
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return OllamaSchemaPropertyType.Number;
        }

        if (type == typeof(bool))
        {
            return OllamaSchemaPropertyType.Boolean;
        }

        if (type == typeof(DateTime))
        {
            return OllamaSchemaPropertyType.String;
        }

        if (type.IsArray || typeof(IList).IsAssignableFrom(type))
        {
            return OllamaSchemaPropertyType.Array;
        }

        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            return OllamaSchemaPropertyType.Object;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return OllamaSchemaPropertyType.Array;
        }

        return OllamaSchemaPropertyType.Object;
    }
    
    [JsonDerivedType(typeof(OllamaSchemaObjectProperty))]
    [JsonDerivedType(typeof(OllamaSchemaArrayProperty))]
    public class OllamaSchemaProperty
    {
        [JsonPropertyName("type")]
        public required OllamaSchemaPropertyType[] Type { get; init; }
    }
    
    public class OllamaSchemaObjectProperty : OllamaSchemaProperty
    {
        [JsonPropertyName("properties")]
        public required Dictionary<string, OllamaSchemaProperty> Properties { get; init; }
    }
    
    public class OllamaSchemaArrayProperty : OllamaSchemaProperty
    {
        public OllamaSchemaArrayProperty()
        {
            Type = [OllamaSchemaPropertyType.Array];
        }
        
        [JsonPropertyName("items")]
        public required OllamaSchemaArrayItem Items { get; init; }
    }

    public class OllamaSchemaArrayItem
    {
        [JsonPropertyName("type")]
        public required OllamaSchemaPropertyType[] Type { get; init; }
        
        [JsonPropertyName("properties")]
        public Dictionary<string, OllamaSchemaProperty>? Properties { get; init; }
    }

    public enum OllamaSchemaPropertyType
    {
        Null,
        Boolean,
        Number,
        String,
        Array,
        Object,
    }
}