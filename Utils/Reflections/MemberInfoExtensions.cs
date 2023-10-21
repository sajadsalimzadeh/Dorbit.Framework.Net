using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Dorbit.Utils.Reflections
{
    public static class MemberInfoExtensions
    {
        public static string GetDisplayName(this MemberInfo member)
        {
            var attribute = member.GetCustomAttribute(typeof(DisplayNameAttribute));
            return (attribute != null ? ((DisplayNameAttribute)attribute).DisplayName : member.Name);
        }
        public static bool IsField(this MemberInfo member) => member.GetCustomAttribute<NotMappedAttribute>() == null;
        public static bool IsPrimaryKey(this MemberInfo member) => member.GetCustomAttribute<KeyAttribute>() != null;
        public static bool IsIdentity(this MemberInfo member) => member.GetCustomAttribute<DatabaseGeneratedAttribute>() != null;
        public static bool IsInsertable(this MemberInfo member) => !IsIdentity(member) && IsField(member);
        public static bool IsUpdatable(this MemberInfo member) => !IsIdentity(member) && IsField(member);
    }
}
