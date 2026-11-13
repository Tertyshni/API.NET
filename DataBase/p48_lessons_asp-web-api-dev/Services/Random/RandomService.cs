namespace p48_lessons_asp.Services.Random;

public class RandomService : IRandomService
{
    private readonly System.Random random = new();
    private readonly string symbolsDataSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private readonly string _safeChars = new string(
        Enumerable.Range(20, 107).Select(x => (char)x).ToArray());

    public string ConfirmCode(int length)
    {
        string result = "";
        for (int i = 0; i < length; i++)
            result += symbolsDataSet[random.Next(symbolsDataSet.Length)];
        return result;
    }

    public string RandomNumberUseDate() =>
    $"{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{random.Next(1000, 9999)}";

    public string RandomString(int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = _safeChars[random.Next(_safeChars.Length)];
        return new string(chars);
    }
}
