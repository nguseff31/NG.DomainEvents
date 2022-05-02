namespace NG.DomainEvents.Common.Exceptions;

public class NeedRetryException : Exception {
    public NeedRetryException() { }

    public NeedRetryException(Exception? inner) : base(inner?.Message, inner) { }
}