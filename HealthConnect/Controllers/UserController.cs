using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthConnect.Models;
using HealthConnect.Repositories;
using System.Security.Claims;
using HealthConnect.ViewModels;

namespace HealthConnect.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IConfiguration _configuration;
        private const string apiKey = "AIzaSyDoQ2jdmGKmKALVj977-JCYZ7jUT6J6OHA";  // Use your Google API key
        private const string apiUrl = "https://maps.googleapis.com/maps/api/geocode/json";
        private readonly MedicineRepository _medicine;
        public UserController(UserManager<User> userManager, IDoctorRepository doctorRepository, IConfiguration configuration, MedicineRepository medicine)
        {
            _userManager = userManager;
            _doctorRepository = doctorRepository;
            _configuration = configuration;
            _medicine = medicine;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FindDoctors()
        {
            ViewData["GoogleMapsApiKey"] = _configuration["GoogleMaps:ApiKey"];
            return View();
        }

        public IActionResult FindDoctors(DoctorFilterViewModel filter)
        {
            // Fetch doctors based on location and search term
            var doctors = _doctorRepository.GetDoctorsByLocationAndSpecialization(filter.Location, filter.SearchString);

            // Initialize the filter view model with the fetched doctors
            filter.Doctors = doctors.ToList();

            // Return the doctors list with an empty filter model
            return RedirectToAction("DoctorsNearYou", new { location = filter.Location, searchString = filter.SearchString });
        }


        // Display doctors with filters applied (gender, experience, etc.)
        public IActionResult DoctorsNearYou(string location, string searchString, DoctorFilterViewModel filter)
        {
            // Fetch doctors based on location and search string
            var doctors = _doctorRepository.GetDoctorsByLocationAndSpecialization(location, searchString);

            // Apply filters if provided
            var filteredDoctors = _doctorRepository.ApplyFilters(doctors, filter);

            // Update the filter model with the filtered doctor list
            filter.Doctors = filteredDoctors.ToList();

            // Return the filtered list of doctors to the view
            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime date,int isOnline,AppointmentStatus? status, int? appointmentId)
        {
             var doctor = await _doctorRepository.SearchDoctorAsync(doctorId);
            if (doctor == null)
            {
                TempData["Message"] = "Doctor not found.";
                return RedirectToAction("FindDoctors");
            }

            var availableSlots = await _doctorRepository.GetAvailableSlotsAsync(doctorId, date);
            //Filter the slots by current time
            if (date.Date == DateTime.Now.Date)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                availableSlots = availableSlots
                    .Where(slot =>
                    {
                        // Parse slot string to TimeSpan
                        if (TimeSpan.TryParse(slot, out var slotTime))
                        {
                            return slotTime > currentTime; // Compare with current time
                        }
                        return false;
                    })
                    .ToList();
            }
            //if (availableSlots == null)
            //{
            //    availableSlots = new List<string>();  // Or any other default value you prefer
            //}

            ViewBag.AvailableSlots = availableSlots.ToList();
            ViewBag.SelectedDate = date;
            ViewBag.IsOnline = isOnline;
            if(status == AppointmentStatus.ReScheduled) ViewBag.AppointmentStatus = status;
            else ViewBag.AppointmentStatus = AppointmentStatus.Scheduled;
            if (appointmentId != null) ViewBag.AppointmentId = appointmentId;
            ViewData["Title"] = "Book Appointment";
            return View(doctor);
        }

        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null || appointment.AppointmentDate.Add(TimeSpan.Parse(appointment.Slot)).Subtract(DateTime.Now).TotalHours < 3)
            {
                TempData["Error"] = "Appointment cannot be canceled within 3 hours of the scheduled time.";
                return RedirectToAction("ProfileDashboard","Account");
            }

            await _doctorRepository.CancelAppointmentAsync(appointment);
            appointment.Status = AppointmentStatus.Canceled;
            TempData["Success"] = "Appointment canceled successfully.";
            return View();

           
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int doctorId, string selectedSlot, DateTime date,int isOnline,AppointmentStatus? status,int? appointmentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            if (string.IsNullOrEmpty(selectedSlot))
            {
                TempData["Message"] = "Please select a valid time slot.";
                return RedirectToAction("BookAppointment", new { doctorId, date });
            }
            bool consultationType;
            if (isOnline == 1) consultationType = true;
            else consultationType = false;

            if(appointmentId!=null)
            {
                Appointment appointment1 = await _doctorRepository.GetAppointmentByIdAsync((int)appointmentId);
                appointment1.AppointmentDate= date;
                appointment1.Slot = selectedSlot;
                await _doctorRepository.RescheduleAppointment(appointment1);
                return RedirectToAction("ProfileDashboard", "Account");
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), // Assuming logged-in user
                Slot = selectedSlot,
                AppointmentDate = date,
                IsOnline=consultationType,
            };
            if(status==AppointmentStatus.Scheduled) await _doctorRepository.AddAppointmentAsync(appointment);
            else await _doctorRepository.RescheduleAppointment(appointment);

            if (status == null) TempData["Message"] = "Appointment booked successfully!";
            else TempData["Message"] = "Appointment Rescheduled Successfully";
            ViewBag.AppointmentDate = date;
            ViewBag.TimeSlot = selectedSlot;

            return View();
        }



        // GET: Edit Profile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);  // Get logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if user not found
            }

            return View(user);  // Return the profile data to the view
        }

        // POST: Edit Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User updatedUser, IFormFile profilePhoto)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(updatedUser);
            }

            var user = await _userManager.GetUserAsync(User);  // Get logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if user not found
            }

            // Update the user properties
            user.UserName = updatedUser.UserName;
            user.DateofBirth = updatedUser.DateofBirth;
            user.Gender = updatedUser.Gender;
            user.BloodGroup = updatedUser.BloodGroup;
            user.HouseNumber = updatedUser.HouseNumber;
            user.Street = updatedUser.Street;
            user.City = updatedUser.City;
            user.State = updatedUser.State;
            user.Country = updatedUser.Country;
            user.PostalCode = updatedUser.PostalCode;

            // Handle profile photo upload
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePhoto.CopyToAsync(memoryStream);
                    user.ProfilePhoto = memoryStream.ToArray();  // Store the profile photo as byte array
                }
            }

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index","Home");  // Redirect to profile page on success
            }

            // Handle errors (e.g., validation errors)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // Return the view with validation errors
            return Ok("Unsuucessful");
        }

        public async Task<IActionResult> GetProfilePhoto(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.ProfilePhoto != null)
            {
                return File(user.ProfilePhoto, "image/jpeg");
            }

            // Return a default profile photo if none is set
            return File("~/images/download.png", "image/jpeg");
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<string>()); // Return an empty list if no query
            }

            var suggestions = _medicine.GetMedicinesByPartialName(query); // Call the repository to fetch suggestions
            return Json(suggestions.Take(4)); // Limit the results to 4
        }

        [HttpGet]
        public IActionResult SearchMedicines()
        {
            return View();
        }
        public IActionResult DisplayMedicines(int categoryId)
        {
            var model = _medicine.getMedicines(categoryId);
            ViewBag.CategoryId = categoryId;
            return View(model);
        }
        [HttpGet]
        public IActionResult MedicineInfo(int id)
        {
            // Fetch medicine details by id
            var medicine = _medicine.GetMedicineById(id);

            // If medicine is not found, return a "Not Found" page or handle appropriately
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }
        [HttpGet]
        public IActionResult MedicineInfoByName(string medicineName)
        {
            if (string.IsNullOrWhiteSpace(medicineName))
            {
                return NotFound("Invalid medicine name.");
            }

            // Decode friendly URL (replace hyphens back to spaces)
            var decodedName = medicineName.Replace('-', ' ');

            // Fetch medicine details
            var medicine = _medicine.GetMedicineByName(decodedName).FirstOrDefault();

            if (medicine == null)
            {
                return NotFound("Medicine not found.");
            }

            return View("MedicineInfo",medicine);
        }

        [HttpGet]
        public IActionResult SearchbyProduct(string medicineName,int categoryId)
        {
            var medicines = _medicine.GetMedicineByName(medicineName,categoryId);
            ViewBag.CategoryId = categoryId;
            return View("DisplayMedicines",medicines);
        }

        [HttpGet("GetImage/{id}")]
        public IActionResult GetImage(int id)
        {
            var medicine = _medicine.GetMedicineById(id);

            if (medicine == null || medicine.Image == null)
            {
                return NotFound(); // Return 404 if the medicine or image is not found
            }

            return File(medicine.Image, "image/jpeg"); // Return the image as a file
        }
    }
}
