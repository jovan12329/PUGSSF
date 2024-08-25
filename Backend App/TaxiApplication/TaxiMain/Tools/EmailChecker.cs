using System.Text.RegularExpressions;

namespace TaxiMain.Tools
{
    public class EmailChecker
    {

        public static bool IsValidEmail(string email)
        {
            const string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }


    }
}
