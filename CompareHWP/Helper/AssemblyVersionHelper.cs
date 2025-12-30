using System.Reflection;

namespace CompareHWP.Helper
{
    public static class AssemblyVersionHelper
    {
        public static string InformationalVersion
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();

                var attr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

                return attr?.InformationalVersion ?? "Unknown";
            }
        }
    }
}
