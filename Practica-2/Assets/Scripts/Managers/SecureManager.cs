using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Clase creada para gestionar los hash
/// </summary>
public class SecureManager
{
    /// <summary>
    /// Crea un hash usando SHA256 y lo devuelve
    /// </summary>
    /// <param name="data">Data a "hashear"</param>
    /// <returns></returns>
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
