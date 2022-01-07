namespace WS_uppgift2.Models
{
    public class AppFile
    {
        public Guid Id { get; set; }
        public string UntrustedName { get; set; }
        public DateTime TimeStamp { get; set; }
        public long? Size { get; set; }
        public byte[] Content { get; set; }


    }
}
