

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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
