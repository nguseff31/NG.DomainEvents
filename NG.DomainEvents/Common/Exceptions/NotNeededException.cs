namespace NG.DomainEvents.Common.Exceptions;

public class NotNeededException : Exception, ITaskResultException {
    const string DefaultMessage = "No task needed";
    public NotNeededException() : base(DefaultMessage) { }
    public NotNeededException(string? message, Exception? innerException = null) : base($"{DefaultMessage}. {message}", innerException) { }
}