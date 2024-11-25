using HealthConnect.Models;
using HealthConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;
using HealthConnect.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace HealthConnect.Controllers
{
    [Authorize(Roles ="Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorController(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new DoctorRegistrationViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(DoctorRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                string clinicImagePath = null;
                string certificatePath = null;

                // Handle file uploads for Clinic Image
                if (model.ClinicImage != null && model.ClinicImage.Length > 0)
                {
                    clinicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "clinicimages", model.ClinicImage.FileName);
                    using (var stream = new FileStream(clinicImagePath, FileMode.Create))
                    {
                        await model.ClinicImage.CopyToAsync(stream);
                    }
                }

                // Handle file uploads for Certificate
                if (model.Certificate != null && model.Certificate.Length > 0)
                {
                    certificatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "certificates", model.Certificate.FileName);
                    using (var stream = new FileStream(certificatePath, FileMode.Create))
                    {
                        await model.Certificate.CopyToAsync(stream);
                    }
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create a Doctor object and set the properties
                var doctor = new Doctor
                {
                    FullName = model.FullName,
                    Specialization = model.Specialization,
                    MciRegistrationNumber = model.MciRegistrationNumber,
                    Experience = model.Experience,
                    ConsultationFee = model.ConsultationFee,
                    OnlineConsultation = model.OnlineConsultation,
                    ClinicAppointment = model.ClinicAppoinment,
                    ClinicName = model.ClinicName,
                    ClinicImagePath = "/uploads/clinicimages/" + model.ClinicImage.FileName,
                    CertificatePath = "/uploads/certificates/" + model.Certificate.FileName,
                    ApprovalStatus = "Pending" ,// Set default approval status
                    HnoAndStreetName = model.HnoAndStreetName,
                    Place=model.Place,
                    District=model.District,
                    UserId=userId

                };

                // Save the doctor data using the service (which internally uses the repository)
                await _doctorRepository.AddDoctorAsync(doctor);

                return RedirectToAction("Index", "Doctor");
            }

            // If the model is invalid, return the same view with validation errors
            return View(model);
        }

        public IActionResult Feedback()
        {
            return View();
        }
    }
}
