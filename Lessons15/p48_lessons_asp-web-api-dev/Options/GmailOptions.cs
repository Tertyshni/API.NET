using System;

namespace p48_lessons_asp.Options;

public class GmailOptions
{
    public const string SectionName = "GmailOptions";
    public string Host{get;set;} = String.Empty;
    public int Port{get;set;} 
    public string SenderEmail{get;set;} = String.Empty;
    public string Password{get;set;} = String.Empty;
}
