using System.Text.Json;

namespace Oceana.Server.Infrastructure.Persistence;

/// <summary>
/// An <see cref="IStateStore{T}"/> that stores state as an indented JSON file. Writes are atomic
/// (written to a temporary file then moved into place) and serialised by a per-file lock, so a
/// crash mid-write cannot corrupt the existing file. A missing or unreadable file loads as null.
/// </summary>
/// <typeparam name="T">The state type persisted as a unit.</typeparam>
public sealed class JsonStateStore<T> : IStateStore<T>
    where T : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly object gate = new object();
    private readonly string filePath;
    private readonly ILogger<JsonStateStore<T>> logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="JsonStateStore{T}"/> class.
    /// </summary>
    /// <param name="filePath">The absolute path of the JSON file backing this store.</param>
    /// <param name="logger">The logger used to report load/save problems.</param>
    public JsonStateStore(string filePath, ILogger<JsonStateStore<T>> logger)
    {
        this.filePath = filePath;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public T? Load()
    {
        lock (this.gate)
        {
            if (!File.Exists(this.filePath))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(this.filePath), SerializerOptions);
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                this.logger.LogWarning(ex, "Could not read persisted state from {Path}; starting fresh.", this.filePath);
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public void Save(T state)
    {
        lock (this.gate)
        {
            var directory = Path.GetDirectoryName(this.filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = this.filePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(state, SerializerOptions));
            File.Move(tempPath, this.filePath, overwrite: true);
        }
    }
}
