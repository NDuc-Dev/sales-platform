public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public int Price { get; set; } // VND
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }   // ảnh chính
}