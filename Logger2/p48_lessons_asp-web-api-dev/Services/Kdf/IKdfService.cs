using System;

namespace p48_lessons_asp.Services.Kdf;

public interface IKdfService
{
    string GetDerivedKey(string password, string? salt);
}
