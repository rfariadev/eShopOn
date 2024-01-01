namespace Identity.API;

class Program {
  static void Main(string[] args) {
    CreateWebBuilder(args).Build().Run();
  }

  public static IHostBuilder CreateWebBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
      .ConfigureWebHostDefaults(webBuilder => {
        webBuilder.UseStartup<Startup>();
      });
}