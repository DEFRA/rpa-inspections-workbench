using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace RPA.Inspections.Workbench.Tests.Factory
{
    public static class MockHttpContext
    {
        public static IHttpContextAccessor GetMockedHttpContextAccessor()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "earth\\M600300") }, "Test"))
            };

            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(x => x.HttpContext).Returns(context);

            return accessor.Object;
        }
    }
}
