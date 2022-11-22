using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Axis.Data.Abstraction;

public static class DataUtility {

  /// <summary>
  /// Aes Encrypt
  /// </summary>
  /// <param name="text"></param>
  /// <param name="key"></param>
  /// <param name="iv"></param>
  /// <returns></returns>
  public static string AesEncrypt(string text, string key, string iv) {
    var sourceBytes = Encoding.UTF8.GetBytes(text);
    var aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = Encoding.UTF8.GetBytes(key.PadLeft(16, 'x'));
    aes.IV = Encoding.UTF8.GetBytes(iv.PadLeft(16, 'x'));
    var transform = aes.CreateEncryptor();
    return Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
  }

  /// <summary>
  /// Aes Decrypt
  /// </summary>
  /// <param name="text"></param>
  /// <param name="key"></param>
  /// <param name="iv"></param>
  /// <returns></returns>
  public static string AesDecrypt(string text, string key, string iv) {
    var encryptBytes = Convert.FromBase64String(text);
    var aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;
    aes.Key = Encoding.UTF8.GetBytes(key.PadLeft(16, 'x'));
    aes.IV = Encoding.UTF8.GetBytes(iv.PadLeft(16, 'x'));
    var transform = aes.CreateDecryptor();
    return Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));
  }

  /// <summary>
  /// GZip compress to file
  /// </summary>
  /// <param name="file"></param>
  /// <param name="content"></param>
  public static void GzCompress(this Stream stream, string content) {
    byte[] data = Encoding.UTF8.GetBytes(content);
    using (GZipStream gz = new(stream, CompressionLevel.Optimal)) {
      gz.Write(data, 0, data.Length);
    }
  }

  /// <summary>
  /// GZip Decompress from file
  /// </summary>
  /// <param name="file"></param>
  /// <returns></returns>
  public static string GzDecompress(this Stream stream) {
    byte[] data;
    using (MemoryStream ms = new())
    using (GZipStream gz = new(stream, CompressionMode.Decompress)) {
      gz.CopyTo(ms);
      data = ms.ToArray();
    }
    string text = Encoding.UTF8.GetString(data);
    return text;
  }

}
