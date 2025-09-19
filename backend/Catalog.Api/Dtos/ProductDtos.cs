public record ProductListItemDto(int Id, string Name, string Brand, int Price);
public record ProductDetailDto(int Id, string Name, string Brand, int Price);
public record ProductCreateDto(string Name, string Brand, int Price, string? Description);
public record ProductUpdateDto(string Name, string Brand, int Price, string? Description);