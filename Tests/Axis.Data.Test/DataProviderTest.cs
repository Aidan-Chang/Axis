using Axis.Data.Abstraction;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Data.Test;

public class DataProviderTest {

  private readonly ITestOutputHelper _output;

  public DataProviderTest(ITestOutputHelper output) {
    _output = output;
  }

  [Fact]
  public void Create_Db_File() {
    DatabaseOptions options = new DatabaseOptions() {
      ConnectionName = "default",
      ProviderName = "postgres",
      Server = "localhost",
      DatabaseName = "axis",
      ConnectionString = "Server=localhost;Database=axis;User Id=postgre;Password=1q2w3e4r5t"
    };
    // write to file
    File.Create("default.db").GzCompress(options.ToString());
    Assert.True(File.Exists("default.db"));
  }

  [Fact]
  public void Load_Db_File() {
    string text = File.OpenRead("default.db").GzDecompress();
    DatabaseOptions? options = DatabaseOptions.Load(text);
    Assert.NotNull(options);
    Assert.Equal("Server=localhost;Database=axis;User Id=postgre;Password=1q2w3e4r5t", options.ConnectionString);
  }

  [Fact]
  public void Decrypt() {
    byte[] bytes = File.ReadAllBytes("test.licx");
    string content = Encoding.UTF8.GetString(bytes);
    byte[] base64 = Convert.FromBase64String(content);
    var key = "DFD7B54E512268154C9AE63F4B18D413BA9F15CD3151365B"
      .Select((x, y) => new { x, y })
      .GroupBy(g => g.y / 2)
      .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), System.Globalization.NumberStyles.AllowHexSpecifier)).ToArray();
    var iv = "290A72A4C410522D"
      .Select((x, y) => new { x, y })
      .GroupBy(g => g.y / 2)
      .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), System.Globalization.NumberStyles.AllowHexSpecifier)).ToArray();
    var des = TripleDES.Create();
    des.Key = key;
    des.IV = iv;
    var transform = des.CreateDecryptor();
    string value = Encoding.UTF8.GetString(transform.TransformFinalBlock(base64, 0, base64.Length));
    // 反轉
    string valueR = string.Join("", value.Reverse());
    _output.WriteLine(valueR);
    // 互換
    int[] values = valueR
      .Split(" ")
      .Select((x, y) => new { x, y })
      .GroupBy(g => g.y / 2)
      .Select(s => s.Select(w => int.Parse(w.x)).Reverse()).SelectMany(i => i).ToArray();
    _output.WriteLine(string.Join(" ", values.Select(x => x)));
    // 轉為文字
    string text = string.Join("", values.Select(x => Char.ConvertFromUtf32(x)));
    _output.WriteLine(text);
  }

  [Fact]
  public void Encrypt() {
    string text = "WIN-ACDNJJ5LPL4\\SQLEXPRESS&AllowUser&0&2023/12/20&11408&2022/12/20&N/A";
    int[] values = text.Select((x, y) => Char.ConvertToUtf32(x.ToString(), 1)).ToArray();

  }

}