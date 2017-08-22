using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace WannaGo.Utility
{
    public static class Binder
    {
        public static void Bind(object source, object destination)
        {
            BindFields(source, destination);
            BindProperties(source, destination);
        }

        public static void BindFields(object source, object destination)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var sourceType = source.GetType();
            var destType = destination.GetType();
            var sourceFields = sourceType.GetFields(flags);
            var destFields = destType.GetFields(flags);
            
            foreach(var destField in destFields)
            {
                var sourceField = sourceFields.FirstOrDefault(f => f.Name == destField.Name);
                if (sourceField != null)
                    destField.SetValue(destination,sourceField.GetValue(source));
            }
        }

        public static void BindProperties(object source, object destination)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var sourceType = source.GetType();
            var destType = destination.GetType();
            var sourceProp = sourceType.GetProperties(flags);
            var destFields = destType.GetProperties(flags);

            foreach (var destField in destFields)
            {
                var sourceField = sourceProp.FirstOrDefault(f => f.Name == destField.Name);
                if (sourceField != null && destField.PropertyType == sourceField.PropertyType)
                    destField.SetValue(destination,sourceField.GetValue(source));
            }
        }
    }
}
