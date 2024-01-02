#region using
using System.Security.Cryptography;
#endregion

namespace WorkoutStorageBot.Helpers.Crypto
{
    internal class Cryptography
    {
        public static byte[] CreateRandomByteArray(int size = 5)
        {
            var mas = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(mas);
            }

            return mas;
        }
    }
}