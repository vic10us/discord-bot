using System;
using System.Runtime.Serialization;

namespace bot.Modules;

[Serializable]
internal class DuplicateMessageException : Exception
{
    public DuplicateMessageException()
    {
    }

    public DuplicateMessageException(string message) : base(message)
    {
    }

    public DuplicateMessageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected DuplicateMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
