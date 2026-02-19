using System.Security.Cryptography;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Crypto
{
    internal class CryptographyHelper
    {
        internal static string CreateRandomCallBackQueryId(uint byteSize = 5)
            => Convert.ToBase64String(CreateRandomByteArray(byteSize));

        private static byte[] CreateRandomByteArray(uint byteSize = 5)
        {
            byte[] arr = new byte[byteSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(arr);
            }

            return arr;
        }
    }
}