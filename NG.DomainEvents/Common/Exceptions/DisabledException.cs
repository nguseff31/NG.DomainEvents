namespace NG.DomainEvents.Common.Exceptions;

public class DisabledException : Exception, ITaskResultException {
    const string DefaultMessage = "Task disabled";
    public DisabledException() : base(DefaultMessage) { }
    public DisabledException(string? message, Exception? innerException = null) : base($"{DefaultMessage}. {message}", innerException) { }
}