namespace EvacuationApi.Services;

public class MapFileStorage : IMapFileStorage
{
    public const long MaxSizeBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    private readonly string _root;

    public MapFileStorage(IHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "RouteMaps");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(string originalFileName, Stream content)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!ContentTypes.ContainsKey(ext))
            throw new InvalidOperationException("Допустимые форматы карты: PDF, PNG, JPG.");

        // Читаем в память с ограничением размера.
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk.AsMemory(0, chunk.Length))) > 0)
        {
            buffer.Write(chunk, 0, read);
            if (buffer.Length > MaxSizeBytes)
                throw new InvalidOperationException("Файл карты больше 10 МБ.");
        }

        var bytes = buffer.ToArray();
        if (!MatchesSignature(ext, bytes))
            throw new InvalidOperationException("Содержимое файла не соответствует его расширению.");

        // Имя на диске генерируем сами — исходное имя от пользователя на диск не попадает.
        var storedName = Guid.NewGuid().ToString("N") + ext;
        await File.WriteAllBytesAsync(Path.Combine(_root, storedName), bytes);
        return storedName;
    }

    public StoredMapFile? Open(string storedName)
    {
        var path = ResolvePath(storedName);
        if (path is null || !File.Exists(path)) return null;

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new StoredMapFile(stream, ContentTypes[Path.GetExtension(path)]);
    }

    public void Delete(string storedName)
    {
        var path = ResolvePath(storedName);
        if (path is not null && File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Защита от path traversal: допускается только простое имя файла
    /// </summary>
    private string? ResolvePath(string storedName)
    {
        if (string.IsNullOrEmpty(storedName) || storedName != Path.GetFileName(storedName))
            return null;
        if (!ContentTypes.ContainsKey(Path.GetExtension(storedName)))
            return null;
        return Path.Combine(_root, storedName);
    }

    private static bool MatchesSignature(string ext, byte[] b) => ext switch
    {
        ".pdf" => b.Length >= 4 && b[0] == 0x25 && b[1] == 0x50 && b[2] == 0x44 && b[3] == 0x46,
        ".png" => b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47,
        ".jpg" or ".jpeg" => b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF,
        _ => false
    };
}
