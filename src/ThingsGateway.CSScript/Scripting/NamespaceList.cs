using Microsoft.CodeAnalysis;
using System.Text;

namespace Westwind.Scripting
{
    /// <summary>
    /// HashSet of namespaces
    /// </summary>
    public class NamespaceList : HashSet<string>
    {
        public override string ToString()
        {
            using var sb = new ValueStringBuilder();
            var enumerator = this.GetEnumerator();
            foreach (string ns in this)
            {
                sb.AppendLine($"using {ns};");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// HashSet of References
    /// </summary>
    public class ReferenceList : HashSet<PortableExecutableReference>
    {

    }
}
