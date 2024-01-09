#region using
using System.Security.Cryptography;
#endregion

namespace WorkoutStorageBot.Helpers.Crypto
{
    internal class Cryptography
    {
        internal static byte[] CreateRandomByteArray(int size = 5)
        {
            var mas = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(mas);
            }

            return mas;
        }

        internal static string CreateRandomCallBackQueryId(int size = 5)
        {
            return Convert.ToBase64String(CreateRandomByteArray(size));
        }
    }
}