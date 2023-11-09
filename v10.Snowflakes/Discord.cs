using v10.Snowflakes.Extensions;

namespace v10.Snowflakes;

public class Discord
{
    //private Id Id { get; set; }
    public long UnixTimeMilliseconds { get; set; }
    public short InternalWorkerID { get; set; }
    public short InternalProcessID { get; set; }
    public short Increment { get; set; }

    //public Discord(): this(Id.Create()) { }

    //public Discord(Id id)
    //{
    //    Id = id;
    //    UnixTimeMilliseconds = Id.ToUnixTimeMilliseconds();
    //    InternalWorkerID = (short)((Id >> 17) & 0b11111);
    //    InternalProcessID = (short)((Id >> 12) & 0b11111);
    //    Increment = (short)(Id & 0b111111111111);
    //}
}
