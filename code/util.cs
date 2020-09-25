using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LaneFight
{
    class util
    {

    }

    public class ReflectionUtil { 

        public static T GetValue<T>(Object obj, String param)
        {
            var type = obj.GetType();
            const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic;

            PropertyInfo property = null;

            while (type != null)
            {
                property = type.GetProperty(param, InstanceBindFlags);
                if (property != null)
                {
                    break;
                }

                type = type.BaseType;
            }
            if (property != null)
            {
                return (T)property.GetValue(obj);
            }

            return default;
        }

    }
}
