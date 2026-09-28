namespace SMOO.Exceptions;

internal class SequencedStoreFullException : Exception
{
    public SequencedStoreFullException()
    {

    }

    public SequencedStoreFullException(string? message) : base(message)
    {

    }

    public SequencedStoreFullException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}