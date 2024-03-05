namespace backend_library_app.Models
{
    public class Book
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public DateTime Created { get; set; }
        public string Image { get; set; }

    }
}
