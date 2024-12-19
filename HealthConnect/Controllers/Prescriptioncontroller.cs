using iTextSharp.text;
using iTextSharp.text.pdf;

public class PrescriptionController : Controller
{
    private readonly ApplicationDbContext _context;

    public PrescriptionController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult SubmitPrescription()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> SubmitPrescription(int appointmentId, PrescriptionViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Generate the PDF
            var pdfBytes = GeneratePrescriptionPdf(model);

            // Create a new Prescription entry
            var prescription = new Prescription
            {
                AppointmentId = appointmentId,
                PrescriptionPdf = pdfBytes
            };

            // Save to the database
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success"); // Redirect after successful submission
        }

        return View(model);
    }


    private byte[] GeneratePrescriptionPdf(PrescriptionViewModel model)
    {
        using (var ms = new MemoryStream())
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, ms);
            document.Open();

            // Add content
            document.Add(new Paragraph($"Prescription for {model.PatientName}"));
            document.Add(new Paragraph($"Generated on: {DateTime.Now}"));
            document.Add(new Paragraph(" "));

            foreach (var medicine in model.MedicineList)
            {
                document.Add(new Paragraph($"Medicine: {medicine.Name}"));
                document.Add(new Paragraph($"Timings: {string.Join(", ", medicine.Timings)}"));
                document.Add(new Paragraph($"Food Instructions: {medicine.FoodInstruction}"));
                document.Add(new Paragraph($"Additional Advice: {medicine.AdditionalAdvice}"));
                document.Add(new Paragraph(" "));
            }

            document.Close();
            return ms.ToArray();
        }
    }
}
