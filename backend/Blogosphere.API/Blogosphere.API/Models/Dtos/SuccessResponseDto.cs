namespace Blogosphere.API.Models.Dtos;

public record SuccessResponseDto(string Token, string Message)
{
   public string Token { get; init; } = Token;
   public string Message { get; init; } = Message;
}
