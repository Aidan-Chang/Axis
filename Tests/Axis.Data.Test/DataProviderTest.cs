using Axis.Data.Abstraction;
using Xunit.Abstractions;

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
      ConnectionString = "Server=localhost;Database=axis;User Id=axis;Password=1q2w3e4r5t"
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
    Assert.Equal("Server=localhost;Database=axis;User Id=axis;Password=1q2w3e4r5t", options.ConnectionString);
  }

}