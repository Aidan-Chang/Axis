using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace Axis.Data.Test;

public class DesHackTest {

  private readonly ITestOutputHelper _output;

  public DesHackTest(ITestOutputHelper output) {
    _output = output;
  }

  [Fact]
  public void Decrypt() {
    // 讀取檔案
    byte[] bytes = File.ReadAllBytes("old.licx");
    string content = Encoding.UTF8.GetString(bytes);
    _output.WriteLine("Step 1:{0}", content);
    byte[] base64 = Convert.FromBase64String(content);
    _output.WriteLine("Step 2:{0}", Encoding.UTF8.GetString(base64));
    // 解密器
    var des = (byte[] base64) => {
      // 將兩個字組成一個字元 DF-D7-B5-4E...
      var key = "DFD7B54E512268154C9AE63F4B18D413BA9F15CD3151365B"
        .Select((x, y) => new { x, y })
        .GroupBy(g => g.y / 2)
        .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), NumberStyles.AllowHexSpecifier)).ToArray();
      var iv = "290A72A4C410522D"
        .Select((x, y) => new { x, y })
        .GroupBy(g => g.y / 2)
        .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), NumberStyles.AllowHexSpecifier)).ToArray();
      var des = TripleDES.Create();
      des.Key = key;
      des.IV = iv;
      var transform = des.CreateDecryptor();
      string value = Encoding.UTF8.GetString(transform.TransformFinalBlock(base64, 0, base64.Length));
      return value;
    };
    string value = des.Invoke(base64);
    _output.WriteLine("Step 3:{0}", value);
    // 反轉
    string valueR = string.Join("", value.Reverse());
    _output.WriteLine("Step 4:{0}", valueR);
    // 兩兩互換
    int[] values = valueR
      .Split(" ")
      .Select((x, y) => new { x, y })
      .GroupBy(g => g.y / 2)
      .Select(s => s.Select(w => int.Parse(w.x)).Reverse()).SelectMany(i => i).ToArray();
    _output.WriteLine("Step 5:{0}", string.Join(" ", values.Select(x => x)));
    // 轉為文字
    string text = string.Join("", values.Select(x => Char.ConvertFromUtf32(x)));
    _output.WriteLine("Step 6:{0}", text);
    Assert.True(text.Length > 0);
  }

  [Fact]
  public void Encrypt() {
    string text = "WIN-ACDNJJ5LPL4\\SQLEXPRESS&AllowUser&0&2023/12/20&11408&2022/12/20&N/A";
    // 轉為數字
    int[] values = text.Select((x, y) => Char.ConvertToUtf32(x.ToString(), 0)).ToArray();
    _output.WriteLine("Step 1:{0}", string.Join(" ", values.Select(x => x)));
    // 兩兩互換
    string[] valurR = values
      .Select((x, y) => new { x, y })
      .GroupBy(g => g.y / 2)
      .Select(s => s.Select(w => w.x.ToString()).Reverse()).SelectMany(i => i).ToArray();
    _output.WriteLine("Step 2:{0}", string.Join(" ", valurR));
    // 空格並反轉
    string value = string.Join("", string.Join(" ", valurR).Reverse());
    _output.WriteLine("Step 3:{0}", value);
    // 加密器
    var des = (string value) => {
      // 轉為字元
      byte[] content = Encoding.UTF8.GetBytes(value);
      // 將兩個字組成一個字元 DF-D7-B5-4E...
      var key = "DFD7B54E512268154C9AE63F4B18D413BA9F15CD3151365B"
        .Select((x, y) => new { x, y })
        .GroupBy(g => g.y / 2)
        .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), NumberStyles.AllowHexSpecifier)).ToArray();
      var iv = "290A72A4C410522D"
        .Select((x, y) => new { x, y })
        .GroupBy(g => g.y / 2)
        .Select(s => Byte.Parse(string.Join("", s.Select(w => w.x)), NumberStyles.AllowHexSpecifier)).ToArray();
      var des = TripleDES.Create();
      des.Key = key;
      des.IV = iv;
      var transform = des.CreateEncryptor();
      byte[] data = transform.TransformFinalBlock(content, 0, content.Length);
      return data;
    };
    byte[] data = des.Invoke(value);
    _output.WriteLine("Step 4:{0}", Encoding.UTF8.GetString(data));
    // 寫入檔案
    string content = Convert.ToBase64String(data);
    _output.WriteLine("Step 5:{0}", content);
    byte[] bytes = Encoding.UTF8.GetBytes(content);
    File.WriteAllBytes("new.licx", bytes);
    Assert.Equal(new FileInfo("new.licx").Length, new FileInfo("old.licx").Length);
  }

}