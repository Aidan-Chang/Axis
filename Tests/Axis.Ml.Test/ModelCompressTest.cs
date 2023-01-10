using Axis.Data.IO;
using System.IO.Compression;

namespace Axis.Ml.Test;

public class ModelCompressTest {

  [Fact]
  public void Model_File_Archieve() {
    string[] names = {
      "face_detector_640",
      "arcfaceresnet100-8",
      "depth_googlenet_slim",
      "landmarks_68_pfld",
      "recognition_resnet27",
    };
    // chunk file size is 8MB
    const int size = 2 << 22;
    // split files to chunk
    foreach (var name in names) {
      string path = Path.Combine("Models", name + ".onnx");
      using (FileStream input = File.OpenRead(path))
      using (ChunkFileStream output = new ChunkFileStream(name, ChunkMode.Split, size))
      //using (FileStream output = File.Create(Path.Combine(name, name + ".gz")))
      using (GZipStream gz = new(output, CompressionMode.Compress, false)) {
        input.CopyTo(gz);
      }
    }
  }

  [Fact]
  public void Model_File_Load_From_Archieve() {
    string[] names = {
      "face_detector_640",
      "arcfaceresnet100-8",
      "depth_googlenet_slim",
      "landmarks_68_pfld",
      "recognition_resnet27",
    };
    foreach (var name in names) {
      using (FileStream output = File.Create(name + ".bak"))
      using (ChunkFileStream input = new ChunkFileStream(name, ChunkMode.Merge))
      using (GZipStream gz = new(input, CompressionMode.Decompress, false)) {
        gz.CopyTo(output);
      }
    }
  }

}