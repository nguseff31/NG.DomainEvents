namespace NG.DomainEvents.Common.Exceptions;

public class AlreadyDoneException : Exception, ITaskResultException {
    const string DefaultMessage = "Task is already done";
    public AlreadyDoneException() : base(DefaultMessage) { }
    public AlreadyDoneException(string? message, Exception? innerException = null) : base($"{DefaultMessage}. {message}", innerException) { }
}