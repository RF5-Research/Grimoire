using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

//This will be PR'd into AssetTools
//namespace AssetsTools.NET.Extra
namespace Grimoire.Core
{
    public static class Serialization
    {
        public class Attributes
        {
            [AttributeUsage(AttributeTargets.Field)]
            public sealed class SerializeField : Attribute { };
        }

        public static void SerializeObject<T>(T value, AssetTypeValueField baseField)
        {
            WriteValue(value, baseField);
        }

        public static async void SerializeObjectAsync<T>(T value, AssetTypeValueField baseField)
        {
            await Task.Run(() => SerializeObject(value, baseField));
        }

        private static void WriteValue(object value, AssetTypeValueField assetTypeValueField)
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
                    WriteList((IList)value, assetTypeValueField);
                else
                    WriteObject(value, assetTypeValueField);
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

        private static void WriteList(IList value, AssetTypeValueField assetTypeValueField)
        {
            var array = assetTypeValueField.Get("Array");

            var list = new List<AssetTypeValueField>();
            foreach (var item in value)
            {
                var itemValueField = ValueBuilder.DefaultValueFieldFromArrayTemplate(array);
                WriteValue(item, itemValueField);
                list.Add(itemValueField);
            }
            array.SetChildrenList(list.ToArray());
        }

        private static void WriteObject(object value, AssetTypeValueField assetTypeValueField)
        {
            var type = value.GetType();

            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldType = field.FieldType;
                object fieldValue = field.GetValue(value);
                var valueField = assetTypeValueField.Get(field.Name);

                if (fieldType.IsNotPublic)
                {
                    var serializableAttr = fieldType.GetCustomAttribute(typeof(SerializableAttribute));
                    if (serializableAttr != null)
                        WriteValue(fieldValue, valueField);
                }
                else
                    WriteValue(fieldValue, valueField);
            }
        }


        public static T DeserializeObject<T>(AssetTypeValueField baseField)
        {
            return (T)ReadValue(typeof(T), baseField);
        }

        public static async Task<T> DeserializeObjectAsync<T>(AssetTypeValueField baseField)
        {
            return await Task.Run(() => DeserializeObject<T>(baseField));
        }


        private static object ReadValue(Type type, AssetTypeValueField assetTypeValueField)
        {
            if (type.IsPrimitive || typeof(string) == type)
                return ReadPrimitive(type, assetTypeValueField);
            else if (typeof(IList).IsAssignableFrom(type))
                return ReadList(type, assetTypeValueField);
            //Dictionaries are not supported by Unity
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return null;
            else
                return ReadObject(type, assetTypeValueField);
        }

        private static IList ReadList(Type type, AssetTypeValueField assetTypeValueField)
        {
            IList ilist;
            Type elementType;
            var array = assetTypeValueField.Get("Array");
            var count = array.GetChildrenCount();

            if (type.IsArray)
            {
                elementType = type.GetElementType();
                ilist = Array.CreateInstance(elementType, count);
            }
            else
            {
                ilist = (IList)Activator.CreateInstance(type);
                elementType = type.GetGenericArguments().Single();
            }

            for (int index = 0; index < count; index++)
            {
                var value = ReadValue(elementType, array.Get(index));

                if (ilist.IsFixedSize)
                    ilist[index] = value;
                else
                    ilist.Add(value);
            }
            return ilist;
        }

        private static object ReadPrimitive(Type type, AssetTypeValueField assetTypeValueField)
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

        private static object ReadObject(Type type, AssetTypeValueField assetTypeValueField)
        {
            var instance = Activator.CreateInstance(type);

            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var childField in assetTypeValueField.GetChildrenList())
            {
                FieldInfo fieldInfo = null;
                foreach (var item in fieldInfos)
                {
                    if (item.Name == childField.GetName())
                    {
                        fieldInfo = item;
                        break;
                    }
                }

                //Asset Field doesn't exist
                if (fieldInfo != null)
                {
                    var fieldType = fieldInfo.FieldType;
                    object fieldValue;

                    if (fieldType.IsNotPublic && fieldType.GetCustomAttribute(typeof(SerializableAttribute)) != null)
                        fieldValue = ReadValue(fieldType, childField);
                    else
                        fieldValue = ReadValue(fieldType, childField);

                    fieldInfo.SetValue(instance, fieldValue);
                }
            }
            return instance;
        }
    }
}