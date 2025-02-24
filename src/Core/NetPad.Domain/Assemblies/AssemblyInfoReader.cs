using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace NetPad.Assemblies;

public class AssemblyInfoReader : IAssemblyInfoReader
{
    public HashSet<string> GetNamespaces(byte[] assembly)
    {
        var namespaces = new HashSet<string>();

        using var stream = new MemoryStream(assembly);
        using var portableExecutableReader = new PEReader(stream);
        var metadataReader = portableExecutableReader.GetMetadataReader();

        foreach (var typeDefHandle in metadataReader.TypeDefinitions)
        {
            var typeDef = metadataReader.GetTypeDefinition(typeDefHandle);

            var ns = metadataReader.GetString(typeDef.Namespace);

            if (string.IsNullOrWhiteSpace(ns))
                continue; // If it's namespace is blank, it's not a user-defined type

            if (!typeDef.Attributes.HasFlag(TypeAttributes.Public))
                continue;

            namespaces.Add(ns);
        }

        return namespaces;
    }
}
