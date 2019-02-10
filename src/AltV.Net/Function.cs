using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AltV.Net.Elements.Entities;
using AltV.Net.Native;

namespace AltV.Net
{
    public partial class Function
    {
        public delegate object Func(params object[] args);

        private class FuncWrapper
        {
            private readonly MValue.Function function;

            public FuncWrapper(MValue.Function function)
            {
                this.function = function;
            }

            public object Call(params object[] args)
            {
                var result = MValue.Nil;
                var mValueArgs = MValue.Create((from obj in args
                    select MValue.CreateFromObject(obj)
                    into mValue
                    where mValue.HasValue
                    select mValue.Value).ToArray());
                function(ref mValueArgs, ref result);
                return result.ToObject(Alt.Module.BaseEntityPool);
            }
        }

        private class TypeInfo
        {
            public readonly bool IsEntity;

            public readonly bool IsVehicle;

            public readonly bool IsPlayer;

            public readonly bool IsBlip;

            public readonly bool IsCheckpoint;

            public readonly bool IsDict;

            public readonly bool IsList;

            public readonly TypeInfo Element;

            public readonly Type ElementType;

            public readonly TypeInfo DictionaryValue;

            public readonly Type[] GenericArguments;

            public readonly Type DictType;

            public readonly Func<System.Collections.IDictionary> CreateDictionary;

            public TypeInfo(Type type)
            {
                IsList = type.BaseType == Array;
                IsDict = type.Name.StartsWith("Dictionary");
                if (IsDict)
                {
                    GenericArguments = type.GetGenericArguments();
                    DictType = typeof(Dictionary<,>).MakeGenericType(GenericArguments[0], GenericArguments[1]);
                    DictionaryValue = GenericArguments.Length == 2 ? new TypeInfo(GenericArguments[1]) : null;
                    CreateDictionary = Expression.Lambda<Func<System.Collections.IDictionary>>(
                        Expression.New(DictType)
                    ).Compile();
                }
                else
                {
                    GenericArguments = null;
                    DictType = null;
                    DictionaryValue = null;
                    CreateDictionary = null;
                }

                var interfaces = type.GetInterfaces();
                if (interfaces.Contains(Entity))
                {
                    IsEntity = true;
                    IsVehicle = type == Vehicle || interfaces.Contains(Vehicle);
                    IsPlayer = type == Player || interfaces.Contains(Player);
                    IsBlip = type == Blip || interfaces.Contains(Blip);
                    IsCheckpoint = type == Checkpoint || interfaces.Contains(Checkpoint);
                }
                else
                {
                    IsEntity = false;
                    IsVehicle = false;
                    IsPlayer = false;
                    IsBlip = false;
                    IsCheckpoint = false;
                }

                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    ElementType = elementType;
                    Element = new TypeInfo(elementType);
                }
            }
        }

        private delegate object Parser(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool, TypeInfo typeInfo);

        private static readonly Type Void = typeof(void);

        private static readonly Type Bool = typeof(bool);

        private static readonly Type Int = typeof(int);

        private static readonly Type Long = typeof(long);

        private static readonly Type UInt = typeof(uint);

        private static readonly Type ULong = typeof(ulong);

        private static readonly Type Double = typeof(double);

        private static readonly Type String = typeof(string);

        private static readonly Type Player = typeof(IPlayer);

        private static readonly Type Vehicle = typeof(IVehicle);

        private static readonly Type Checkpoint = typeof(ICheckpoint);

        private static readonly Type Array = typeof(System.Array);

        private static readonly Type Entity = typeof(IEntity);

        private static readonly Type Blip = typeof(IBlip);

        private static readonly Type Obj = typeof(object);

        private static readonly Type FunctionType = typeof(Func);

        //TODO: for high optimization add ParseBoolUnsafe ect. that doesn't contains the mValue type check for scenarios where we already had to check the mValue type

        private static object CreateArray(Type type, MValue[] mValues, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            var length = mValues.Length;
            if (type == Obj)
            {
                var array = new object[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (!ValidateMValueType(currMValue.type, type, typeInfo?.Element))
                        return null;
                    array[i] = ParseObject(ref currMValue, type, baseEntityPool, typeInfo?.Element);
                }

                return array;
            }

            if (type == Bool)
            {
                var array = new bool[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.BOOL)
                    {
                        array[i] = currMValue.GetBool();
                    }
                    else
                    {
                        array[i] = default;
                    }

                    array[i] = currMValue.GetBool();
                }

                return array;
            }

            if (type == Int)
            {
                var array = new int[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.INT)
                    {
                        array[i] = (int) currMValue.GetInt();
                    }
                    else
                    {
                        array[i] = default;
                    }
                }

