using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data {
  public class AppDbContext(DbContextOptions<AppDbContext> options) : 
    IdentityDbContext(options);
}
