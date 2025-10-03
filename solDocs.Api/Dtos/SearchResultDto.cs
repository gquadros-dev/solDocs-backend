namespace solDocs.Dtos
{
    public class SearchResultDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Snippet { get; set; }
    }
}