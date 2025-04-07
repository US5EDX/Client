using System.ComponentModel.DataAnnotations;

namespace Client.Validation
{
    public static class Validation
    {
        public static string? ValidateLoginData(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return "Введіть пошту та пароль";

            if (!new EmailAddressAttribute().IsValid(email) || password.Length < 3)
                return "Неправильна пошта або пароль";

            return null;
        }

        public static bool ValidateEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }
    }
}
