using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, ReflectionCache> Cache = new ConcurrentDictionary<Type, ReflectionCache>();

        public static ReflectionCache Of<T>()
        {
            return Of(typeof(T));
        }

        public static ReflectionCache Of<T>(T instance)
        {
            return Of(typeof(T));
        }

        public static ReflectionCache Of(Type type)
        {
            // ReSharper disable AssignNullToNotNullAttribute
            return Cache.GetOrAdd(type, t => new ReflectionCache(t));
            // ReSharper restore AssignNullToNotNullAttribute
        }


        private readonly Type _type;

        private readonly IReadOnlyList<object> _empty = new List<object>().AsReadOnly();

        private ReflectionCache(Type type)
        {
            _type = type;
        }

        private IReadOnlyList<object> _attributesWithInheritance;
        private IReadOnlyList<object> _attributesWithoutInheritance;
        private IReadOnlyList<MemberInfo> _publicMembers;
        private IReadOnlyList<MethodInfo> _publicMethods;
        private IReadOnlyList<PropertyInfo> _publicProperties;
        private readonly Dictionary<Enum, IReadOnlyList<object>> _enumValueAttributes = new Dictionary<Enum, IReadOnlyList<object>>();
       
        public IReadOnlyList<object> GetCustomAttributes(bool inherit)
        {
            return inherit ? GetCustomAttributesWithInheritance() : GetCustomAttributesWithoutInheritance();
        }

        private IReadOnlyList<object> GetCustomAttributesWithInheritance()
        {
            return _attributesWithInheritance ?? (_attributesWithInheritance = _type.GetCustomAttributes(true).Where(a => a != null).ToArray());
        }

        private IReadOnlyList<object> GetCustomAttributesWithoutInheritance()
        {
            return _attributesWithoutInheritance ?? (_attributesWithoutInheritance = _type.GetCustomAttributes(false).Where(a => a != null).ToArray());
        }

        public IReadOnlyList<MethodInfo> GetPublicMethods()
        {
            return _publicMethods ?? (_publicMethods = _type.GetMethods().Where(m => m != null).ToArray());
        }

        public IReadOnlyList<MethodInfo> GetPublicMethods(string name, StringComparison comparisonType)
        {
            return GetPublicMethods().Where(m => string.Equals(m.Name, name, comparisonType)).ToList().AsReadOnly();
        }

        public IReadOnlyList<MemberInfo> GetPublicMembers()
        {
            return _publicMembers ?? (_publicMembers = _type.GetMembers().Where(m => m != null).ToArray());
        }
    
        public IReadOnlyList<MemberInfo> GetPublicMembers(string name, StringComparison comparisonType)
        {
            return GetPublicMembers().Where(m => m != null && string.Equals(m.Name, name, comparisonType)).ToList().AsReadOnly();
        }

        public IReadOnlyList<PropertyInfo> GetPublicProperties()
        {
            return _publicProperties ?? (_publicProperties = _type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(p => p != null).ToArray());
        }

        public PropertyInfo GetPublicProperty(string name, StringComparison comparisonType)
        {
            return GetPublicProperties().FirstOrDefault(p => p != null && string.Equals(p.Name, name, comparisonType));
        }
        
        public IReadOnlyList<object> GetEnumValueCustomAttributes<T>(T value)
        {
            var member = GetPublicMembers(value.ToString(), StringComparison.InvariantCulture);
            if (member.Count == 0 || member[0] == null) return _empty;

            IReadOnlyList<object> attributes;
            if (_enumValueAttributes.TryGetValue(Cast.To<Enum>(value), out attributes)) return attributes ?? _empty;

            attributes = member[0].GetCustomAttributes(false);
            return (_enumValueAttributes[Cast.To<Enum>(value)] = attributes);
        }
    }
}