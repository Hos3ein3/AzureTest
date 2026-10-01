path = "src/AzureTest.MVC/Controllers/AuthController.cs"
with open(path, 'r') as f:
    c = f.read()

# Fix Login and RefreshToken (which have Result<Value> with Value.Success, Value.Token, Value.RefreshToken, Value.Errors)
# Let's just do targeted replaces.

c = c.replace(
"""        if (result.Success)
        {
            if (_featureOptions.UseJWT)
            {
                // Store JWT in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                Response.Cookies.Append("jwt", result.Token!, cookieOptions);
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
                }
            }""",
"""        if (result.IsSuccess && result.Value.Success)
        {
            if (_featureOptions.UseJWT)
            {
                // Store JWT in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                Response.Cookies.Append("jwt", result.Value.Token!, cookieOptions);
                if (!string.IsNullOrEmpty(result.Value.RefreshToken))
                {
                    Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
                }
            }""")

c = c.replace(
"""        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        return View();""",
"""        if (result.IsSuccess && result.Value.Errors != null)
        {
            foreach (var error in result.Value.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        return View();""", 1)

# Fix Register
c = c.replace(
"""        if (result.Success)
        {
            return RedirectToAction(nameof(Login));
        }

        if (result.Errors != null)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }""",
"""        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Login));
        }

        if (result.Error != null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
        }""")

# Fix RefreshToken
c = c.replace(
"""        if (result.Success)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            
            Response.Cookies.Append("jwt", result.Token!, cookieOptions);
            
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            }
            
            return Ok();
        }""",
"""        if (result.IsSuccess && result.Value.Success)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            
            Response.Cookies.Append("jwt", result.Value.Token!, cookieOptions);
            
            if (!string.IsNullOrEmpty(result.Value.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
            }
            
            return Ok();
        }""")

with open(path, 'w') as f:
    f.write(c)

