//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json.Linq;
//using Revoulter.Core.Models;
//using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Security.Authentication;
//using System.Text;
//using System.Threading.Tasks;

//[ApiController]
//[AllowAnonymous]
//[Route("api")]
//public class PrivyAuthController : ControllerBase
//{
//    private readonly IConfiguration _config;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly SignInManager<ApplicationUser> _signInManager;

//    private readonly string? _appId;
//    private readonly string? _appSecret;
//    private readonly string? _baseUrl;
//    private readonly string? _origin;

//    public PrivyAuthController(
//        IConfiguration config,
//        UserManager<ApplicationUser> userManager,
//        SignInManager<ApplicationUser> signInManager)
//    {
//        _config = config;
//        _userManager = userManager;
//        _signInManager = signInManager;

//        // Use configuration values (commented out for now)
//        //_appId = _config["Privy:AppId"];
//        //_appSecret = _config["Privy:AppSecret"];
//        //_baseUrl = _config["Privy:BaseUrl"] ?? "https://auth.privy.io";
//        //_origin = _config["Privy:Origin"];

//        // Hardcoded values for testing
//        _appId = "cmjlp21o905gxl10czfonofy8";
//        _appSecret = "privy_app_secret_LA7oCTWvYyBpqcBdHYYW9PKTpwbbujttQ3GZRyaNsePz31zzzY88uiUdszxDPNqRPoiwsfwFjvuFZpMx9FWQ4YK";
//        _baseUrl = "https://auth.privy.io";
//        _origin = "http://localhost:5115";

//        // Ensure TLS 1.2 is available globally
//        EnsureTlsSupport();
//    }

//    private void EnsureTlsSupport()
//    {
//        // Ensure TLS 1.2 is available (important for .NET Framework/older .NET Core)
//        try
//        {
//            // For .NET Framework compatibility, but it's safe to use in .NET Core too
//            System.Net.ServicePointManager.SecurityProtocol |=
//                System.Net.SecurityProtocolType.Tls12 |
//                System.Net.SecurityProtocolType.Tls13;
//        }
//        catch
//        {
//            // Ignore if not available (on newer .NET versions)
//        }
//    }

//    private string GetAuthHeader()
//    {
//        if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_appSecret))
//            throw new InvalidOperationException("Privy AppId or AppSecret is missing in configuration.");

//        var credentials = $"{_appId}:{_appSecret}";
//        return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";
//    }

//    private HttpClient CreateHttpClient()
//    {
//        var handler = new HttpClientHandler();

//        // Force TLS 1.2 (fixes most SSL/TLS connection issues)
//        handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

//        // Add timeout handling
//        handler.MaxConnectionsPerServer = 20;

//        // For development only - allows self-signed certificates
//#if DEBUG
//        handler.ServerCertificateCustomValidationCallback =
//            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
//#endif

//        var client = new HttpClient(handler);

//        // Set reasonable timeout
//        client.Timeout = TimeSpan.FromSeconds(30);

//        return client;
//    }

//    [HttpPost("send-code")]
//    public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
//    {
//        if (request == null || (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Phone)))
//            return BadRequest(new { error = "Email or phone is required" });

//        try
//        {
//            // Create handler with specific settings
//            var handler = new HttpClientHandler
//            {
//                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
//                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
//            };

//            using var client = new HttpClient(handler);
//            client.Timeout = TimeSpan.FromSeconds(30);

//            // Build exact JSON like Node.js does
//            string jsonPayload;
//            if (!string.IsNullOrEmpty(request.Email))
//            {
//                jsonPayload = $"{{\"email\":\"{request.Email}\"}}";
//            }
//            else
//            {
//                jsonPayload = $"{{\"phone\":\"{request.Phone}\"}}";
//            }

//            Console.WriteLine($"📤 Payload: {jsonPayload}");
//            Console.WriteLine($"📍 URL: {_baseUrl}/api/v1/passwordless/init");

//            // Create request
//            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/passwordless/init");

//            // CRITICAL: Use ByteArrayContent and set headers manually
//            var contentBytes = Encoding.UTF8.GetBytes(jsonPayload);
//            var content = new ByteArrayContent(contentBytes);

