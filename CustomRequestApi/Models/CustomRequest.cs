namespace CustomRequestApi.Models
{
    public class CustomRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public List<string>? Attachments { get; set; }
    }
}
