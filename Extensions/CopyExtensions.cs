﻿using System;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable MemberHidesStaticFromOuterClass

// ReSharper disable SuggestBaseTypeForParameter

namespace IO.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

        public static bool IsPrimitive(this Type type) => type == typeof(string) || type.IsValueType & type.IsPrimitive;

        public static T Copy<T>(this T original) => (T)Copy((object?)original)!;
        public static object? Copy(this object? originalObject) => InternalCopy(originalObject, new Dictionary<object, object?>(new ReferenceEqualityComparer()));

        private static object? InternalCopy(object? originalObject, IDictionary<object, object?> visited)
        {
            if (originalObject is null) return null;

            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;

            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType()!;
                if (!IsPrimitive(arrayType))
                {
                    var clonedArray = (Array)cloneObject!;
                    ForEach(clonedArray, (array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object? originalObject, IDictionary<object, object?> visited, object? cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType is null) return;

            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }

        private static void CopyFields(object? originalObject, IDictionary<object, object?> visited, object? cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool>? filter = null)
        {
            foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);

                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        internal class ReferenceEqualityComparer : EqualityComparer<object>
        {
            public override bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public override int GetHashCode(object? obj) => obj == null ? 0 : obj.GetHashCode();
        }

        private static void ForEach(Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;

            ArrayTraverse walker = new(array);

            do action(array, walker.Position);
            while (walker.Step());
        }

        internal class ArrayTraverse
        {
            public int[] Position;
            private readonly int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (var i = 0; i < array.Rank; ++i)
                    maxLengths[i] = array.GetLength(i) - 1;

                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (var i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] >= maxLengths[i]) continue;

                    Position[i]++;
                    for (var j = 0; j < i; j++)
                        Position[j] = 0;

                    return true;
                }

                return false;
            }
        }
    }
}