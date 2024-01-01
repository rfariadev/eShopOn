namespace Identity.API.Models.TokenViewModel;

public class ResponseLogin {
  public string Jti { get; set; }
  public int Exp { get; set; }
  public UserToken UserToken { get; set; }
}

public class UserToken {
  public string Id { get; set; }
  public string Email { get; set; }
  public IEnumerable<UserClaim> Claims { get; set; }
}

public class UserClaim {
  public string Type { get; set; }
  public string Value { get; set; }
}