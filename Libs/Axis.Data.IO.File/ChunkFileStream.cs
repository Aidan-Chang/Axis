using System.IO;

namespace Axis.Data.IO;

public class ChunkFileStream : Stream {

  #region Base Parameters

  public override bool CanRead => true;

  public override bool CanSeek => throw new NotSupportedException();

  public override bool CanWrite => true;

  public override long Length => throw new NotSupportedException();

  public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

  #endregion Base Parameters

  public string Path { get; }

  public int Size { get; }

  public int Index { get; private set; }

  public FileStream? Current { get; private set; }

  private DirectoryInfo directory;

  public ChunkFileStream(string path, ChunkMode mode, int size = 0) {
    Path = path;
    Size = size;
    Index = 1;
    directory = new DirectoryInfo(path);
    switch (mode) {
      case ChunkMode.Split:
        if (size < (2 << 19)) {
          throw new ArgumentException("Size could not be grater then 1MB in split mode");
        }
        // create new or remove exists chunk files
        if (directory.Exists) directory.GetFiles().ToList().ForEach(file => file.Delete());
        else directory.Create();
        break;
      case ChunkMode.Merge:
      default:
        break;
    }
  }

  public override int Read(byte[] buffer, int offset, int count) {
    string chunk_file_name = System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3"));
    if (File.Exists(chunk_file_name) == false) {
      return 0;
    }
    if (Current == null) {
      Current = File.OpenRead(chunk_file_name);
    }
    int size = Current.Read(buffer, 0, count);
    if (size == 0) {
      Current.Close();
      Index++;
      chunk_file_name = System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3"));
      // without next chunk file
      if (File.Exists(chunk_file_name) == false) {
        return 0;
      }
      Current = File.OpenRead(chunk_file_name);
      return -1;
    }
    return size;
  }

  private int file_remaining = 0;
  public override void Write(byte[] buffer, int offset, int count) {
    if (Current == null) {
      Current = File.Create(System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3")));
      file_remaining = Size;
    }
    int buffer_remaining = count;
    while (buffer_remaining > 0) {
      if (file_remaining <= 0) {
        Index++;
        Current.Close();
        Current = File.Create(System.IO.Path.Combine(Path, directory.Name + "." + Index.ToString("D3")));
        file_remaining = Size;
      }
      int size = Math.Min(file_remaining, buffer_remaining);
      byte[] content = new byte[size];
      Array.Copy(buffer, count - buffer_remaining, content, 0, size);
      Current.Write(content, 0, content.Length);
      buffer_remaining -= content.Length;
      file_remaining -= content.Length;
    }
  }

  public override void Close() {
    if (Current != null) {
      Current.Close();
    }
    base.Close();
  }

  #region Base Method

  public override void Flush() {
    throw new NotSupportedException();
  }

  public override long Seek(long offset, SeekOrigin origin) {
    throw new NotSupportedException();
  }

  public override void SetLength(long value) {
    throw new NotSupportedException();
  }

  #endregion Base Method

}
