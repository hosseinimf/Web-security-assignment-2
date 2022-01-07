namespace WS_uppgift2.Models
{
    public class Comments
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? Content { get; set; }
    }
}
