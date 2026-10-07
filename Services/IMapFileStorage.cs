namespace EvacuationApi.Services;

public record StoredMapFile(Stream Content, string ContentType);

/// <summary>
/// Хранилище электронных карт маршрутов (файлы на диске)
/// </summary>
public interface IMapFileStorage
{
    /// <summary>
    /// Сохраняет файл и возвращает имя на диске.
    /// Бросает InvalidOperationException: недопустимый формат, содержимое не соответствует расширению, размер &gt; 10 МБ.
    /// </summary>
    Task<string> SaveAsync(string originalFileName, Stream content);

    StoredMapFile? Open(string storedName);

    void Delete(string storedName);
}
