using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.API.Extensions;
using Identity.API.Models.AccountViewModels;
using Identity.API.Models.TokenViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Controllers;

[Route("api/[controller]")]
public class AuthController : MainController {
  private readonly SignInManager<IdentityUser> _signInManager;
  private readonly UserManager<IdentityUser> _userManager;
  private readonly AppSettings _appSettings;

  public AuthController(UserManager<IdentityUser> userManager,
                        SignInManager<IdentityUser> signInManager,
                        IOptions<AppSettings> appSettings) {
    _userManager = userManager;
    _signInManager = signInManager;
    _appSettings = appSettings.Value;
  }

  [HttpPost("signup")]
  public async Task<IResult> SignUp(SignUp signUp) {
    if (!ModelState.IsValid) return CustomResponse(ModelState);

    var user = new IdentityUser {
      UserName = signUp.Email,
      Email = signUp.Email,
      EmailConfirmed = true
    };

    var result = await _userManager.CreateAsync(user, signUp.Password);

    signUp.Password = string.Empty;

    if (result.Succeeded) {
      return CustomResponse();
    }

    foreach (var error in result.Errors) {
      AddErrorProcess(error.Description);
    }

    return CustomResponse();
  }

  [HttpPost("signin")]
  public async Task<IResult> SignIn(SignIn signIn) {
    if (!ModelState.IsValid) return CustomResponse(ModelState);

    var result = await _signInManager.PasswordSignInAsync(
      signIn.Email, signIn.Password, isPersistent: false, lockoutOnFailure: true);

    if (result.Succeeded) {
      return CustomResponse(await CreateJwT(signIn.Email));
    }

    if(result.IsLockedOut){
      AddErrorProcess("Your account temporarily locked out.");
      return CustomResponse();
    }

    AddErrorProcess("User or password is incorrect.");
    return CustomResponse();
  }

  private async Task<ResponseLogin> CreateJwT(string email) {
    var user = await _userManager.FindByEmailAsync(email);
    var claims = await _userManager.GetClaimsAsync(user);
    var roles = await _userManager.GetRolesAsync(user);

    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
    claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
    claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEchoDate(DateTime.UtcNow).ToString()));
    claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEchoDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

    foreach (var role in roles) {
      claims.Add(new Claim("Role", role));
    }

    var identityClaims = new ClaimsIdentity();
    identityClaims.AddClaims(claims);

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_appSettings.Jti);

    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor {
      Issuer = _appSettings.Iss,
      Audience = _appSettings.Aud,
      Subject = identityClaims,
      Expires = DateTime.UtcNow.AddHours(_appSettings.Exp),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    });

    var encodedToken = tokenHandler.WriteToken(token);

    var response = new ResponseLogin {
      Jti = encodedToken,
      Exp = Convert.ToInt16(TimeSpan.FromHours(_appSettings.Exp).TotalSeconds),
      UserToken = new UserToken {
        Id = user.Id,
        Email = user.Email,
        Claims = claims.Select(s => new UserClaim { Type = s.Type, Value = s.Value })
      }
    };

    return response;
  }

  private static long ToUnixEchoDate(DateTime date) =>
    (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}