using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server;
using BabyPedia.Models;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using BabyPedia.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BabyPedia.Controllers;

public class HomeController : Controller
{
    private static Random _random = new Random();
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly BabyPediaContext _babyPediaContext;
    private readonly EmailHandler _emailHandler;
    private readonly IServerAddressesFeature _addressesFeature;

    public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager, BabyPediaContext babyPediaContext, EmailHandler emailHandler)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _babyPediaContext = babyPediaContext;
        _emailHandler = emailHandler;

        if (User is { } user && _userManager.GetUserAsync(User).Result is { } identityUser)
            BabyPediaContext.CurrentUserId = identityUser.Id;
    }

    public IActionResult Home()
    {
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public async Task<string> GetAccessToken()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://api.sandbox.paypal.com");

        string clientId = "AbhPFeMhKFkyF6Xv-9ft8iO5HPmYcYTLA8QeZXmfA5Yqwl8MDbJtrpsOKaLT37Le0tTzR9ssUM1uLxqD";
        string secret = "EEVR-vvxoS1HFWEoQ-Ub9jg9e_nT-QCGCPARxt28NvDfCT-8awWyyse5ARIp-cb8IIni-ZZY-yfBRDCc";

        var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{secret}");
        var authString = Convert.ToBase64String(authBytes);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);

        var body = new StringContent("grant_type=client_credentials", Encoding.UTF8,
            "application/x-www-form-urlencoded");

        var response = await client.PostAsync("/v1/oauth2/token", body);

        var responseString = await response.Content.ReadAsStringAsync();

        return JObject.Parse(responseString)["access_token"].ToString();
    }

    [HttpPost]
    [Route("api/verifyPayment")]
    public async Task<IActionResult> VerifyPayment([FromBody] dynamic paymentDetails, [FromQuery] bool isPedia = true)
    {
        dynamic payment = JObject.Parse(paymentDetails.ToString());

        string accessToken = await GetAccessToken();

        _logger.LogInformation("Payment received!");
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://api.sandbox.paypal.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.GetAsync($"/v2/checkout/orders/{payment.id}");

        if (response.IsSuccessStatusCode)
        {
            var paymentAmount = Double.Parse(payment.purchase_units[0].amount.value.ToString());
            if (isPedia)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var currentPedia =
                    await _babyPediaContext.PartneredPedias.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

                var pediaPayment = new PediaPayment()
                {
                    Payment = paymentAmount,
                    PartneredPedia = currentPedia
                };

                await _babyPediaContext.PediaPayments.AddAsync(pediaPayment);
                await _babyPediaContext.SaveChangesAsync();
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var currentParent = await _babyPediaContext.Parents.FirstOrDefaultAsync(x => x.Id == currentUser.Id);

                var pediaPayment = new AppointmentPayment()
                {
                    Amount = paymentAmount,
                    Parent = currentParent,
                    Status = string.Empty
                };

                await _babyPediaContext.AppointmentPayments.AddAsync(pediaPayment);
                await _babyPediaContext.SaveChangesAsync();
            }

            // Payment verification succeeded!
            return Redirect("/pedia/subscriptionpayment");
        }
        else
        {
            // Payment verification failed!
            return BadRequest();
        }
    }

    [HttpGet("/reset/{code}")]
    public async Task<IActionResult> ForgotPassword(string code)
    {
        return View(await _userManager.FindByIdAsync(code));
    }


    private static Dictionary<int, string> shortenedTokens =
        new Dictionary<int, string>();


    [HttpGet("/reset/password/view")]
    public async Task<IActionResult> ResetPasswordForm()
    {
        return View();
    }
    

    [HttpPost("/reset/password")]
    public async Task<IActionResult> ResetPasswordSent([FromForm] string email, [FromForm] string newpass)
    {
        var user = await _userManager.FindByEmailAsync(email);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, newpass);

        return Redirect("/home/login?info=Password set!");
    }
    

    [HttpGet("/test/sendemail")]
    public async Task<IActionResult> SendEmailTest()
    {
        return Content($"{HttpContext.Connection.LocalPort}");
    }

    [HttpPost("/admin/emails/send")]
    public async Task<IActionResult> SendEmail([FromForm] string email, [FromForm] string subject,
        [FromForm] string message)
    {
        await _emailHandler.SendEmail(email, subject, message);
        return Redirect($"/admin/sendEmail?info=Email Sent to {email}!");
    }

    [HttpPost("/reset/password/send")]
    public async Task<IActionResult> SendEmail([FromForm] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Redirect($"/home/login?error=That email doesn't exist in our system!");
        }

        await _emailHandler.SendEmail(email, "BabyPedia Forgot Password",
            @$"Please verify your account https://babypediaph.com/reset/{user.Id}");
        
        return Redirect($"/admin/sendEmail?info=Reset password sent to {email}!");
    }

    [HttpGet("/email/{id}")]
    public async Task<IActionResult> SendEmailConfirmation(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var shortenedCode = _random.Next(10000, 99999);

        shortenedTokens.Add(shortenedCode, token);

        await _emailHandler.SendEmail(user.Email, "BabyPedia Verification",
            @$"Please verify your account https://babypediaph.com/emailconfirm/{id}?token={shortenedCode}");

        return Redirect("/home/login?info=Please check your email for verification!");
    }

    [HttpGet("/emailconfirm/{id}")]
    public async Task<IActionResult> ConfirmEmail(string id, [FromQuery] int token)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user.EmailConfirmed)
            return Redirect("/home/login?info=Already Verified!");

        string realToken = string.Empty;
        if (shortenedTokens.TryGetValue(token, out var sToken))
        {
            realToken = sToken;
        }

        var confirmationResult = await _userManager.ConfirmEmailAsync(user, realToken);

        return Redirect("/home/login?info=Verified!");
    }

    [Authorize(Roles = "Parent")]
    [HttpGet("/parent/currentappointment/{id}")]
    public async Task<IActionResult> ParentCurrentAppointment(long id)
    {
        // var currentUserId = _userManager.GetUserId(User);
        // var currentParent = await _babyPediaContext.Parents
        //     .Include(x => x.Appointments)
        //     .ThenInclude(x => x.AppointmentType)
        //     .Include(x => x.Appointments)
        //     .ThenInclude(x => x.Pedia)
        //     .Include(x => x.Appointments)
        //     .ThenInclude(x => x.Child)
        //     .Include(x => x.Appointments)
        //     .ThenInclude(x => x.Payment)
        //     .FirstOrDefaultAsync(x => x.Id == currentUserId);
        //
        return View(await _babyPediaContext.Appointments
            .Include(x => x.Pedia)
            .Include(x => x.Parent)
            .Include(x => x.Child)
            .Include(x => x.AppointmentType)
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id == id));
    }

    [Authorize(Roles = "Pedia")]
    [HttpGet("/pedia/appointment/{id}")]
    public async Task<IActionResult> PediaChat(long id)
    {
        return View(await _babyPediaContext.Appointments
            .Include(x => x.Child)
            .Include(x => x.Parent)
            .Include(x => x.Payment)
            .Include(x => x.Pedia)
            .Include(x => x.AppointmentType)
            .FirstOrDefaultAsync(x => x.Id == id));
    }

    [Authorize(Roles = "Pedia,Parent")]
    [HttpGet("/parent/childinformation/{id}")]
    [HttpGet("/pedia/childinformation/{id}")]
    public async Task<IActionResult> ChildInformation(long id)
    {
        return View(await _babyPediaContext.Children.FirstOrDefaultAsync(x => x.Id == id));
    }


    [Authorize(Roles = "Pedia,Parent")]
    [HttpGet("/parent/immunizationrecord/{id}")]
    [HttpGet("/pedia/immunizationrecord/{id}")]
    public async Task<IActionResult> immunizationrecord(long id)
    {
        return View(await _babyPediaContext.Children.Include(x => x.ImmunizationRecord)
            .FirstOrDefaultAsync(x => x.Id == id));
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("/verify/{id}")]
    public async Task<IActionResult> Verification(string id)
    {
        return View(await _babyPediaContext.PartneredPedias.FirstOrDefaultAsync(x => x.Id == id));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("/user/delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var identityResult = await _userManager.DeleteAsync(user);

        return Redirect($"/admin?message={user.UserName} has been deleted!");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("/confirm/{id}")]
    public async Task<IActionResult> ConfirmPedia(string id)
    {
        var foundPedia = await _babyPediaContext.PartneredPedias.FirstOrDefaultAsync(x => x.Id == id);
        foundPedia.IsVerified = !foundPedia.IsVerified;
        _babyPediaContext.Update(foundPedia);
        await _babyPediaContext.SaveChangesAsync();

        return Redirect(
            $"/admin?message={foundPedia.UserName} is {(foundPedia.IsVerified ? "Verified!" : "Unverified!")}!"
        );
    }

    [Authorize(Roles = "Parent,Pedia")]
    [HttpGet("/prescriptions/{id}")]
    public async Task<IActionResult> Prescription(long id)
    {
        return View(await _babyPediaContext.Appointments
            .Include(x => x.Child)
            .Include(x => x.Parent)
            .Include(x => x.Pedia)
            .Include(x => x.Payment)
            .Include(x => x.AppointmentType)
            .FirstOrDefaultAsync(x => x.Id == id));
    }

    [Authorize(Roles = "Pedia")]
    [HttpGet("/prescribe/{id}")]
    public async Task<IActionResult> PrescribeForm(long id)
    {
        return View(await _babyPediaContext.Appointments
            .Include(x => x.Child)
            .Include(x => x.Parent)
            .Include(x => x.Payment)
            .Include(x => x.AppointmentType)
            .FirstOrDefaultAsync(x => x.Id == id));
    }

    [Authorize(Roles = "Pedia")]
    [HttpPost("/prescribe/add")]
    public async Task<IActionResult> AddPrescription(Appointment appointment)
    {
        var exisintAppointment = await _babyPediaContext.Appointments.FirstOrDefaultAsync(x => x.Id == appointment.Id);
        exisintAppointment.Prescription = appointment.Prescription;

        _babyPediaContext.Update(exisintAppointment);
        await _babyPediaContext.SaveChangesAsync();

        return Redirect("/pedia/appointmentlist");
    }


    [Authorize(Roles = "Pedia,Admin")]
    [HttpGet("/vaccines/{id}")]
    public async Task<IActionResult> VaccineInformation(long id)
    {
        return View(await _babyPediaContext.Vaccines.FirstOrDefaultAsync(x => x.Id == id));
    }


    [HttpGet("/logout")]
    public async Task<IActionResult> LogoutUser()
    {
        await _signInManager.SignOutAsync();

        return Redirect("/");
    }

    [HttpGet("/admin/download/logs")]
    [HttpGet("/audit/")]
    public async Task<IActionResult> GetAudit()
    {
        // await _babyPediaContext.AuditLogs.AddAsync(new AuditLog()
        // {
        //     Id = new Guid().ToString(),
        //     Table = "Test"
        // });
        //
        // await _babyPediaContext.SaveChangesAsync();
        //
        // return Content("OK");

        var currentUserId = await _userManager.GetUserAsync(User);
        var auditLogs = await _babyPediaContext.AuditLogs
            .ToListAsync();

        if (!User.IsInRole("Admin"))
        {
            auditLogs = auditLogs.Where(x => x.UserId == currentUserId.Id).ToList();
        }

        StringBuilder sb = new StringBuilder();
        foreach (var auditLog in auditLogs)
        {
            sb.AppendLine(
                $"{auditLog.UpdateTime.ToString()},{auditLog.Table},{auditLog.TableColumn},{auditLog.OldValue},{auditLog.NewValue}");
        }

        return new FileContentResult(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv");
    }

    [HttpPost("/login")]
    public async Task<IActionResult> LoginUser([FromForm] string username, [FromForm] string password)
    {
        var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(username);

            BabyPediaContext.CurrentUserId = user.Id;

            if (await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Redirect("/parent");
            }

            if (await _userManager.IsInRoleAsync(user, "Pedia"))
            {
                return Redirect("/pedia");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Redirect("/admin/");
            }
        }

        var existingUser = await _userManager.FindByNameAsync(username);
        if (!existingUser.EmailConfirmed)
            return Redirect(
                $"/home/login?error=Please confirm your email!");

        return Redirect(
            $"/home/login?error=Bad credentials!");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}