//            // Clear any default headers and set exactly what we need
//            content.Headers.Clear();
//            content.Headers.Add("Content-Type", "application/json");

//            httpRequest.Content = content;

//            // Add headers in exact order as Node.js
//            httpRequest.Headers.TryAddWithoutValidation("Authorization", GetAuthHeader());
//            httpRequest.Headers.TryAddWithoutValidation("privy-app-id", _appId);
//            httpRequest.Headers.TryAddWithoutValidation("Origin", _origin ?? "http://localhost:5115");

//            // Debug: Print all headers
//            Console.WriteLine("\n📋 Request Headers:");
//            foreach (var header in httpRequest.Headers)
//            {
//                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//            }

//            Console.WriteLine("\n📋 Content Headers:");
//            foreach (var header in httpRequest.Content.Headers)
//            {
//                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//            }

//            // Send request
//            var response = await client.SendAsync(httpRequest);
//            var responseBody = await response.Content.ReadAsStringAsync();

//            Console.WriteLine($"\n📥 Status: {(int)response.StatusCode} {response.ReasonPhrase}");
//            Console.WriteLine($"📥 Response: {responseBody}");

//            Console.WriteLine("\n📋 Response Headers:");
//            foreach (var header in response.Headers)
//            {
//                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//            }

//            if (!response.IsSuccessStatusCode)
//            {
//                try
//                {
//                    var errorData = JObject.Parse(responseBody);
//                    return StatusCode((int)response.StatusCode, errorData);
//                }
//                catch
//                {
//                    return StatusCode((int)response.StatusCode, new
//                    {
//                        error = "Failed to send code",
//                        details = responseBody,
//                        statusCode = (int)response.StatusCode
//                    });
//                }
//            }

//            Console.WriteLine("✅ Code sent successfully!");
//            return Ok(new { success = true, message = "Code sent successfully" });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Exception: {ex.GetType().Name}");
//            Console.WriteLine($"❌ Message: {ex.Message}");
//            Console.WriteLine($"❌ Stack: {ex.StackTrace}");

//            return StatusCode(500, new
//            {
//                error = "Internal server error",
//                message = ex.Message
//            });
//        }
//    }

//    [HttpPost("verify-code")]
//    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
//    {
//        if (request == null || string.IsNullOrEmpty(request.Code))
//            return BadRequest(new { error = "Verification code is required" });

//        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Phone))
//            return BadRequest(new { error = "Email or phone is required" });

//        try
//        {
//            var handler = new HttpClientHandler
//            {
//                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
//                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
//            };

//            using var client = new HttpClient(handler);
//            client.Timeout = TimeSpan.FromSeconds(30);

//            // Build exact JSON
//            string jsonPayload;
//            if (!string.IsNullOrEmpty(request.Email))
//            {
//                jsonPayload = $"{{\"email\":\"{request.Email}\",\"code\":\"{request.Code}\"}}";
//            }
//            else
//            {
//                jsonPayload = $"{{\"phone\":\"{request.Phone}\",\"code\":\"{request.Code}\"}}";
//            }

//            Console.WriteLine($"📤 Verify Payload: {jsonPayload}");
//            Console.WriteLine($"📍 URL: {_baseUrl}/api/v1/passwordless/authenticate");

//            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/passwordless/authenticate");

//            var contentBytes = Encoding.UTF8.GetBytes(jsonPayload);
//            var content = new ByteArrayContent(contentBytes);

//            content.Headers.Clear();
//            content.Headers.Add("Content-Type", "application/json");

//            httpRequest.Content = content;

//            httpRequest.Headers.TryAddWithoutValidation("Authorization", GetAuthHeader());
//            httpRequest.Headers.TryAddWithoutValidation("privy-app-id", _appId);
//            httpRequest.Headers.TryAddWithoutValidation("Origin", _origin ?? "http://localhost:5115");

//            // Send request
//            var response = await client.SendAsync(httpRequest);
//            var responseString = await response.Content.ReadAsStringAsync();

//            Console.WriteLine($"\n📥 Status: {(int)response.StatusCode} {response.ReasonPhrase}");
//            Console.WriteLine($"📥 Response Body: {responseString}");

