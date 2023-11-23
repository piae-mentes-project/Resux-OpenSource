using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AnimationCurveConvertor : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(AnimationCurve);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        AnimationCurve curve = serializer.Deserialize<SerializableAnimationCurve>(reader);
        return curve;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var curve = new SerializableAnimationCurve(value as AnimationCurve);
        serializer.Serialize(writer, curve);
    }
}

public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = Vector2.zero;
        var jobj = JObject.Load(reader);
        value.x = jobj.Value<float>("x");
        value.y = jobj.Value<float>("y");
        return value;
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WriteEndObject();
    }
}
