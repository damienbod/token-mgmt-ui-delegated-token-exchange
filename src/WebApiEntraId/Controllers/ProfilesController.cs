using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Net.Http.Headers;
using WebApiEntraId.WebApiDuende;

namespace WebApiEntraId.Controllers
{
    [Authorize]
    [AuthorizeForScopes(Scopes = ["api://72286b8d-5010-4632-9cea-e69e565a5517/user_impersonation"])]
    [ApiController]
    [Route("[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly WebApiDuendeService _apiService;

        public ProfilesController(WebApiDuendeService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var scopeRequiredByApi = new string[] { "access_as_user" };
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            var entraIdBearerToken = Request.Headers[HeaderNames.Authorization]
                .ToString().Replace("Bearer ", string.Empty);

            var dataWebApiDuende = await _apiService.GetWebApiDuendeDataAsync(entraIdBearerToken);

            return dataWebApiDuende;
        }
    }
}