//            // Check if response is empty
//            if (string.IsNullOrWhiteSpace(responseString))
//            {
//                return StatusCode((int)response.StatusCode, new
//                {
//                    error = "Empty response from Privy API",
//                    statusCode = (int)response.StatusCode
//                });
//            }

//            // Try to parse JSON
//            JObject data;
//            try
//            {
//                data = JObject.Parse(responseString);
//            }
//            catch (Exception parseEx)
//            {
//                Console.WriteLine($"❌ JSON Parse Error: {parseEx.Message}");
//                return StatusCode((int)response.StatusCode, new
//                {
//                    error = "Invalid response format from Privy API",
//                    response = responseString,
//                    parseError = parseEx.Message
//                });
//            }

//            // ✅ IMPORTANT: Return Privy's error message to the frontend
//            if (!response.IsSuccessStatusCode)
//            {
//                var errorMessage = data["error"]?.ToString()
//                    ?? data["message"]?.ToString()
//                    ?? "Verification failed";

//                Console.WriteLine($"❌ Privy Error: {errorMessage}");
//                Console.WriteLine($"❌ Full Response: {data}");

//                return StatusCode((int)response.StatusCode, new
//                {
//                    error = errorMessage,
//                    details = data,
//                    statusCode = (int)response.StatusCode
//                });
//            }

//            Console.WriteLine("✅ Verification successful!");

//            // Extract Privy's unique user ID
//            string? privyUserId = data["user"]?["id"]?.ToString();
//            if (string.IsNullOrEmpty(privyUserId))
//            {
//                Console.WriteLine("⚠️ Warning: No user ID in response");
//                Console.WriteLine($"Response data: {data}");
//                return StatusCode(500, new
//                {
//                    error = "Privy user ID not returned",
//                    response = data
//                });
//            }

//            Console.WriteLine($"✅ User authenticated: {privyUserId}");

//            // Try to find existing user by Privy ID first
//            ApplicationUser? user = await _userManager.FindByIdAsync(privyUserId);

//            // Fallback: search by email or phone
//            if (user == null && !string.IsNullOrEmpty(request.Email))
//            {
//                user = await _userManager.FindByEmailAsync(request.Email);
//                Console.WriteLine($"Found user by email: {user != null}");
//            }

//            if (user == null && !string.IsNullOrEmpty(request.Phone))
//            {
//                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone);
//                Console.WriteLine($"Found user by phone: {user != null}");
//            }

//            // If still not found, create new user using Privy data
//            if (user == null)
//            {
//                Console.WriteLine("Creating new user...");
//                user = new ApplicationUser
//                {
//                    Id = privyUserId,
//                    UserName = privyUserId,
//                    Email = request.Email,
//                    PhoneNumber = request.Phone,
//                    EmailConfirmed = !string.IsNullOrEmpty(request.Email),
//                    PhoneNumberConfirmed = !string.IsNullOrEmpty(request.Phone)
//                };

//                var createResult = await _userManager.CreateAsync(user);
//                if (!createResult.Succeeded)
//                {
//                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
//                    Console.WriteLine($"❌ Failed to create user: {errors}");
//                    return StatusCode(500, new
//                    {
//                        error = "Failed to create user",
//                        details = createResult.Errors
//                    });
//                }

//                await _userManager.AddToRoleAsync(user, "User");
//                Console.WriteLine($"✅ New user created: {user.Id}");
//            }
//            else
//            {
//                Console.WriteLine($"Updating existing user: {user.Id}");
//                bool needsUpdate = false;

//                if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
//                {
//                    user.Email = request.Email;
//                    user.EmailConfirmed = true;
//                    needsUpdate = true;
//                }

//                if (!string.IsNullOrEmpty(request.Phone) && user.PhoneNumber != request.Phone)
//                {
//                    user.PhoneNumber = request.Phone;
//                    user.PhoneNumberConfirmed = true;
//                    needsUpdate = true;
//                }

//                if (needsUpdate)
//                {
//                    await _userManager.UpdateAsync(user);
//                    Console.WriteLine("✅ User updated");
//                }
//            }

//            // Sign in the user
//            await _signInManager.SignInAsync(user, isPersistent: true);
//            Console.WriteLine("✅ User signed in successfully");

