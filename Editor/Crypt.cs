using UnityEngine;
using System.Security.Cryptography;

namespace PretiaEditor
{
    public class Crypt
    {
        private const string GLYPHS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string RandomString(int length)
        {
            string str = "";
            for(int i=0; i<length; i++)
            {
                str += GLYPHS[Random.Range(0, GLYPHS.Length)];
            }
            return str;
        }
        
        public static byte[] CalculateSHA256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }
    }
}