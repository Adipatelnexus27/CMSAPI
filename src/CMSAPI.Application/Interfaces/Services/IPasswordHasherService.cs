namespace CMSAPI.Application.Interfaces.Services;

public interface IPasswordHasherService
{
    (byte[] Hash, byte[] Salt) HashPassword(string password);
    bool Verify(string password, byte[] hash, byte[] salt);
}

