using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

//This will be PR'd into AssetTools
//namespace AssetsTools.NET.Extra
namespace Grimoire
{
    public static class Serialization
    {
        public class Attributes
        {
            public enum Target
            {
                Field,
                Property
            }

            //Add assemblyName and serialize
            //NonSerialized attr
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
            public sealed class SerializableAttribute : Attribute
            {
                internal Target Target { get; set; }
                internal string? AssemblyName { get; set; }
                public SerializableAttribute(Target target = Target.Field, string? assemblyName = null)
                {
                    Target = target;
                    AssemblyName = assemblyName;
                }
            };

            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
            public sealed class SerializeFieldAttribute : Attribute
            {
                internal readonly string? Name;
                public SerializeFieldAttribute(string? name = null, bool serialize = true)
                {
                    Name = name;
                }
            };
        }

        public static void SerializeObject<T>(T value, AssetsManager am, AssetTypeValueField baseField, AssetsFileInstance? fileInstance = null)
        {
            WriteValue(value, am, baseField, fileInstance);
        }

        private static void WriteValue(object value, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //Remember that some members are not initialized from deserialization, so it's a null object
            if (value != null)
            {
                var type = value.GetType();
                if (type.IsPrimitive || type == typeof(string))
                    WritePrimitive(value, assetTypeValueField);
                //Dictionaries are not supported by Unity
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return;
                else if (typeof(IList).IsAssignableFrom(type))
                    WriteList((IList)value, am, assetTypeValueField, fileInstance);
                else
                    WriteObject(value, am, assetTypeValueField, fileInstance);
            }
        }

        private static void WritePrimitive(object value, AssetTypeValueField assetTypeValueField)
        {
            var type = value.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.String:
                    assetTypeValueField.GetValue().Set(value);
                    return;
                default: return;
            }
        }

        private static void WriteList(IList value, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            var array = assetTypeValueField.Get("Array");

            var list = new List<AssetTypeValueField>();
            foreach (var item in value)
            {
                var itemValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(array);
                WriteValue(item, am, itemValueField, fileInstance);
                list.Add(itemValueField);
            }
            array.SetChildrenList(list.ToArray());
        }

        public static T? DeserializeObject<T>(AssetsManager am, AssetTypeValueField baseField, AssetsFileInstance? fileInstance = null)
        {
            return (T)ReadValue(am, typeof(T), baseField, fileInstance)!;
        }

        private static object? ReadValue(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            if (type.IsPrimitive || typeof(string) == type)
                return ReadPrimitive(type, assetTypeValueField);
            else if (typeof(IList).IsAssignableFrom(type))
                return ReadList(am, type, assetTypeValueField, fileInstance);
            //Dictionaries are not supported by Unity
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return null;
            else
                return ReadObject(am, type, assetTypeValueField, fileInstance);
        }

        private static object? ReadPrimitive(Type type, AssetTypeValueField assetTypeValueField)
        {
            var valueField = assetTypeValueField.GetValue();
            switch (valueField.GetValueType())
            {
                case EnumValueTypes.Bool: return Convert.ChangeType(valueField.AsBool(), type);
                case EnumValueTypes.UInt8: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.UInt16: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.UInt32: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.UInt64: return Convert.ChangeType(valueField.AsInt64(), type);
                case EnumValueTypes.Int8: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.Int16: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.Int32: return Convert.ChangeType(valueField.AsInt(), type);
                case EnumValueTypes.Int64: return Convert.ChangeType(valueField.AsInt64(), type);
                case EnumValueTypes.Float: return Convert.ChangeType(valueField.AsFloat(), type);
                case EnumValueTypes.Double: return Convert.ChangeType(valueField.AsDouble(), type);
                case EnumValueTypes.String: return valueField.AsString();
                default: return null;
            };
        }

        private static IList ReadList(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            IList ilist;
            Type elementType;
            var array = assetTypeValueField.Get("Array");
            var count = array.GetChildrenCount();

            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
                ilist = Array.CreateInstance(elementType, count);
            }
            else
            {
                ilist = (IList)Activator.CreateInstance(type)!;
                elementType = type.GetGenericArguments().Single();
            }

