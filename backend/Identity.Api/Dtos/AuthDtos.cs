public record RegisterRequest(string Email, string Password, string? FullName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, string Role, string Email);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
