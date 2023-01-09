using System.IO.Compression;

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
    // chunk file size is 8MB
    const int size = 2 << 22;
    // split files to chunk
    foreach (var name in names) {
      string path = Path.Combine("Models", name + ".onnx");
      using (FileStream input = File.OpenRead(path))
      using (ChunkFileStream output = new ChunkFileStream(name, size))
      using (GZipStream gz = new(output, CompressionLevel.Optimal)) {
        // buffer size 8KB
        byte[] buffer = new byte[2 << 15];
        // source file reading
        while (input.Read(buffer, 0, buffer.Length) > 0) {
          // compress and write to chunk stream
          gz.Write(buffer, 0, buffer.Length);
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
      using (FileStream output = File.Create(name + ".bak"))
      using (ChunkFileStream input = new ChunkFileStream(name))
      using (GZipStream gz = new(input, CompressionMode.Decompress)) {
        // buffer size 8KB
        byte[] buffer = new byte[2 << 15];
        // target file writing
        while (gz.Read(buffer, 0, buffer.Length) != 0) {
          // decompress and write to target file stream
          output.Write(buffer, 0, buffer.Length);
        }
      }
    }
  }

}

public class ChunkFileStream : Stream {

  public override bool CanRead => true;

  public override bool CanSeek => throw new NotSupportedException();

  public override bool CanWrite => true;

  public override long Length => throw new NotSupportedException();

  public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

  public string Path { get; }

  public int Size { get; }

  public int Index { get; private set; }

  public FileStream? Current { get; private set; }

  private DirectoryInfo directory;
  public ChunkFileStream(string path, int size = 0) {
    Path = path;
    Size = size;
    Index = 1;
    // create new or remove exists chunk files
    directory = new DirectoryInfo(path);
    if (directory.Exists) directory.GetFiles().ToList().ForEach(file => file.Delete());
    else directory.Create();
  }

  public override int Read(byte[] buffer, int offset, int count) {
    string chunk_file_name = System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3"));
    if (File.Exists(chunk_file_name) == false) {
      return 0;
    }
    if (Current == null) {
      Current = File.OpenRead(chunk_file_name);
    }
    int size = Current.Read(buffer, 0, buffer.Length);
    if (size == 0) {
      Index++;
      return -1;
    }
    return size;
  }

  private int file_remaining = 0;
  public override void Write(byte[] buffer, int offset, int count) {
    if (Size < (2 << 19)) {
      throw new InvalidOperationException("Chunk size have to greater than 1MB");
    }
    if (Current == null) {
      Current = File.Create(System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3")));
      file_remaining = Size;
    }
    int buffer_remaining = buffer.Length;
    while (buffer_remaining > 0) {
      if (file_remaining <= 0) {
        Index++;
        Current = File.Create(System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3")));
        file_remaining = Size;
      }
      int size = Math.Min(file_remaining, buffer_remaining);
      byte[] content = new byte[size];
      Array.Copy(buffer, buffer.Length - buffer_remaining, content, 0, size);
      Current.Write(content, 0, content.Length);
      buffer_remaining -= content.Length;
      file_remaining -= content.Length;
    }
  }

  public override void Flush() {
    throw new NotSupportedException();
  }

  public override long Seek(long offset, SeekOrigin origin) {
    throw new NotSupportedException();
  }

  public override void SetLength(long value) {
    throw new NotSupportedException();
  }

}