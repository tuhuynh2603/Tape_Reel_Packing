using System;
using System.ComponentModel;
using System.Reflection;

namespace TapeReelPacking
{
   public static class ExceedToolkit
    {

        public static string GetDisplayName<T>(string propertyName)
        {
            // Get the type of the generic class T
            Type type = typeof(T);

            // Get the PropertyInfo of the property
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            // Check if the DisplayNameAttribute is defined on the property
            if (propertyInfo != null && Attribute.IsDefined(propertyInfo, typeof(DisplayNameAttribute)))
            {
                // Get the DisplayNameAttribute from the property
                DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DisplayNameAttribute));

                // Return the DisplayName value
                return displayNameAttribute.DisplayName;
            }

            return null;
        }
    }
}
