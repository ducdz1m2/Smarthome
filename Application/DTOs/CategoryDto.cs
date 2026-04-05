namespace Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto> Children { get; set; } = new();
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
