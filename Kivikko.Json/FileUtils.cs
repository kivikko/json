namespace Kivikko.Json;

internal static class FileUtils
{
    internal static void WriteAllTextIfDifferent(string path, string content, Encoding encoding)
    {
        if (File.Exists(path))
        {
            var newContentHashCode = content.GetHashCode();
            var oldContentHashCode = File.ReadAllText(path, encoding).GetHashCode();
            
            if (newContentHashCode == oldContentHashCode)
                return;
        }
        
        CreateDirectoryIfNotExist(path);
        File.WriteAllText(path, content, encoding);
    }

    private static void CreateDirectoryIfNotExist(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (directory is not null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}