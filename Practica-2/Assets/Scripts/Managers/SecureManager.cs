using System.Security.Cryptography;
using System.Text;

public class SecureManager
{
    public static string Hash(string data)
    {
        SHA256Managed mySha256 = new SHA256Managed();
        byte[] textToBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashValue = mySha256.ComputeHash(textToBytes);
        return GetHexStringFromHash(hashValue);
    }

    private static string GetHexStringFromHash(byte[] hash)
    {
        string hexString = string.Empty;
        foreach (byte b in hash)
        {
            hexString += b.ToString("x2");
        }
        return hexString;
    }
}