//            return Ok(new
//            {
//                success = true,
//                redirect = "/",
//                user = data["user"],
//                token = data["token"],
//                refresh_token = data["refresh_token"],
//                local_user_id = user.Id
//            });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Exception: {ex.GetType().Name}");
//            Console.WriteLine($"❌ Message: {ex.Message}");
//            Console.WriteLine($"❌ Stack: {ex.StackTrace}");

//            return StatusCode(500, new
//            {
//                error = "Internal server error",
//                message = ex.Message,
//                type = ex.GetType().Name
//            });
//        }
//    }

//    [HttpGet("user")]
//    public async Task<IActionResult> GetUser()
//    {
//        if (!Request.Headers.TryGetValue("Authorization", out var authValues) ||
//            !authValues.ToString().StartsWith("Bearer "))
//            return Unauthorized(new { error = "No token provided" });

//        var token = authValues.ToString()["Bearer ".Length..].Trim();

//        try
//        {
//            using var client = CreateHttpClient();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            client.DefaultRequestHeaders.Add("privy-app-id", _appId);
//            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Revoulter/1.0)");

//            var response = await client.GetAsync($"{_baseUrl}/api/v1/users/me");
//            var responseString = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                try
//                {
//                    var errorData = JObject.Parse(responseString);
//                    return StatusCode((int)response.StatusCode, errorData);
//                }
//                catch
//                {
//                    return StatusCode((int)response.StatusCode, new
//                    {
//                        error = "Failed to get user",
//                        details = responseString
//                    });
//                }
//            }

//            var data = JObject.Parse(responseString);

//            // Also return local user info if available
//            var privyUserId = data["id"]?.ToString();
//            ApplicationUser? localUser = null;
//            if (!string.IsNullOrEmpty(privyUserId))
//            {
//                localUser = await _userManager.FindByIdAsync(privyUserId);
//            }

//            return Ok(new
//            {
//                privy_user = data,
//                local_user = localUser != null ? new
//                {
//                    id = localUser.Id,
//                    email = localUser.Email,
//                    phone = localUser.PhoneNumber,
//                    roles = await _userManager.GetRolesAsync(localUser)
//                } : null
//            });
//        }
//        catch (TaskCanceledException)
//        {
//            return StatusCode(408, new
//            {
//                error = "Request timeout",
//                message = "User info request timed out."
//            });
//        }
//        catch (HttpRequestException ex)
//        {
//            return StatusCode(502, new
//            {
//                error = "Network error",
//                message = ex.Message
//            });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new
//            {
//                error = "Internal server error",
//                message = ex.Message
//            });
//        }
//    }

//    [HttpGet("health")]
//    public async Task<IActionResult> Health()
//    {
//        // Test both internal health and Privy API connectivity
//        var healthInfo = new
//        {
//            server_status = "running",
//            server_time = DateTime.UtcNow,
//            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
//        };

//        try
//        {
//            // Test Privy API connectivity
//            using var client = CreateHttpClient();
//            client.Timeout = TimeSpan.FromSeconds(10);

//            var privyResponse = await client.GetAsync($"{_baseUrl}/api/v1/health");
//            var privyHealth = new
//            {
//                status = privyResponse.IsSuccessStatusCode ? "connected" : "disconnected",
//                status_code = (int)privyResponse.StatusCode
//            };

//            return Ok(new
//            {
//                server = healthInfo,
//                privy_api = privyHealth
//            });
//        }
//        catch (Exception ex)
//        {
//            return Ok(new
//            {
//                server = healthInfo,
//                privy_api = new
//                {
//                    status = "disconnected",
//                    error = ex.Message
//                },
//                warning = "Privy API is not accessible. Check network connection and SSL/TLS configuration."
//            });
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Revoulter.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class PrivyAuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    private readonly string? _appId;
    private readonly string? _appSecret;
    private readonly string? _baseUrl;
    private readonly string? _origin;

    public PrivyAuthController(
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _config = config;
        _userManager = userManager;
        _signInManager = signInManager;

        _appId = "cmjlp21o905gxl10czfonofy8";
        _appSecret = "privy_app_secret_LA7oCTWvYyBpqcBdHYYW9PKTpwbbujttQ3GZRyaNsePz31zzzY88uiUdszxDPNqRPoiwsfwFjvuFZpMx9FWQ4YK";
        _baseUrl = "https://auth.privy.io";
        _origin = "http://localhost:5115";

        EnsureTlsSupport();
    }

    private void EnsureTlsSupport()
    {
        try
        {
            System.Net.ServicePointManager.SecurityProtocol |=
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls13;
        }
        catch
        {
        }
    }

    private string GetAuthHeader()
    {
        if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_appSecret))
            throw new InvalidOperationException("Privy AppId or AppSecret is missing in configuration.");

        var credentials = $"{_appId}:{_appSecret}";
        return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler();
        handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        handler.MaxConnectionsPerServer = 20;

