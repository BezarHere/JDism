

using System.Text.Json;

class Config
{
  private static readonly string _ConfigPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "config.json"
  );

  public static Config Load()
  {
    return JsonSerializer.Deserialize<Config>(
      File.ReadAllText(_ConfigPath));
  }

  


}
