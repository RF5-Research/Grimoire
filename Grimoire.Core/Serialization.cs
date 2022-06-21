using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

//namespace AssetsTools.NET.Extra
namespace Grimoire.Core
{
    public static class Serialization
    {
        [AttributeUsage(AttributeTargets.Field)]
        public sealed class SerializeField : Attribute { };

        public static void SerializeObject<T>(T value, AssetTypeValueField baseField)
        {
            WriteValue(value, baseField);
        }

        public static T DeserializeObject<T>(AssetTypeValueField baseField)
        {
            return (T)ReadValue(typeof(T), baseField);
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

            //for (var index = 0; index < array.GetChildrenCount(); index++)
            //{
            //    WriteValue(value[index], array.Get(index));
            //}
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

        private static object ReadValue(Type type, AssetTypeValueField assetTypeValueField)
        {
            if (type.IsPrimitive || type == typeof(string))
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
            object objectValue = valueField.GetValueType().ToString() switch
            {
                "Boolean" => valueField.AsBool(),
                "Byte" => valueField.AsInt(),
                "Char" => valueField.AsInt(),
                "UInt16" => valueField.AsInt(),
                "UInt32" => valueField.AsInt(),
                "UInt64" => valueField.AsInt64(),
                "SByte" => valueField.AsInt(),
                "Int16" => valueField.AsInt(),
                "Int32" => valueField.AsInt(),
                "Int64" => valueField.AsInt64(),
                "Single" => valueField.AsFloat(),
                "Double" => valueField.AsDouble(),
                "String" => valueField.AsString(),
                _ => null,
            };
            return Convert.ChangeType(objectValue, type);
        }

        private static object ReadObject(Type type, AssetTypeValueField assetTypeValueField)
        {
            var instance = Activator.CreateInstance(type);

            //The fields won't be sorted by the sequential declaration of the fields
            //But that shouldn't be relevant to this
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldType = field.FieldType;
                object fieldValue = null;
                var valueField = assetTypeValueField.Get(field.Name);

                if (fieldType.IsNotPublic && fieldType.GetCustomAttribute(typeof(SerializableAttribute)) != null)
                    fieldValue = ReadValue(fieldType, valueField);
                else
                    fieldValue = ReadValue(fieldType, valueField);

                field.SetValue(instance, fieldValue);
            }
            return instance;
        }
    }
}