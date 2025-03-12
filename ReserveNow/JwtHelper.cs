using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReserveNow
{
    public static class JwtHelper
    {
        public static bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
                return true;

            try
            {
                // Разделяем токен на части
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return true;

                // Декодируем payload
                var payloadBase64 = parts[1];
                var payloadJson = DecodeBase64(payloadBase64);

                // Ищем поле 'exp' (время истечения в секундах)
                var payload = JsonDocument.Parse(payloadJson);
                if (!payload.RootElement.TryGetProperty("exp", out var expProperty))
                    return true;

                var expirationTime = expProperty.GetInt64();
                var expirationDateTime = DateTimeOffset
                    .FromUnixTimeSeconds(expirationTime)
                    .UtcDateTime;

                // Сравниваем с текущим временем
                return DateTime.UtcNow >= expirationDateTime;
            }
            catch
            {
                return true; // В случае ошибки считаем токен недействительным
            }
        }

        private static string DecodeBase64(string base64String)
        {
            // Дополняем '=' для корректного декодирования
            var padded = base64String.PadRight(base64String.Length + (4 - base64String.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(padded);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