            for (int index = 0; index < count; index++)
            {
                var value = ReadValue(am, elementType, array.Get(index), fileInstance);

                if (ilist.IsFixedSize)
                    ilist[index] = value;
                else
                    ilist.Add(value);
            }
            return ilist;
        }

        private static void WriteObject(object value, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            var type = value.GetType();
            //Do user-defined serialization
            if (value is ISerialization)
            {
                ((ISerialization)value).Serialize(am, assetTypeValueField, fileInstance);
                return;
            }

            var serializable = (Attributes.SerializableAttribute?)type.GetCustomAttribute(typeof(Attributes.SerializableAttribute));

            if (serializable != null)
            {
                if (serializable.Target == Attributes.Target.Field)
                    WriteFields(value, type, am, assetTypeValueField, fileInstance);
                else
                    WriteProperties(value, type, am, assetTypeValueField, fileInstance);
            }
            else
            {
                //Default
                WriteFields(value, type, am, assetTypeValueField, fileInstance);
            }
        }

        private static void WriteProperties(object value, Type type, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var propertyType = propertyInfo.PropertyType;
                object? propertyValue = propertyInfo.GetValue(value);
                var valueField = assetTypeValueField.Get(propertyInfo.Name);


                if (propertyValue != null && (
                    (propertyType.IsNotPublic && propertyType.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute)) != null) ||
                    (!propertyType.IsNotPublic && propertyValue != null)))
                    SerializeMember(propertyValue, am, valueField, fileInstance);
            }
        }

        private static void WriteFields(object value, Type type, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldType = fieldInfo.FieldType;
                object? fieldValue = fieldInfo.GetValue(value);
                var valueField = assetTypeValueField.Get(fieldInfo.Name);

                ////Check for user-defined names
                //var serializeField = (Attributes.SerializeFieldAttribute?)fieldType.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute));
                //if (serializeField != null && serializeField.Name != null)
                //{
                //    var assetValueField = assetTypeValueField.Get(serializeField.Name);
                //    if (serializeField.Name != null)
                //    {
                //        valueField = assetValueField;
                //        break;
                //    }
                //}
                //else
                //{
                //    valueField = assetTypeValueField.Get(fieldInfo.Name);
                //}

                if (fieldValue != null && (
                    (fieldType.IsNotPublic && fieldType.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute)) != null) ||
                    (!fieldType.IsNotPublic && fieldValue != null)))
                    SerializeMember(fieldValue, am, valueField, fileInstance);
            }
        }

        private static void SerializeMember(object value, AssetsManager am, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //If PPtr
            //Some have $ and some don't?
            var match = Regex.Match(assetTypeValueField.GetFieldType(), @"(?:PPtr)(?:\<)(?:\$?)([^\>]+)(?:\>)");
            if (match.Success)
            {
                //Check loaded files relative to file
                if (fileInstance != null)
                {
                    var extAsset = am.GetExtAsset(fileInstance, assetTypeValueField);
                    if (extAsset.file != null && extAsset.info != null)
                    {
                        var baseField = extAsset.instance.GetBaseField();
                        WriteValue(value, am, baseField, extAsset.file);
                        //Write always
                        WriteExternalAsset(extAsset, baseField);
                    }
                }
            }
            else
            {
                WriteValue(value, am, assetTypeValueField, fileInstance);
            }
        }

        private static void WriteExternalAsset(AssetExternal extAsset, AssetTypeValueField baseField)
        {
            var assetBytes = baseField.WriteToByteArray();
            var repl = new AssetsReplacerFromMemory(0, extAsset.info.index, (int)extAsset.info.curFileType, AssetHelper.GetScriptIndex(extAsset.file.file, extAsset.info), assetBytes);

            byte[] newAssetData;
            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                extAsset.file.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
                newAssetData = stream.ToArray();
            }
            var bunRepl = new BundleReplacerFromMemory(extAsset.file.name, null, true, newAssetData, -1);

            //TODO: Figure out how to not couple the export function this with this class
            var bundle = extAsset.file.parentBundle;
            using (var bunWriter = new AssetsFileWriter(File.Create(PathUtilities.GetExportPath(bundle.path))))
            {
                bundle.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });
            }
        }

        private static object? ReadObject(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            var instance = Activator.CreateInstance(type);

            //Do user-defined serialization
            if (instance is ISerialization)
            {
                ((ISerialization)instance).Deserialize(am, type, assetTypeValueField, fileInstance);
                return instance;
            }

            var serializable = (Attributes.SerializableAttribute?)type.GetCustomAttribute(typeof(Attributes.SerializableAttribute));
            if (serializable != null)
            {
                if (serializable.Target == Attributes.Target.Field)
                    ReadFields(instance, am, type, assetTypeValueField, fileInstance);
                else
                    ReadProperties(instance, am, type, assetTypeValueField, fileInstance);
            }
            else
            {
                //Default
                ReadFields(instance, am, type, assetTypeValueField, fileInstance);
            }
            return instance;
        }

        private static void ReadFields(object? instance, AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var childField in assetTypeValueField.GetChildrenList())
            {
                FieldInfo? fieldInfo = null;
                var targetName = childField.GetName();

                foreach (var member in fieldInfos)
                {
                    if (member.Name == targetName)
                    {
                        fieldInfo = member;
                        break;
                    }

                    //Check for user-defined names
                    var serializeField = (Attributes.SerializeFieldAttribute?)member.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute));
                    if (serializeField != null && serializeField.Name != null)
                    {
                        if (serializeField.Name == targetName)
                        {
                            fieldInfo = member;
                            break;
                        }
                    }
                    else
                    {
                        if (member.Name == targetName)
                        {
                            fieldInfo = member;
                            break;
                        }
                    }
                }

                //Asset Field exist
                if (fieldInfo != null)
                {
                    var fieldType = fieldInfo.FieldType;
                    object? fieldValue = null;

                    if ((fieldType.IsNotPublic && fieldType.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute)) != null) ||
                        !fieldType.IsNotPublic)
                        fieldValue = DeserializeMember(am, fieldType, childField, fileInstance);
                    
                    fieldInfo.SetValue(instance, fieldValue);
                }
            }
        }

        private static void ReadProperties(object? instance, AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var childField in assetTypeValueField.GetChildrenList())
            {
                PropertyInfo? propertyInfo = null;
                var targetName = childField.GetName();

                foreach (var member in propertyInfos)
                {
                    //Check for user-defined names
                    var serializeField = (Attributes.SerializeFieldAttribute?)member.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute));
                    if (serializeField != null && serializeField.Name != null)
                    {
                        if (serializeField.Name == targetName)
                        {
                            propertyInfo = member;
                            break;
                        }
                    }
                    else
                    {
                        if (member.Name == targetName)
                        {
                            propertyInfo = member;
                            break;
                        }
                    }
                }

                //Asset Field exist
                if (propertyInfo != null)
                {
                    var propertyType = propertyInfo.PropertyType;
                    object? propertyValue = null;
                    if ((propertyType.IsNotPublic && propertyType.GetCustomAttribute(typeof(Attributes.SerializeFieldAttribute)) != null) ||
                        !propertyType.IsNotPublic)
                        propertyValue = DeserializeMember(am, propertyType, childField, fileInstance);
                    propertyInfo.SetValue(instance, propertyValue);
                }
            }
        }

        private static object? DeserializeMember(AssetsManager am, Type type, AssetTypeValueField assetTypeValueField, AssetsFileInstance? fileInstance = null)
        {
            //If PPtr
            //Some have $ and some don't?
            object? instance = null;
            var match = Regex.Match(assetTypeValueField.GetFieldType(), @"(?:PPtr)(?:\<)(?:\$?)([^\>]+)(?:\>)");
            if (match.Success)
            {
                //Check loaded files relative to file
                if (fileInstance != null)
                {
                    var extAsset = am.GetExtAsset(fileInstance, assetTypeValueField);
                    if (extAsset.file != null && extAsset.info != null)
                    {
                        instance = ReadValue(am, type, extAsset.instance.GetBaseField(), extAsset.file);
                    }
                }
            }
            else
            {
                instance = ReadValue(am, type, assetTypeValueField, fileInstance);
            }
            return instance;
        }
    }
}