using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BabyPedia.Data;
using BabyPedia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyPedia.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly BabyPediaContext _dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailHandler _emailHandler;

        public RegistrationController(BabyPediaContext dbContext,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, EmailHandler emailHandler)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailHandler = emailHandler;
        }

        public IActionResult ParentRegistration()
        {
            return View();
        }

        [HttpPost("register-parent")]
        public async Task<IActionResult> RegisterParent(Parent parent, [FromForm] string password)
        {
            var roleName = "Parent";
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return Redirect(
                        $"/Registration/ParentRegistration?error=Failed to create role for parent!");
                }
            }
            
            var signInResult = await _userManager.CreateAsync(parent, password);

            if (signInResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(parent, roleName);
                // await _signInManager.SignInAsync(parent, true);

                // Send email confirmation
                return Redirect($"/email/{parent.Id}");
            }
            else
            {
                return Redirect(
                    $"/Registration/ParentRegistration?error={signInResult.Errors.FirstOrDefault().Description}");
            }
        }

        [Authorize(Roles = "Parent")]
        [HttpPost("register-child")]
        public async Task<IActionResult> RegisterChild(BabyPedia.Models.Child child)
        {
            // var roleName = "Parent";
            //
            // var roleExists = await _roleManager.RoleExistsAsync(roleName);
            // if (!roleExists)
            // {
            //     var role = new IdentityRole(roleName);
            //     var result = await _roleManager.CreateAsync(role);
            //     if (!result.Succeeded)
            //     {
            //         return Redirect(
            //             $"/Registration/ParentRegistration?error=Failed to create role for parent!");
            //     }
            // }

            var existingChild = await _dbContext.Children.AsNoTracking().FirstOrDefaultAsync(x =>
                x.Id == child.Id);

            child.Parent = await _dbContext.Parents.FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));

            if (existingChild is null)
            {
                await _dbContext.Children.AddAsync(child);
            }
            else
            {
                existingChild.FirstName = child.FirstName;
                existingChild.LastName = child.LastName;
                existingChild.Sex = child.Sex;
                existingChild.BirthHeight = child.BirthHeight;
                existingChild.BirthWeight = child.BirthWeight;
                existingChild.PlaceOfBirth = child.PlaceOfBirth;
                existingChild.DateTimeCreated = child.DateTimeCreated;

                _dbContext.Children.Update(child);
            }

            await _dbContext.SaveChangesAsync();


            if (User.IsInRole("Parent"))
                return Redirect("/parent/patientrecord");
            return Redirect("/pedia/patientrecord");
        }


        [HttpPost("register-account")]
        public async Task<IActionResult> RegisterUser(Parent parent, [FromForm] string password, [FromForm] string role)
        {
            IdentityUser user = role == "Parent"
                ? parent
                : new PartneredPedia()
                {
                    FirstName = parent.FirstName,
                    LastName = parent.LastName,
                    Address = parent.Address,
                    Email = parent.Email,
                };
            var roleName = role;

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var _role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(_role);
                if (!result.Succeeded)
                {
                    return Redirect(
                        $"/admin/newuser?error=Failed to create role for parent!");
                }
            }

            var signInResult = await _userManager.CreateAsync(user, password);

            if (signInResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);
                // await _signInManager.SignInAsync(user, true);

                return Redirect("/admin");
            }
            else
            {
                return Redirect(
                    $"/admin/newuser?error={signInResult.Errors.FirstOrDefault().Description}");
            }
        }

        [HttpPost("/register-appointment")]
        public async Task<IActionResult> RegisterAppointment(
            [FromForm] string date,
            [FromForm] string time,
            [FromForm] string description,
            [FromForm] string appointmentType,
            [FromForm] string pedia,
            [FromForm] double paid,
            [FromForm] long child
        )
        {
            var userId = _userManager.GetUserId(User);

            var selectedPedia =
                await _dbContext.PartneredPedias.FirstOrDefaultAsync(x => x.Id == pedia);

            var currentParent =
                await _dbContext.Parents.FirstOrDefaultAsync(x => x.Id == userId);

            var selectedChild =
                await _dbContext.Children.FirstOrDefaultAsync(x =>
                    x.Parent.Id == currentParent.Id && child == x.Id);

            //
            // if (selectedChild is null)
            // {
            //     selectedChild = new Child()
            //     {
            //         FirstName = child,
            //         Parent = currentParent
            //     };
            //
            //     await _dbContext.Children.AddAsync(selectedChild);
            // }

            AppointmentType appointmentTypeObject;

            if (await _dbContext.AppointmentTypes.FirstOrDefaultAsync(x =>
                    x.Name == appointmentType) is AppointmentType appointmentTypeCheck)
            {
                appointmentTypeObject = appointmentTypeCheck;
            }
            else
            {
                appointmentTypeObject = (await _dbContext.AppointmentTypes.AddAsync(new AppointmentType()
                {
                    Name = appointmentType
                })).Entity;
            }

            var payment = new AppointmentPayment()
            {
                Amount = paid, Status = "Paid"
            };

            payment = (await _dbContext.AppointmentPayments.AddAsync(payment)).Entity;

            var appointment = new Appointment()
            {
                Name = new Guid().ToString(),
                AppointmentTime = time,
                AppointmentDate = date,
                Payment = payment,
                AppointmentType = appointmentTypeObject,
                Parent = currentParent,
                Pedia = selectedPedia,
                Child = selectedChild
            };

            appointment = (await _dbContext.Appointments.AddAsync(appointment)).Entity;

            currentParent.Appointments.Add(appointment);
            selectedPedia.Appointments.Add(appointment);
            selectedChild.Appointments.Add(appointment);

            _dbContext.Update(currentParent);
            _dbContext.Update(selectedPedia);

            await _dbContext.SaveChangesAsync();

            return Redirect("/parent/appointmentlist");
        }

        [HttpPost("/add-immunization")]
        public async Task<IActionResult> RegisterImmunization(
            ImmunizationRecord immunizationRecord,
            [FromForm] long childId,
            [FromForm] long vaccineId
        )
        {
            var curUser =
                await _dbContext.PartneredPedias.FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
            var child = await _dbContext.Children.FirstOrDefaultAsync(x => x.Id == childId);
            var vaccine = await _dbContext.Vaccines.FirstOrDefaultAsync(x => x.Id == vaccineId);

            immunizationRecord.Vaccine = vaccine.Name;
            immunizationRecord.AdministeredBy = curUser.Email;

            child.ImmunizationRecord.Add(immunizationRecord);

            _dbContext.Update(child);
            await _dbContext.SaveChangesAsync();

            return Redirect("/pedia/immunizationrecord/" + child.Id);
        }

        [HttpPost("/register-pedia")]
        public async Task<IActionResult> RegisterPedia(PartneredPedia parent, [FromForm] string password)
        {
            var roleName = "Pedia";

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return Redirect(
                        $"/Registration/ParentRegistration?error=Failed to create role for parent!");
                }
            }

            var signInResult = await _userManager.CreateAsync(parent, password);

            if (signInResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(parent, roleName);
                // await _signInManager.SignInAsync(parent, true);

                // Send email confirmation
                return Redirect($"/email/{parent.Id}");
            }
            else
            {
                return Redirect(
                    $"/Registration/PediaRegistration?error={signInResult.Errors.FirstOrDefault().Description}");
            }
        }

        public IActionResult PediaRegistration()
        {
            return View();
        }
    }
}