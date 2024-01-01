using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Identity.API.Controllers;

[ApiController]
public abstract class MainController : Controller {
  public ICollection<string> Errors = new List<string>();
  
  protected IResult CustomResponse(object result = null) {
    if (ValidOperation()) {
      return TypedResults.Ok(result);
    }

    return TypedResults.BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> {
      {"Message:", Errors.ToArray()}
    }));
  }

  protected IResult CustomResponse(ModelStateDictionary modelState) {
    var errors = modelState.Values.SelectMany(e => e.Errors);
    foreach (var error in errors) {
      AddErrorProcess(error.ErrorMessage);
    }

    return CustomResponse();
  }

  protected bool ValidOperation() {
    return !Errors.Any();
  }

  protected void AddErrorProcess(string error) {
    Errors.Add(error);
  }

  protected void ClearErrorProcess() {
    Errors.Clear();
  }
}