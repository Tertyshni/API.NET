namespace p48_lessons_asp.Services.Random;

public interface IRandomService
{
    string RandomString(int length);
    string ConfirmCode(int length);
    string RandomNumberUseDate();
}
