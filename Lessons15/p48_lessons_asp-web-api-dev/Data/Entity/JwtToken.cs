using System;
using p48_lessons_asp.Data.Enum;

namespace p48_lessons_asp.Data.Entity;

// Об'єкт токену.
/*
Яка мета збереження?  Сам по собі токен він залишається незмінним. Це означає, що змінити дату завершення дії ми не можемо.
Тому доведеться токен зберігати у БД, щоб у ній позначати токен як деактивований

*/
public class JwtToken
{
    public Guid Id{get;set;}
    public string Token{get;set;} = string.Empty;
    public TokenType Type{get;set;}
    public Guid? ParentTokenId{get;set;}
    public DateTime ExpirationDate{get;set;}
    public List<TokenJournal>? TokenJournals{get;set;}
}
