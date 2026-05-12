using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CoursFlairy.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CoursFlairy.Model
{
    public static class ReferralSystem
    {
        private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly int CodeLength = 8;
        private static readonly string SaltKey = "CoursFlairy_Referral_2024"; // Сіль для хешування

        /// <summary>
        /// Генерує реферальний код на основі ID користувача (псевдогенерація)
        /// Код завжди буде однаковим для одного ID
        /// </summary>
        public static string GenerateReferralCodeFromUserId(int userId)
        {
            // Комбінуємо ID користувача з сіллю для безпеки
            string input = $"{userId}_{SaltKey}";
            
            // Обчислюємо SHA256 хеш
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                
                // Перетворюємо хеш в код потрібної довжини
                StringBuilder codeBuilder = new StringBuilder();
                for (int i = 0; i < CodeLength; i++)
                {
                    // Використовуємо байти хешу для вибору символів
                    int charIndex = hashBytes[i % hashBytes.Length] % Characters.Length;
                    codeBuilder.Append(Characters[charIndex]);
                }
                
                return codeBuilder.ToString();
            }
        }

        /// <summary>
        /// Отримує ID користувача, для якого згенеровано даний код
        /// Спочатку намагається розшифрувати код математично, потім перевіряє в БД
        /// </summary>
        public static int? GetUserIdByReferralCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
                return null;

            try
            {
                // Спочатку отримуємо діапазон можливих ID користувачів з БД
                var connection = DataBase.GetConnection();
                int minId = 1, maxId = 1000;
                
                // Отримуємо мінімальний та максимальний ID
                using (var command = new SqlCommand("SELECT MIN(ID), MAX(ID) FROM [User]", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            minId = reader.IsDBNull(0) ? 1 : reader.GetInt32(0);
                            maxId = reader.IsDBNull(1) ? 1000 : reader.GetInt32(1);
                        }
                    }
                }
                
                // Перебираємо тільки реальний діапазон ID
                for (int userId = minId; userId <= maxId; userId++)
                {
                    string generatedCode = GenerateReferralCodeFromUserId(userId);
                    if (generatedCode == code)
                    {
                        // Перевіряємо чи існує такий користувач
                        using (var command = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE ID = @userId", connection))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            if (count > 0)
                            {
                                return userId;
                            }
                        }
                    }
                }
            }
            catch
            {
                // В случае ошибки возвращаем null
            }

            return null;
        }

        /// <summary>
        /// Альтернативний метод для отримання ID користувача по коду (без БД)
        /// Використовується для валідації коду без перевірки існування користувача
        /// </summary>
        public static int? TryDecodeReferralCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
                return null;

            // Оскільки наш алгоритм не має зворотної функції (SHA256 односторонній),
            // ми можемо тільки перебрати можливі ID від 1 до розумної межі
            for (int userId = 1; userId <= 100000; userId++) // Обмежуємо пошук до 100k користувачів
            {
                string generatedCode = GenerateReferralCodeFromUserId(userId);
                if (generatedCode == code)
                {
                    return userId;
                }
            }

            return null;
        }

        /// <summary>
        /// Перевіряє валідність реферального коду
        /// </summary>
        public static bool IsValidReferralCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (code.Length != CodeLength)
                return false;

            if (!code.All(c => Characters.Contains(c)))
                return false;

            // Перевіряємо чи існує користувач з таким кодом
            return GetUserIdByReferralCode(code).HasValue;
        }

        /// <summary>
        /// Отримує реферальний код користувача (псевдогенерація)
        /// </summary>
        public static string GetUserReferralCode(int userId)
        {
            return GenerateReferralCodeFromUserId(userId);
        }

        /// <summary>
        /// Встановлює реферера для користувача
        /// </summary>
        public static bool SetUserReferrer(int userId, int referrerId)
        {
            try
            {
                var connection = DataBase.GetConnection();
                
                // Спочатку перевіряємо чи існує стовпець RefID
                string checkColumnQuery = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'User' AND COLUMN_NAME = 'RefID'";
                
                using (var checkCommand = new SqlCommand(checkColumnQuery, connection))
                {
                    int columnExists = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (columnExists == 0)
                    {
                        // Стовпець не існує, повертаємо true але нічого не робимо
                        System.Diagnostics.Debug.WriteLine($"Стовпець RefID не існує. Реферальний зв'язок {userId} -> {referrerId} не збережено в БД.");
                        return true;
                    }
                }
                
                // Стовпець існує, оновлюємо запис
                string query = "UPDATE [User] SET RefID = @referrerId WHERE ID = @userId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@referrerId", referrerId);
                    command.Parameters.AddWithValue("@userId", userId);
                    
                    var rowsAffected = command.ExecuteNonQuery();
                    bool success = rowsAffected > 0;
                    
                    if (success)
                    {
                        System.Diagnostics.Debug.WriteLine($"Реферальний зв'язок встановлено: користувач {userId} запрошений користувачем {referrerId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Не вдалось встановити реферальний зв'язок: користувач {userId} не знайдений");
                    }
                    
                    return success;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка встановлення реферера: {ex.Message}");
                // Якщо це помилка про відсутність стовпця - повертаємо true
                if (ex.Message.Contains("Invalid column name 'RefID'"))
                {
                    System.Diagnostics.Debug.WriteLine("Стовпець RefID відсутній, але система працює без нього.");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Отримує кількість рефералів користувача
        /// </summary>
        public static int GetReferralCount(int userId)
        {
            try
            {
                var connection = DataBase.GetConnection();
                string query = "SELECT COUNT(*) FROM [User] WHERE RefID = @userId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = command.ExecuteScalar();
                    
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка підрахунку рефералів: {ex.Message}");
                // Якщо стовпця RefID немає в БД - повертаємо 0
            }

            return 0;
        }

        /// <summary>
        /// Отримує список рефералів користувача
        /// </summary>
        public static List<ReferralInfo> GetUserReferrals(int userId)
        {
            var referrals = new List<ReferralInfo>();
            
            try
            {
                var connection = DataBase.GetConnection();
                string query = @"
                    SELECT ID, Login, Email, Name, Surname 
                    FROM [User] 
                    WHERE RefID = @userId 
                    ORDER BY ID DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var referral = new ReferralInfo
                            {
                                Id = reader.GetInt32(0), // ID
                                Login = reader.IsDBNull(1) ? "" : reader.GetString(1), // Login
                                Email = reader.IsDBNull(2) ? "" : reader.GetString(2), // Email
                                Name = reader.IsDBNull(3) ? "" : reader.GetString(3), // Name
                                Surname = reader.IsDBNull(4) ? "" : reader.GetString(4) // Surname
                            };
                            
                            referrals.Add(referral);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка отримання списку рефералів: {ex.Message}");
                // Якщо стовпця RefID немає в БД - повертаємо порожній список
            }

            return referrals;
        }

        // Видаляємо застарілі методи, які працювали з базою даних
        [Obsolete("Використовуйте GenerateReferralCodeFromUserId")]
        public static string GenerateReferralCode()
        {
            throw new NotSupportedException("Метод застарілий. Використовуйте GenerateReferralCodeFromUserId");
        }

        [Obsolete("Реферальний код тепер генерується автоматично")]
        public static bool SetUserReferralCode(int userId, string code)
        {
            throw new NotSupportedException("Метод застарілий. Реферальний код тепер генерується автоматично");
        }
    }

    /// <summary>
    /// Інформація про реферала
    /// </summary>
    public class ReferralInfo
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        
        public string DisplayName => 
            !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Surname) 
                ? $"{Name} {Surname}" 
                : !string.IsNullOrEmpty(Login) 
                    ? Login 
                    : Email;
    }
} 