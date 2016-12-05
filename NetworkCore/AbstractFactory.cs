using System.Collections.Generic;
using System;
using System.Reflection;

namespace MNS
{
#if UNITY_5
    public static class TypeExtensions
    {
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
#endif

    public class AbstractFactory<EnumT, Base, SingletonT> : Singleton<SingletonT>
        where SingletonT : class, new()
        where Base : class
    {
        Dictionary<EnumT, Type> m_Factories = new Dictionary<EnumT, Type>();

        protected void Add(EnumT enum_type, Type class_type)
        {
            if (class_type.GetTypeInfo().IsSubclassOf(typeof(Base)) == false)
                throw new System.Exception(string.Format("{0} is not sub class of {1}", class_type.Name, typeof(Base).Name));
            m_Factories.Add(enum_type, class_type);
        }

        public Base Create(EnumT enum_type)
        {
            Type class_type;
            m_Factories.TryGetValue(enum_type, out class_type);
            return Activator.CreateInstance(class_type) as Base;
        }
    }
}