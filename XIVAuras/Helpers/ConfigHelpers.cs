﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XIVAuras.Auras;
using XIVAuras.Config;

namespace XIVAuras.Helpers
{
    public static class ConfigHelpers
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            SerializationBinder = new XIVAurasSerializationBinder()
        };

        public static void ExportToClipboard<T>(T toExport)
        {
            string? exportString = ConfigHelpers.GetExportString(toExport);

            if (exportString is not null)
            {
                ImGui.SetClipboardText(exportString);
                DrawHelpers.DrawNotification("Export string copied to clipboard.");
            }
            else
            {
                DrawHelpers.DrawNotification("Failed to Export!", NotificationType.Error);
            }
        }

        public static string? GetExportString<T>(T toExport)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(toExport, Formatting.None, _serializerSettings);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (DeflateStream compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal))
                    {
                        using (StreamWriter writer = new StreamWriter(compressionStream, Encoding.UTF8))
                        {
                            writer.Write(jsonString);
                        }
                    }

                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }

            return null;
        }

        public static T? GetFromImportString<T>(string importString)
        {
            if (string.IsNullOrEmpty(importString)) return default;

            try
            {
                byte[] bytes = Convert.FromBase64String(importString);

                string decodedJsonString;
                using (MemoryStream inputStream = new MemoryStream(bytes))
                {
                    using (DeflateStream compressionStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(compressionStream, Encoding.UTF8))
                        {
                            decodedJsonString = reader.ReadToEnd();
                        }
                    }
                }

                T? importedObj = JsonConvert.DeserializeObject<T>(decodedJsonString, _serializerSettings);
                return importedObj;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }

            return default;
        }

        public static XIVAurasConfig LoadConfig(string path)
        {
            XIVAurasConfig? config = null;

            try
            {
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<XIVAurasConfig>(jsonString, _serializerSettings);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());

                string backupPath = $"{path}.bak";
                if (File.Exists(path))
                {
                    try
                    {
                        File.Copy(path, backupPath);
                        PluginLog.Information($"Backed up LMeter config to '{backupPath}'.");
                    }
                    catch
                    {
                        PluginLog.Warning($"Unable to back up LMeter config.");
                    }
                }
            }

            return config ?? new XIVAurasConfig();
        }

        public static void SaveConfig()
        {
            ConfigHelpers.SaveConfig(Singletons.Get<XIVAurasConfig>());
        }

        public static void SaveConfig(XIVAurasConfig config)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented, _serializerSettings);
                File.WriteAllText(Plugin.ConfigFilePath, jsonString);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
            }
        }
    }
    
    public class ComboIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Write not supported.");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsArray ||
                objectType.GetElementType() != typeof(uint) ||
                reader.TokenType != JsonToken.Integer)
            {
                return serializer.Deserialize(reader, objectType);
            }
            
            try
            {
                uint? value = (uint?)serializer.Deserialize(reader, typeof(uint));
                if (value.HasValue)
                {
                    return SpellHelpers.GetComboIds(value.Value);
                }
            }
            catch
            {
            }

            return Array.Empty<uint>();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }

    public class ArrayConvertor<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Write not supported.");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (!objectType.IsArray ||
                objectType.GetElementType() != typeof(T))
            {
                return serializer.Deserialize(reader, objectType);
            }
            
            try
            {
                T? value = (T?)serializer.Deserialize(reader, typeof(T));
                if (value is not null && value.GetType() == typeof(T))
                {
                    return new T[] { value };
                }
            }
            catch
            {
            }

            return Array.Empty<T>();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }

    /// <summary>
    /// Because the game blocks the json serializer from loading assemblies at runtime, we define
    /// a custom SerializationBinder to ignore the assembly name for the types defined by this plugin.
    /// </summary>
    public class XIVAurasSerializationBinder : ISerializationBinder
    {
        // FIXME: Make this automatic somehow
        private static List<Type> _configTypes = new List<Type>()
        {
            typeof(AuraBar),
            typeof(AuraGroup),
            typeof(AuraIcon),
            typeof(AuraLabel),
            typeof(AuraListItem),
            typeof(AuraListConfig),
            typeof(BarStyleConfig),
            typeof(CooldownTrigger),
            typeof(ConfigColor),
            typeof(FontConfig),
            typeof(FontData),
            typeof(IconStyleConfig),
            typeof(LabelStyleConfig),
            typeof(StatusTrigger),
            typeof(TriggerConfig),
            typeof(TriggerData),
            typeof(TriggerOptions),
            typeof(VisibilityConfig),
            typeof(XIVAurasConfig)
        };

        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();

        public XIVAurasSerializationBinder()
        {
            foreach (Type type in _configTypes)
            {
                if (type.FullName is not null)
                {
                    this.typeToName.Add(type, type.FullName);
                    this.nameToType.Add(type.FullName, type);
                }
            }
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            if (this.typeToName.TryGetValue(serializedType, out string? name))
            {
                assemblyName = null;
                typeName = name;
            }
            else
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;
            }
        }

        public Type BindToType(string? assemblyName, string? typeName)
        {
            if (typeName is not null &&
                this.nameToType.TryGetValue(typeName, out Type? type))
            {
                return type;
            }

            return Type.GetType($"{typeName}, {assemblyName}", true) ??
                throw new TypeLoadException($"Unable to load type '{typeName}' from assembly '{assemblyName}'");
        }
    }
}
