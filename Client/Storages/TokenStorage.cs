using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Client.Storages
{
    public static class TokenStorage
    {
        private static readonly string _filePath;
        private static readonly byte[] _key;
        private static readonly byte[] _iv;

        static TokenStorage()
        {
            _filePath = "env.enc";
            _key = Encoding.UTF8.GetBytes("Viv9jfe@GO@!#Fn7wzF)2qIz[uq8@PTN");
            _iv = Encoding.UTF8.GetBytes("mb{&NDqfBq^x({Ui");
        }

        public static async Task<bool> SaveTokenAsync(string token)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            await writer.WriteAsync(token);
                        }
                        await File.WriteAllBytesAsync(_filePath, memoryStream.ToArray());
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<string?> LoadTokenAsync()
        {
            if (!File.Exists(_filePath))
                return null;

            try
            {
                byte[] encryptedData = await File.ReadAllBytesAsync(_filePath);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool DeleteToken()
        {
            if (!File.Exists(_filePath))
                return true;

            try
            {
                File.Delete(_filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
