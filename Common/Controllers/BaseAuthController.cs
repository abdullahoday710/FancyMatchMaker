using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Common.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BaseAuthController : ControllerBase
    {
        protected long? UserId
        {
            get
            {
                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

                if (long.TryParse(subClaim, out var userId))
                {
                    return userId;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
