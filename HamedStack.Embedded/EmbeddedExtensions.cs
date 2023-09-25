// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace HamedStack.Embedded;

/// <summary>
/// Provides extension methods for embedded resources.
/// </summary>
public static class EmbeddedExtensions
{
    /// <summary>
    /// Checks if the source string contains the specified substring with optional case sensitivity.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="str">The substring to search for.</param>
    /// <param name="ignoreCase">Indicates whether the search should be case-sensitive.</param>
    /// <returns>True if the source contains the substring, false otherwise.</returns>
    private static bool Contains(this string source, string str, bool ignoreCase)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(source))
            return true;
        return source.Contains(str, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
    /// <summary>
    /// Gets a specified resource from the provided assembly and returns its content as a string.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve the resource.</param>
    /// <param name="resourceName">The name of the resource to retrieve.</param>
    /// <returns>The content of the specified resource as a string.</returns>
    /// <exception cref="ArgumentException">Thrown when the resource cannot be found in the assembly.</exception>
    public static string GetResourceAsString(this Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new ArgumentException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'. Ensure the resource name is correct and the resource is set to 'Embedded Resource'.");
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Retrieves all the resources from the provided assembly.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <returns>A collection of the resources found in the assembly.</returns>
    public static IEnumerable<IFileInfo> GetResources(this Assembly assembly)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        return embedded.GetDirectoryContents("/");
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <returns>A collection of the resources found in the assemblies.</returns>
    public static IEnumerable<IFileInfo> GetResources(this IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Select(assembly => new EmbeddedFileProvider(assembly))
            .SelectMany(embedded => embedded.GetDirectoryContents("/")).ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on the specified name.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="name">The name of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of resources that match the specified name.</returns>
    public static IEnumerable<IFileInfo> GetResources(this Assembly assembly, string name, bool ignoreCase = false)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded
            .GetDirectoryContents("/")
            .Where(x => x.Name.Contains(name, ignoreCase));
        return resources.ToList();
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies based on the specified name.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="name">The name of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of resources that match the specified name from the assemblies.</returns>
    public static IEnumerable<IFileInfo> GetResources(this IEnumerable<Assembly> assemblies, string name, bool ignoreCase = false)
    {
        return assemblies
            .Select(assembly => new EmbeddedFileProvider(assembly))
            .SelectMany(embedded => embedded.GetDirectoryContents("/")
                .Where(x => x.Name.Contains(name, ignoreCase))).ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on a collection of names.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="names">The names of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of resources that match any of the specified names.</returns>
    public static IEnumerable<IFileInfo> GetResources(this Assembly assembly, string[] names, bool ignoreCase = false)
    {
        var rs = new List<IFileInfo>();
        var embedded = new EmbeddedFileProvider(assembly);
        foreach (var name in names)
        {
            var resources = embedded.GetDirectoryContents("/").Where(x => x.Name.Contains(name, ignoreCase));
            rs.AddRange(resources);
        }

        return rs.ToList();
    }
    /// <summary>
    /// Retrieves resources from a collection of assemblies based on a collection of names.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="names">The names of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of resources that match any of the specified names from the assemblies.</returns>
    public static IEnumerable<IFileInfo> GetResources(this IEnumerable<Assembly> assemblies, string[] names, bool ignoreCase = false)
    {
        var rs = new List<IFileInfo>();
        foreach (var assembly in assemblies)
        {
            var embedded = new EmbeddedFileProvider(assembly);
            foreach (var name in names)
            {
                var resources = embedded.GetDirectoryContents("/").Where(x => x.Name.Contains(name, ignoreCase));
                rs.AddRange(resources);
            }
        }

        return rs.ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on a regex pattern.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="regex">The regex pattern to match resource names.</param>
    /// <returns>A collection of resources that match the regex pattern.</returns>
    public static IEnumerable<IFileInfo> GetResources(this Assembly assembly, Regex regex)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded.GetDirectoryContents("/")
            .Where(x => regex.IsMatch(x.Name));
        return resources.ToList();
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies based on a regex pattern.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="regex">The regex pattern to match resource names.</param>
    /// <returns>A collection of resources that match the regex pattern from the assemblies.</returns>
    public static IEnumerable<IFileInfo> GetResources(this IEnumerable<Assembly> assemblies, Regex regex)
    {
        return assemblies.Select(assembly => new EmbeddedFileProvider(assembly))
            .SelectMany(embedded => embedded.GetDirectoryContents("/")
                .Where(x => regex.IsMatch(x.Name)))
            .ToList();
    }

    /// <summary>
    /// Retrieves all resources from the provided assembly as streams.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <returns>A collection of streams representing each resource.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this Assembly assembly)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded.GetDirectoryContents("/");
        return resources.Select(resource => resource.CreateReadStream()).ToList();
    }

    /// <summary>
    /// Retrieves all resources from a collection of assemblies as streams.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <returns>A collection of streams representing each resource from the assemblies.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this IEnumerable<Assembly> assemblies)
    {
        return (from assembly in assemblies select new EmbeddedFileProvider(assembly) 
            into embedded from resource in embedded.GetDirectoryContents("/") 
            select resource.CreateReadStream())
            .ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on the specified name, returning them as streams.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="name">The name of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of streams representing the resources that match the specified name.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this Assembly assembly, string name, bool ignoreCase = false)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded.GetDirectoryContents("/")
            .Where(x => x.Name.Contains(name, ignoreCase));
        return resources.Select(resource => resource.CreateReadStream()).ToList();
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies based on the specified name, returning them as streams.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="name">The name of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of streams representing the resources that match the specified name from the assemblies.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this IEnumerable<Assembly> assemblies, string name, bool ignoreCase = false)
    {
        return (from assembly in assemblies select new EmbeddedFileProvider(assembly) 
            into embedded from resource in embedded.GetDirectoryContents("/")
                .Where(x => x.Name.Contains(name, ignoreCase)) select resource.CreateReadStream())
            .ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on a collection of names, returning them as streams.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="names">The names of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of streams representing the resources that match any of the specified names.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this Assembly assembly, string[] names, bool ignoreCase = false)
    {
        var rs = new List<IFileInfo>();
        var embedded = new EmbeddedFileProvider(assembly);
        foreach (var name in names)
        {
            var resources = embedded.GetDirectoryContents("/").Where(x => x.Name.Contains(name, ignoreCase));
            rs.AddRange(resources);
        }

        return rs.Select(resource => resource.CreateReadStream()).ToList();
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies based on a collection of names, returning them as streams.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="names">The names of the resources to retrieve.</param>
    /// <param name="ignoreCase">Indicates whether the name match should be case-sensitive.</param>
    /// <returns>A collection of streams representing the resources that match any of the specified names from the assemblies.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this IEnumerable<Assembly> assemblies, string[] names, bool ignoreCase = false)
    {
        var rs = new List<IFileInfo>();
        foreach (var assembly in assemblies)
        {
            var embedded = new EmbeddedFileProvider(assembly);
            foreach (var name in names)
            {
                var resources = embedded.GetDirectoryContents("/").Where(x => x.Name.Contains(name, ignoreCase));
                rs.AddRange(resources);
            }
        }

        return rs.Select(resource => resource.CreateReadStream()).ToList();
    }

    /// <summary>
    /// Retrieves resources from the provided assembly based on a regex pattern, returning them as streams.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve resources.</param>
    /// <param name="regex">The regex pattern to match resource names.</param>
    /// <returns>A collection of streams representing the resources that match the regex pattern.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this Assembly assembly, Regex regex)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded
            .GetDirectoryContents("/")
            .Where(x => regex.IsMatch(x.Name));
        return resources.Select(resource => resource.CreateReadStream()).ToList();
    }

    /// <summary>
    /// Retrieves resources from a collection of assemblies based on a regex pattern, returning them as streams.
    /// </summary>
    /// <param name="assemblies">The assemblies from which to retrieve resources.</param>
    /// <param name="regex">The regex pattern to match resource names.</param>
    /// <returns>A collection of streams representing the resources that match the regex pattern from the assemblies.</returns>
    public static IEnumerable<Stream> GetResourcesAsStream(this IEnumerable<Assembly> assemblies, Regex regex)
    {
        return (from assembly in assemblies select new EmbeddedFileProvider(assembly) 
            into embedded 
            from resource in embedded.GetDirectoryContents("/")
                .Where(x => regex.IsMatch(x.Name)) select resource.CreateReadStream())
            .ToList();
    }

    /// <summary>
    /// Retrieves all resources from the provided assembly and returns them as strings.
    /// </summary>
    /// <param name="assembly">The assembly from which to retrieve the resources.</param>
    /// <returns>A collection of string representations of the resources from the assembly.</returns>
    public static IEnumerable<string> GetResourcesAsString(this Assembly assembly)
    {
        var embedded = new EmbeddedFileProvider(assembly);
        var resources = embedded.GetDirectoryContents("/");
        return resources
            .Select(resource => $"{Assembly.GetExecutingAssembly().GetName().Name}.{resource.Name}")
            .Select(assembly.GetResourceAsString)
            .ToList();
    }
}