                return array;
            }

            if (type == Long)
            {
                var array = new long[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.INT)
                    {
                        array[i] = currMValue.GetInt();
                    }
                    else
                    {
                        array[i] = default;
                    }
                }

                return array;
            }

            if (type == UInt)
            {
                var array = new uint[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.UINT)
                    {
                        array[i] = (uint) currMValue.GetUint();
                    }
                    else
                    {
                        array[i] = default;
                    }
                }

                return array;
            }

            if (type == ULong)
            {
                var array = new ulong[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.UINT)
                    {
                        array[i] = currMValue.GetUint();
                    }
                    else
                    {
                        array[i] = default;
                    }
                }

                return array;
            }

            if (type == Double)
            {
                var array = new double[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.DOUBLE)
                    {
                        array[i] = currMValue.GetDouble();
                    }
                    else
                    {
                        array[i] = default;
                    }
                }

                return array;
            }

            if (type == String)
            {
                var array = new string[length];
                for (var i = 0; i < length; i++)
                {
                    var currMValue = mValues[i];
                    if (currMValue.type == MValue.Type.STRING)
                    {
                        array[i] = currMValue.GetString();
                    }
                    else
                    {
                        array[i] = null;
                    }
                }

                return array;
            }

            var typeArray = System.Array.CreateInstance(type, length);
            for (var i = 0; i < length; i++)
            {
                var currMValue = mValues[i];
                if (!ValidateMValueType(currMValue.type, type, typeInfo?.Element))
                {
                    typeArray.SetValue(null, i);
                }
                else
                {
                    var obj = ParseObject(ref currMValue, type, baseEntityPool, typeInfo?.Element);
                    typeArray.SetValue(obj is IConvertible ? Convert.ChangeType(obj, type) : obj, i);
                }
            }

            return typeArray;
        }

        private static object ParseBool(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool, TypeInfo typeInfo)
        {
            if (mValue.type == MValue.Type.BOOL)
            {
                return mValue.GetBool();
            }

            // Types doesn't match
            return null;
        }

        private static object ParseInt(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool, TypeInfo typeInfo)
        {
            if (mValue.type == MValue.Type.INT)
            {
                return mValue.GetInt();
            }

            // Types doesn't match
            return null;
        }

        private static object ParseUInt(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool, TypeInfo typeInfo)
        {
            if (mValue.type == MValue.Type.UINT)
            {
                return mValue.GetUint();
            }

            // Types doesn't match
            return null;
        }

        private static object ParseDouble(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            if (mValue.type == MValue.Type.DOUBLE)
            {
                return mValue.GetDouble();
            }

            // Types doesn't match
            return null;
        }

        private static object ParseString(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            return mValue.type != MValue.Type.STRING ? null : mValue.GetString();
        }

        private static object ParseObject(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            //return mValue.ToObject(entityPool);
            switch (mValue.type)
            {
                case MValue.Type.NIL:
                    return null;
                case MValue.Type.BOOL:
                    return ParseBool(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.INT:
                    return ParseInt(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.UINT:
                    return ParseUInt(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.DOUBLE:
                    return ParseDouble(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.STRING:
                    return ParseString(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.LIST:
                    return ParseArray(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.ENTITY:
                    return ParseEntity(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.DICT:
                    return ParseDictionary(ref mValue, type, baseEntityPool, typeInfo);
                case MValue.Type.FUNCTION:
                    return ParseFunction(ref mValue, type, baseEntityPool, typeInfo);
                default:
                    return false;
            }
        }

        private static object ParseArray(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            // Types doesn't match
            if (mValue.type != MValue.Type.LIST) return null;
            var mValueList = mValue.GetList();
            var elementType = typeInfo?.ElementType ?? (
                                  type.GetElementType() ??
                                  type); // Object has no element type so we have to use the same type again
            return CreateArray(elementType, mValueList, baseEntityPool, typeInfo);
        }

        private static object ParseEntity(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            // Types doesn't match
            if (mValue.type != MValue.Type.ENTITY) return null;
            var entityPointer = IntPtr.Zero;
            mValue.GetEntityPointer(ref entityPointer);
            if (entityPointer == IntPtr.Zero) return null;
            if (!baseEntityPool.GetOrCreate(entityPointer, out var entity) ||
                !ValidateEntityType(entity.Type, type, typeInfo))
                return null;
            return entity;
        }

        private static object ParseDictionary(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            // Types doesn't match
            if (mValue.type != MValue.Type.DICT) return null;
            var args = typeInfo?.GenericArguments ?? type.GetGenericArguments();
            if (args.Length != 2) return null;
            var keyType = args[0];
            if (keyType != String) return null;
            var valueType = args[1];
            var stringViewArrayRef = StringViewArray.Nil;
            var valueArrayRef = MValueArray.Nil;
            AltVNative.MValueGet.MValue_GetDict(ref mValue, ref stringViewArrayRef, ref valueArrayRef);
            var strings = stringViewArrayRef.ToArray();
            var valueArray = valueArrayRef.ToArray();
            var length = strings.Length;
            if (valueArrayRef.size != (ulong) length) // Value size != key size should never happen
            {
                return null;
            }

            MValue currMValue;

            if (valueType == Obj)
            {
                var dict = new Dictionary<string, object>();
                for (var i = 0; i < length; i++)
                {
                    dict[strings[i]] = ParseObject(ref valueArray[i], valueType, baseEntityPool,
                        typeInfo?.DictionaryValue);
                }

                return dict;
            }

            if (valueType == Bool)
            {
                var dict = new Dictionary<string, bool>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.BOOL)
                    {
                        dict[strings[i]] = currMValue.GetBool();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == Int)
            {
                var dict = new Dictionary<string, int>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.INT)
                    {
                        dict[strings[i]] = (int) currMValue.GetInt();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == Long)
            {
                var dict = new Dictionary<string, long>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.INT)
                    {
                        dict[strings[i]] = currMValue.GetInt();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == UInt)
            {
                var dict = new Dictionary<string, uint>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.UINT)
                    {
                        dict[strings[i]] = (uint) currMValue.GetUint();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == ULong)
            {
                var dict = new Dictionary<string, ulong>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.UINT)
                    {
                        dict[strings[i]] = currMValue.GetUint();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == Double)
            {
                var dict = new Dictionary<string, double>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.DOUBLE)
                    {
                        dict[strings[i]] = currMValue.GetDouble();
                    }
                    else
                    {
                        dict[strings[i]] = default;
                    }
                }

                return dict;
            }

            if (valueType == String)
            {
                var dict = new Dictionary<string, string>();
                for (var i = 0; i < length; i++)
                {
                    currMValue = valueArray[i];
                    if (currMValue.type == MValue.Type.STRING)
                    {
                        dict[strings[i]] = currMValue.GetString();
                    }
                    else
                    {
                        dict[strings[i]] = null;
                    }
                }

                return dict;
            }

            var dictType = typeInfo?.DictType ?? typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var typedDict = typeInfo?.CreateDictionary() ??
                            (System.Collections.IDictionary) Activator.CreateInstance(dictType);
            for (var i = 0; i < length; i++)
            {
                currMValue = valueArray[i];
                if (!ValidateMValueType(currMValue.type, valueType, typeInfo?.DictionaryValue))
                {
                    typedDict[strings[i]] = null;
                }
                else
                {
                    var obj = ParseObject(ref currMValue, valueType, baseEntityPool, typeInfo?.DictionaryValue);
                    if (obj is IConvertible)
                    {
                        typedDict[strings[i]] = Convert.ChangeType(obj, valueType);
                    }
                    else
                    {
                        typedDict[strings[i]] = obj;
                    }
                }
            }

            return typedDict;
        }

        private static object ParseFunction(ref MValue mValue, Type type, IBaseEntityPool baseEntityPool,
            TypeInfo typeInfo)
        {
            if (mValue.type == MValue.Type.FUNCTION)
            {
                return (Func) new FuncWrapper(mValue.GetFunction()).Call;
            }

            // Types doesn't match
            return null;
        }

        private static bool ValidateEntityType(EntityType entityType, Type type, TypeInfo typeInfo)
        {
            if (type == Obj)
            {
                return true;
            }

            switch (entityType)
            {
                case EntityType.Blip:
                    return typeInfo?.IsBlip ?? type == Blip || type.GetInterfaces().Contains(Blip);
                case EntityType.Player:
                    return typeInfo?.IsPlayer ?? type == Player || type.GetInterfaces().Contains(Player);
                case EntityType.Vehicle:
                    return typeInfo?.IsVehicle ?? type == Vehicle || type.GetInterfaces().Contains(Vehicle);
                case EntityType.Checkpoint:
                    return typeInfo?.IsCheckpoint ?? type == Checkpoint || type.GetInterfaces().Contains(Checkpoint);
                default:
                    return false;
            }
        }

        private static bool ValidateMValueType(MValue.Type valueType, Type type, TypeInfo typeInfo)
        {
            if (type == Obj)
            {
                // object[] or object accepts anything
                return true;
            }

            switch (valueType)
            {
                case MValue.Type.NIL:
                    return !type.IsPrimitive || type == String; //TODO: check if there are more none nullable types
                         return type == String || type == Obj || typeInfo.IsList || typeInfo.IsDict || typeInfo.IsEntity;
                     }*/
                //case MValue.Type.NIL:
                //    return true;
                case MValue.Type.BOOL:
                    return type == Bool;
                case MValue.Type.INT:
                    return type == Int || type == UInt;
                case MValue.Type.UINT:
                    return type == UInt || type == ULong;
                case MValue.Type.DOUBLE:
                    return type == Double;
                case MValue.Type.STRING:
                    return type == String;
                case MValue.Type.LIST:
                    return typeInfo?.IsList ?? type.BaseType == Array;
                case MValue.Type.ENTITY:
                    return typeInfo?.IsEntity ?? type.GetInterfaces().Contains(Entity);
                case MValue.Type.FUNCTION:
                    return false; //TODO: needs to be Func or Action
                case MValue.Type.DICT:
                    return typeInfo?.IsDict ?? type.Name.StartsWith("Dictionary");
                default:
                    return false;
            }
        }

        // Returns null when function signature isn't supported
        public static Function Create<T>(T func) where T : Delegate
        {
            var type = func.GetType();
            var genericArguments = type.GetGenericArguments();
            Type returnType;
            if (type.Name.StartsWith("Func"))
            {
                // Return type is last generic argument
                // Function never has empty generic arguments, so no need for size check, but we do it anyway
                if (genericArguments.Length == 0)
                {
                    returnType = Void;
                }
                else
                {
                    returnType = genericArguments[genericArguments.Length - 1];
                    genericArguments = genericArguments.SkipLast(1).ToArray();
                }
            }
            else
            {
                returnType = Void;
            }

            //TODO: check for unsupported types
            var parsers = new Parser[genericArguments.Length];
            var typeInfos = new TypeInfo[genericArguments.Length];
            for (int i = 0, length = genericArguments.Length; i < length; i++)
            {
                var arg = genericArguments[i];
                var typeInfo = new TypeInfo(arg);
                typeInfos[i] = typeInfo;
                if (arg == Obj)
                {
                    //TODO: use MValue.ToObject here
                    parsers[i] = ParseObject;
                }
                else if (arg == Bool)
                {
                    parsers[i] = ParseBool;
                }
                else if (arg == Int || arg == Long)
                {
                    parsers[i] = ParseInt;
                }
                else if (arg == UInt || arg == ULong)
                {
                    parsers[i] = ParseUInt;
                }
                else if (arg == Double)
                {
                    parsers[i] = ParseDouble;
                }
                else if (arg == String)
                {
                    parsers[i] = ParseString;
                }
                else if (arg.BaseType == Array)
                {
                    parsers[i] = ParseArray;
                }
                else if (typeInfo.IsEntity)
                {
                    parsers[i] = ParseEntity;
                }
                else if (typeInfo.IsDict)
                {
                    parsers[i] = ParseDictionary;
                }
                else if (arg == FunctionType)
                {
                    parsers[i] = ParseFunction;
                }
                else
                {
                    // Unsupported type
                    return null;
                }
            }

            return new Function(func, returnType, genericArguments, parsers, typeInfos);
        }

        private readonly Delegate @delegate;

        private readonly Type returnType;

        private readonly Type[] args;

        private readonly Parser[] parsers;

        private readonly TypeInfo[] typeInfos;

        private Function(Delegate @delegate, Type returnType, Type[] args, Parser[] parsers, TypeInfo[] typeInfos)
        {
            this.@delegate = @delegate;
            this.returnType = returnType;
            this.args = args;
            this.parsers = parsers;
            this.typeInfos = typeInfos;
        }

        //TODO: add support for nullable args, these are reducing the required length, add support for default values as well
        internal MValue Call(IBaseEntityPool baseEntityPool, MValue[] values)
        {
            var length = values.Length;
            if (length != args.Length) return MValue.Nil;
            var invokeValues = new object[length];
            for (var i = 0; i < length; i++)
            {
                invokeValues[i] = parsers[i](ref values[i], args[i], baseEntityPool, typeInfos[i]);
            }

            var result = @delegate.DynamicInvoke(invokeValues);
            if (returnType == Void) return MValue.Nil;
            return MValue.CreateFromObject(result) ?? MValue.Nil;
        }

        internal MValue Call(IPlayer player, IBaseEntityPool baseEntityPool, MValue[] values)
        {
            var length = values.Length;
            if (length + 1 != args.Length) return MValue.Nil;
            if (!typeInfos[0].IsPlayer) return MValue.Nil;
            var invokeValues = new object[length + 1];
            invokeValues[0] = player;
            for (var i = 0; i < length; i++)
            {
                invokeValues[i + 1] = parsers[i](ref values[i], args[i], baseEntityPool, typeInfos[i]);
            }

            var result = @delegate.DynamicInvoke(invokeValues);
            if (returnType == Void) return MValue.Nil;
            return MValue.CreateFromObject(result) ?? MValue.Nil;
        }

        internal MValue Call(IBaseEntityPool baseEntityPool, MValue valueArgs)
        {
            return valueArgs.type != MValue.Type.LIST ? MValue.Nil : Call(baseEntityPool, valueArgs.GetList());
        }

        internal void call(ref MValue args, ref MValue result)
        {
            result = Call(Alt.Module.BaseEntityPool, args);
        }
    }
}