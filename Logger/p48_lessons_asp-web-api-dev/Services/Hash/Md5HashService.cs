using System;

namespace p48_lessons_asp.Services.Hash;

/*
MD5 - функція хешування. На сьогоднішній день вважається небезпечною, була взламана. Є високий ризик колізій. Якщо використовуєте обов'язково додавайте сіль у дані.
*/
public class Md5HashService : IHashService
{
    public string Hash(string data)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data)));
    }
}
