using BeautySalonAPI.Models;

namespace BeautySalonAPI.Services.Token;

public interface ITokenService
{ 
    string Generate(User user);
}
