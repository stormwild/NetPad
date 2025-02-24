using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetPad.DotNet;
using NetPad.Packages;

namespace NetPad.Utilities;

public static class ReferenceUtil
{
    public static async Task<HashSet<string>> GetAssemblyPathsAsync(this IEnumerable<Reference> references, IPackageProvider packageProvider)
    {
        var assemblyPaths = new HashSet<string>();

        foreach (var reference in references.Distinct())
        {
            if (reference is AssemblyFileReference assemblyFileReference && assemblyFileReference.AssemblyPath != null)
            {
                assemblyPaths.Add(assemblyFileReference.AssemblyPath);
            }
            else if (reference is PackageReference packageReference)
            {
                var packageAndDependanciesAssemblies = await packageProvider
                    .GetPackageAndDependanciesAssembliesAsync(packageReference.PackageId, packageReference.Version)
                    .ConfigureAwait(false);

                assemblyPaths.AddRange(packageAndDependanciesAssemblies);
            }
        }

        return assemblyPaths;
    }
}
