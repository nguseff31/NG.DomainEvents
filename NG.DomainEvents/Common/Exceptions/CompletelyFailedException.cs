namespace NG.DomainEvents.Common.Exceptions;

public class CompletelyFailedException : Exception, ITaskResultException {
    const string DefaultMessage = "Task completely failed";
    public CompletelyFailedException() : base(DefaultMessage) { }
    public CompletelyFailedException(string? message, Exception? innerException = null) : base($"{DefaultMessage}. {message}", innerException) { }
}