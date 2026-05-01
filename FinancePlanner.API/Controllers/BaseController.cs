using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinancePlanner.API.Controllers
{

    public abstract class BaseController : ControllerBase
    {
        protected int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            return claim is not null && int.TryParse(claim.Value, out var id)
                ? id
                : null;
        }
    }
}