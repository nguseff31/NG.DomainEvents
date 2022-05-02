using System.Text.Json.Serialization;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Common;

/// <summary>
///     Abstract result of domain event handler execution
/// </summary>
/// <remarks>
///     Results are saved in <see cref="DomainEventResultDto" />.
/// </remarks>
public abstract class DomainEventResult {
    public string? Exception { get; set; }

    public bool? NotNeeded { get; set; }

    public bool? AlreadyDone { get; set; }

    private Exception? _exceptionInstance;
    
    [JsonIgnore]
    public Exception? ExceptionInstance
    {
        get => _exceptionInstance;
        set { 
            _exceptionInstance = value;
            Exception = _exceptionInstance?.ToString();
        }
    }

    /// <summary>
    /// Need for retry in case of Exception
    /// </summary>
    [JsonIgnore]
    public bool NeedRetry { get; set; }

    [JsonIgnore]
    public TimeSpan Elapsed { get; set; }

    [JsonIgnore]
    public string Handler { get; set; }
}