#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif

        var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);

        return client;
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
    {
        if (request == null || (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Phone)))
            return BadRequest(new { error = "Email or phone is required" });

        try
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);

            string jsonPayload;
            if (!string.IsNullOrEmpty(request.Email))
            {
                jsonPayload = $"{{\"email\":\"{request.Email}\"}}";
            }
            else
            {
                jsonPayload = $"{{\"phone\":\"{request.Phone}\"}}";
            }

            Console.WriteLine($"📤 Payload: {jsonPayload}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/passwordless/init");

            var contentBytes = Encoding.UTF8.GetBytes(jsonPayload);
            var content = new ByteArrayContent(contentBytes);

            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/json");

            httpRequest.Content = content;

            httpRequest.Headers.TryAddWithoutValidation("Authorization", GetAuthHeader());
            httpRequest.Headers.TryAddWithoutValidation("privy-app-id", _appId);
            httpRequest.Headers.TryAddWithoutValidation("Origin", _origin ?? "http://localhost:5115");

            var response = await client.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorData = JObject.Parse(responseBody);
                    return StatusCode((int)response.StatusCode, errorData);
                }
                catch
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "Failed to send code",
                        details = responseBody
                    });
                }
            }

            Console.WriteLine("✅ Code sent successfully!");
            return Ok(new { success = true, message = "Code sent successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception: {ex.Message}");
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = ex.Message
            });
        }
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Code))
            return BadRequest(new { error = "Verification code is required" });

        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Phone))
            return BadRequest(new { error = "Email or phone is required" });

        try
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);

            string jsonPayload;
            if (!string.IsNullOrEmpty(request.Email))
            {
                jsonPayload = $"{{\"email\":\"{request.Email}\",\"code\":\"{request.Code}\"}}";
            }
            else
            {
                jsonPayload = $"{{\"phone\":\"{request.Phone}\",\"code\":\"{request.Code}\"}}";
            }

            Console.WriteLine($"📤 Verify Payload: {jsonPayload}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/passwordless/authenticate");

            var contentBytes = Encoding.UTF8.GetBytes(jsonPayload);
            var content = new ByteArrayContent(contentBytes);

            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/json");

            httpRequest.Content = content;

            httpRequest.Headers.TryAddWithoutValidation("Authorization", GetAuthHeader());
            httpRequest.Headers.TryAddWithoutValidation("privy-app-id", _appId);
            httpRequest.Headers.TryAddWithoutValidation("Origin", _origin ?? "http://localhost:5115");

            var response = await client.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {(int)response.StatusCode}");

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return StatusCode((int)response.StatusCode, new
                {
                    error = "Empty response from Privy API"
                });
            }

            JObject data;
            try
            {
                data = JObject.Parse(responseString);
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"❌ JSON Parse Error: {parseEx.Message}");
                return StatusCode(500, new
                {
                    error = "Invalid response format from Privy API"
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = data["error"]?.ToString() ?? "Verification failed";
                Console.WriteLine($"❌ Privy Error: {errorMessage}");

                return StatusCode((int)response.StatusCode, new
                {
                    error = errorMessage,
                    details = data
                });
            }

            Console.WriteLine("✅ Verification successful!");

            string? privyUserId = data["user"]?["id"]?.ToString();
            if (string.IsNullOrEmpty(privyUserId))
            {
                return StatusCode(500, new { error = "Privy user ID not returned" });
            }

            Console.WriteLine($"✅ User authenticated: {privyUserId}");

            ApplicationUser? user = await _userManager.FindByIdAsync(privyUserId);

            if (user == null && !string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }

            if (user == null && !string.IsNullOrEmpty(request.Phone))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone);
            }

            if (user == null)
            {
                Console.WriteLine("Creating new user...");

                // ✅ FIX: Use email/phone as username instead of Privy ID
                string userName = request.Email ?? request.Phone ?? privyUserId.Replace(":", "_");

                user = new ApplicationUser
                {
                    Id = privyUserId,
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    Email = request.Email,
                    NormalizedEmail = request.Email?.ToUpperInvariant(),
                    PhoneNumber = request.Phone,
                    EmailConfirmed = !string.IsNullOrEmpty(request.Email),
                    PhoneNumberConfirmed = !string.IsNullOrEmpty(request.Phone),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"❌ Failed to create user: {errors}");
                    return StatusCode(500, new
                    {
                        error = "Failed to create user",
                        details = errors
                    });
                }

                try
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                catch (Exception roleEx)
                {
                    Console.WriteLine($"⚠️ Role assignment failed: {roleEx.Message}");
                }

                Console.WriteLine($"✅ New user created: {user.Id}");
            }
            else
            {
                Console.WriteLine($"Updating existing user: {user.Id}");
                bool needsUpdate = false;

                if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
                {
                    user.Email = request.Email;
                    user.NormalizedEmail = request.Email.ToUpperInvariant();
                    user.EmailConfirmed = true;
                    needsUpdate = true;
                }

                if (!string.IsNullOrEmpty(request.Phone) && user.PhoneNumber != request.Phone)
                {
                    user.PhoneNumber = request.Phone;
                    user.PhoneNumberConfirmed = true;
                    needsUpdate = true;
                }

                // ✅ FIX: Ensure UserName is always set
                if (string.IsNullOrEmpty(user.UserName))
                {
                    user.UserName = request.Email ?? request.Phone ?? user.Id;
                    user.NormalizedUserName = user.UserName.ToUpperInvariant();
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await _userManager.UpdateAsync(user);
                    Console.WriteLine("✅ User updated");
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            Console.WriteLine("✅ User signed in successfully");

            return Ok(new
            {
                success = true,
                redirect = "/",
                user = data["user"],
                token = data["token"],
                refresh_token = data["refresh_token"],
                local_user_id = user.Id
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception: {ex.Message}");
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = ex.Message
            });
        }
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authValues) ||
            !authValues.ToString().StartsWith("Bearer "))
            return Unauthorized(new { error = "No token provided" });

        var token = authValues.ToString()["Bearer ".Length..].Trim();

        try
        {
            using var client = CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("privy-app-id", _appId);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Revoulter/1.0)");

            var response = await client.GetAsync($"{_baseUrl}/api/v1/users/me");
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorData = JObject.Parse(responseString);
                    return StatusCode((int)response.StatusCode, errorData);
                }
                catch
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "Failed to get user",
                        details = responseString
                    });
                }
            }

            var data = JObject.Parse(responseString);

            var privyUserId = data["id"]?.ToString();
            ApplicationUser? localUser = null;
            if (!string.IsNullOrEmpty(privyUserId))
            {
                localUser = await _userManager.FindByIdAsync(privyUserId);
            }

            return Ok(new
            {
                privy_user = data,
                local_user = localUser != null ? new
                {
                    id = localUser.Id,
                    email = localUser.Email,
                    phone = localUser.PhoneNumber,
                    roles = await _userManager.GetRolesAsync(localUser)
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = ex.Message
            });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var healthInfo = new
        {
            server_status = "running",
            server_time = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        };

        try
        {
            using var client = CreateHttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var privyResponse = await client.GetAsync($"{_baseUrl}/api/v1/health");
            var privyHealth = new
            {
                status = privyResponse.IsSuccessStatusCode ? "connected" : "disconnected",
                status_code = (int)privyResponse.StatusCode
            };

            return Ok(new
            {
                server = healthInfo,
                privy_api = privyHealth
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                server = healthInfo,
                privy_api = new
                {
                    status = "disconnected",
                    error = ex.Message
                }
            });
        }
    }
}