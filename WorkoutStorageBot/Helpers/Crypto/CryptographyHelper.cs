#region using
using System.Security.Cryptography;
#endregion

namespace WorkoutStorageBot.Helpers.Crypto
{
    internal class CryptographyHelper
    {
        internal static string CreateRandomCallBackQueryId(uint size = 5)
            => Convert.ToBase64String(CreateRandomByteArray(size));

        private static byte[] CreateRandomByteArray(uint size = 5)
        {
            byte[] arr = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(arr);
            }

            return arr;
        }
    }
}