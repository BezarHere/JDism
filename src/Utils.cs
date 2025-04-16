

static class Utils
{

  public static IEnumerable<FileInfo> IterateFiles(string path)
  {
    Stack<DirectoryInfo> dirs = new();
    dirs.Push(new DirectoryInfo(path));


    while (dirs.TryPop(out DirectoryInfo current))
    {
      foreach (FileInfo f in current.GetFiles())
      {
        yield return f;
      }
      
      foreach (DirectoryInfo dir in current.GetDirectories())
      {
        dirs.Push(dir);
      }
    }
  }

}

