namespace HealthConnect.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prescription/Create
        public IActionResult Create(int appointmentId, string patientName)
        {
            var model = new PrescriptionViewModel
            {
                AppointmentId = appointmentId,
                PatientName = patientName,
                MedicineList = new List<PrescribedMedicineDetail> { new PrescribedMedicineDetail() } // Initialize with one empty item
            };

            return View(model);
        }

        // POST: Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            if (model.MedicineList == null)
            {
                model.MedicineList = new List<PrescribedMedicineDetail>(); // Initialize if null
            }

            if (ModelState.IsValid)
            {
                // Generate PDF from the provided model using jsreport
                byte[] pdfBytes = await GeneratePrescriptionPdf(model);

                // Check if a prescription with the same AppointmentId already exists
                var existingPrescription = await _context.Prescriptions
                                                         .FirstOrDefaultAsync(p => p.AppointmentId == model.AppointmentId);

                if (existingPrescription != null)
                {
                    // If prescription exists, update the existing prescription
                    existingPrescription.PrescriptionPdf = pdfBytes;

                    // Optionally, update other prescription details (if needed)
                    // e.g., existingPrescription.SomeOtherProperty = model.SomeOtherProperty;

                    _context.Update(existingPrescription);
                }
                else
                {
                    // If prescription does not exist, create a new one
                    var prescription = new Prescription
                    {
                        AppointmentId = model.AppointmentId,
                        PrescriptionPdf = pdfBytes
                    };

                    _context.Prescriptions.Add(prescription);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return RedirectToAction("TodaysAppointments", "Doctor");
            }

            return View(model);
        }


        public async Task<IActionResult> DownloadList()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Appointment)
                .ToListAsync();

            return View(prescriptions); // Ensure the view name matches or use View("DownloadPrescription", prescriptions)
        }


        // GET: Prescription/Download/{id}
        public IActionResult DownloadPrescription(int appointmentId)
        {
            // Fetch the prescription from the database using the appointmentId
            var prescription = _context.Prescriptions
                .FirstOrDefault(p => p.AppointmentId == appointmentId);

            if (prescription == null)
            {
                return Json(new { success = false, message = "No prescription has been added yet." });
            }

            // Return the PDF file for download
            return File(prescription.PrescriptionPdf, "application/pdf", $"Prescription_{appointmentId}.pdf");
        }

    

    private async Task<byte[]> GeneratePrescriptionPdf(PrescriptionViewModel model)
        {
            // Create an instance of LocalReporting
            var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

            try
            {
                // Create a template with dynamic content
                var template = new Template
                {
                    Content = @"
                <html>
<head>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 20px;
      color: #333;
    }

    h1 {
      font-size: 28px;
      font-weight: bold;
      text-align: center;
      color: #004085;
      margin-bottom: 10px;
    }

    p {
      font-size: 16px;
      margin: 5px 0;
    }

    .header {
      border-bottom: 2px solid #004085;
      margin-bottom: 20px;
      padding-bottom: 10px;
    }

    .info {
      margin-bottom: 20px;
    }

    .info p {
      font-size: 16px;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    table, th, td {
      border: 1px solid #ddd;
    }

    th, td {
      text-align: left;
      padding: 10px;
      font-size: 14px;
    }

    th {
      background-color: #f8f9fa;
      color: #004085;
    }

    .footer {
      margin-top: 30px;
      text-align: center;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>
<body>
  <div class=""header"">
    <h1>Prescription</h1>
    <p><strong>Appointment ID:</strong> {{AppointmentId}}</p>
  </div>

  <div class=""info"">
    <p><strong>Patient Name:</strong> {{PatientName}}</p>
  </div>

  <table>
    <thead>
      <tr>
        <th>Medicine Name</th>
        <th>Timings</th>
        <th>Food Instructions</th>
        <th>Additional Advice</th>
      </tr>
    </thead>
    <tbody>
      {{#each MedicineList}}
      <tr>
        <td>{{Name}}</td>
        <td>{{Timings}}</td>
        <td>{{FoodInstruction}}</td>
        <td>{{AdditionalAdvice}}</td>
      </tr>
      {{/each}}
    </tbody>
  </table>

  <div class=""footer"">
    <p>Generated by Health Connect</p>
    <p>Please follow the prescribed medications carefully. Contact your doctor if you have any concerns.</p>
  </div>
</body>
</html>
",
                    Recipe = Recipe.ChromePdf, // Use enum instead of string
                    Engine = Engine.Handlebars, // Use enum instead of string
                };

                var renderRequest = new RenderRequest
                {
                    Template = template,
                    Data = model // Pass the data here
                };

                var result = await rs.RenderAsync(renderRequest);

                using (var memoryStream = new MemoryStream())
                {
                    await result.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
                throw new Exception("Error generating PDF", ex);
            }
        }

    }
}
