


public class InnerLogger
{
  private TextWriter _logger = null;
  public TextWriter Logger { get => _logger??Console.Out; set => _logger = value;}
  public bool HasCustomLogger { get => _logger is not null; }

}

