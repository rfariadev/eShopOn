namespace Identity.API.Extensions;

public class AppSettings {
  public string Jti { get; set; }
  public int Exp { get; set; }
  public string Iss { get; set; }
  public string Aud { get; set; }
}