using System.IO.Compression;
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
      using (FileStream source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
      using (MemoryStream ms = new MemoryStream())
      using (GZipStream gz = new(ms, CompressionLevel.Optimal)) {
        DirectoryInfo dir = new DirectoryInfo(name);
        if (dir.Exists) dir.GetFiles("*.spt").ToList().ForEach(file => file.Delete());
        else dir.Create();
        // split file every 8MB
        int size = 2 << 22;
        int fileIndex = 1;
        // buffer size 4KB
        byte[] buffer = new byte[2 << 14];
        int length = source.Read(buffer, 0, buffer.Length);
        FileStream target = File.Create(Path.Combine(dir.FullName, fileIndex.ToString("D4") + ".spt"));
        while (length > 0) {
          gz.Write(buffer, 0, buffer.Length);
          byte[] data = ms.ToArray();
          ms.Flush();
          if (((int)target.Length + data.Length) > size) {
            int remain = size - (int)(target.Length);
            target.Write(data, 0, remain);
            target.Close();
            // move to next split file
            fileIndex++;
            target = File.Create(Path.Combine(dir.FullName, fileIndex.ToString("D4") + ".spt"));
            target.Write(data, remain, data.Length);
          }
          else {
            target.Write(data, 0, data.Length);
          }
          length = source.Read(buffer, 0, buffer.Length);
          if (length == 0) {
            target.Close();
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
        using (FileStream target = new FileStream(name + ".onnx", FileMode.Create, FileAccess.Write)) {
          foreach (var file in files) {
            byte[] content = new byte[0];
            using (FileStream source = file.Open(FileMode.Open))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gz = new(source, CompressionMode.Decompress)) {
              gz.CopyTo(ms);
              content = ms.ToArray();
            }
          }
        }
      }
    }
  }
}