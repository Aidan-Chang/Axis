using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.IO.Compression;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Axis.Ml.Test;

public class ModelCompressTest {

  [Fact]
  public void Model_File_Archieve() {
    string[] names = {
      "arcfaceresnet100-8",
      "depth_googlenet_slim",
      "face_detector_640",
      "landmarks_68_pfld",
      "recognition_resnet27",
    };
    foreach (var name in names) {
      string path = Path.Combine("Models", name + ".onnx");
      using (FileStream input = File.OpenRead(path))
      using (MemoryStream memory = new MemoryStream())
      using (GZipStream gz = new(memory, CompressionLevel.SmallestSize)) {
        // create/remove exists split files
        DirectoryInfo dir = new DirectoryInfo(name);
        if (dir.Exists) dir.GetFiles("*.spt").ToList().ForEach(file => file.Delete());
        else dir.Create();
        // chunk file size is 8MB
        int size = 2 << 22;
        // buffer size 8KB
        byte[] buffer = new byte[2 << 15];
        int index = 1;
        // source file reading
        while (input.Position < input.Length) {
          using (FileStream output = File.OpenWrite(Path.Combine(dir.FullName, index.ToString("D4") + ".spt"))) {
            int remaining = size;
            int length = 0;
            byte[] compress_buffer = new byte[2 << 15];
            // compress bytes to memory
            var compress = () => {
              int source_read_length = input.Read(buffer, 0, buffer.Length);
              if (source_read_length > 0) {
                memory.Position = 0;
                gz.Write(buffer, 0, buffer.Length);
                compress_buffer = memory.ToArray();
                return compress_buffer.Length;
              }
              return 0;
            };
            // write to chunk
            while (remaining > 0 && (length = compress()) > 0) {
              output.Write(compress_buffer, 0, compress_buffer.Length);
              remaining -= length;
            }
            index++;
          }
        }
      }
    }
  }

  [Fact]
  public void Model_File_Load_From_Archieve() {
    string[] names = {
      "arcfaceresnet100-8",
      "depth_googlenet_slim",
      "face_detector_640",
      "landmarks_68_pfld",
      "recognition_resnet27",
    };
    foreach (var name in names) {
      DirectoryInfo dir = new DirectoryInfo(name);
      if (dir.Exists == false) continue;
      FileInfo[] files = dir.GetFiles("*.spt").OrderBy(x => x.Name).ToArray();
      if (files.Length > 0) {
        using (FileStream output = File.Create(name + ".onnx"))
        using(MemoryStream memory = new MemoryStream())
        using (GZipStream gz = new(memory, CompressionMode.Decompress)) {
          // buffer size 8KB
          byte[] buffer = new byte[2 << 15];
          // source chunks files reading
          foreach (var file in files) {
            using (FileStream input = File.OpenRead(file.FullName)) {
              int length = 0;
              while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                // read to memory
                memory.Position = 0;
                memory.SetLength(0);
                memory.Write(buffer, 0, buffer.Length);
                memory.Position = 0;
                byte[] compress_buffer = new byte[2 << 15];
                // decompress bytes to memory
                int decompress_length = 0;
                while ((decompress_length = gz.Read(compress_buffer, 0, compress_buffer.Length)) > 0) {
                  output.Write(compress_buffer, 0, compress_buffer.Length);
                }
              }
            }
          }
        }
      }
    }
  }
}