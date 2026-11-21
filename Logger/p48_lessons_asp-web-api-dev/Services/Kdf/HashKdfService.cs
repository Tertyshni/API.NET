using System;
using p48_lessons_asp.Services.Hash;

namespace p48_lessons_asp.Services.Kdf;

public class HashKdfService : IKdfService
{
    private readonly IHashService _hashService;

    public HashKdfService(IHashService hashService)
    {
        _hashService = hashService;
    }
    public string GetDerivedKey(string password, string? salt)
    {
        string result = string.IsNullOrEmpty(salt) ? password : salt + password;
        return _hashService.Hash(result);
    }
}
