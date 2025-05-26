using CoursFlairy.Model.Enum;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

static class CurrentAccount
{
    public static AccountType accountType { get; private set; }
    public static int id { get; private set; } = -1;

    private static readonly string FileName = "Save/Save.json";
    private const string Password = "Password";

    public static void Set(AccountType accountType, int id)
    {
        CurrentAccount.accountType = accountType;
        CurrentAccount.id = id;
    }

    public static void Save()
    {
        var data = JsonSerializer.Serialize(new SaveData(accountType, id), new JsonSerializerOptions { WriteIndented = true });
        EncryptFile(FileName, data, Password);
    }

    public static void Load()
    {
        if (!File.Exists(FileName)) return;

        try
        {
            string decrypted = DecryptFile(FileName, Password);
            var obj = JsonSerializer.Deserialize<SaveData>(decrypted);

            if (obj != null)
            {
                accountType = obj.AccountType;
                id = obj.Id;
            }
        }
        catch
        {
            MessageBox.Show("Помилка завантаження акаунту");
            accountType = AccountType.None;
            id = -1;
        }
    }

    private static void EncryptFile(string path, string data, string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("Salt123");
        using (var aes = Aes.Create())
        {
            var key = new Rfc2898DeriveBytes(password, salt, 10000);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using (var fs = new FileStream(path, FileMode.Create))
            using (var cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(data);
            }
        }
    }

    private static string DecryptFile(string path, string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("Salt123");
        using (var aes = Aes.Create())
        {
            var key = new Rfc2898DeriveBytes(password, salt, 10000);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using (var fs = new FileStream(path, FileMode.Open))
            using (var cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }

    private class SaveData
    {
        public AccountType AccountType { get; set; }
        public int Id { get; set; }


        public SaveData(AccountType accountType, int id, bool isRemember = false)
        {
            AccountType = accountType;
            Id = id;
        }
    }
}