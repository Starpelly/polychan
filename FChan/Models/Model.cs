using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FChan.Models;

public class Model
{
    [JsonIgnore]
    public string OriginalJson = string.Empty;
}

public class ModelConverter : JsonConverter
{
    public override bool CanWrite => false;
    
    public override bool CanConvert(Type objectType)
    {
        return typeof(Model).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Load the raw JSON
        var jo = JObject.Load(reader);

        // Deserialize normally
        var result = Activator.CreateInstance(objectType) as Model;

        // Populate properties WITHOUT calling this converter again
        var tempSerializer = new JsonSerializer();
        foreach (var conv in serializer.Converters)
            if (conv != this)
                tempSerializer.Converters.Add(conv);

        tempSerializer.Populate(jo.CreateReader(), result);

        // Store original JSON
        result.OriginalJson = jo.ToString(Formatting.None);

        // Recursively store original JSON in nested Model properties
        SetNestedOriginalJson(result);

        return result;
    }
    
    private void SetNestedOriginalJson(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();

        // Only process properties
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var value = prop.GetValue(obj);

            if (value is Model nestedModel)
            {
                // Already populated OriginalJson from parent JSON
                // But recalc in case nested property is object within JSON
                var json = JsonConvert.SerializeObject(nestedModel);
                nestedModel.OriginalJson = json;

                // Recurse
                SetNestedOriginalJson(nestedModel);
            }
            else if (value is IEnumerable<Model> enumerable)
            {
                foreach (var item in enumerable)
                {
                    var json = JsonConvert.SerializeObject(item);
                    item.OriginalJson = json;
                    SetNestedOriginalJson(item);
                }
            }
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException(); // Never called
    }
}