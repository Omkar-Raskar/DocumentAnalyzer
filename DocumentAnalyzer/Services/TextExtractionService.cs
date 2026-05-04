using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using DocumentFormat.OpenXml.Packaging;

public class TextExtractionService
{
    public string ExtractTextFromPdf(Stream fileStream)
    {
        StringBuilder text = new StringBuilder();

        using (PdfReader reader = new PdfReader(fileStream))
        using (PdfDocument pdf = new PdfDocument(reader))
        {
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));
            }
        }

        return text.ToString();
    }

    public string ExtractTextFromDocx(Stream fileStream)
    {
        using (WordprocessingDocument doc = WordprocessingDocument.Open(fileStream, false))
        {
            return doc.MainDocumentPart.Document.Body.InnerText;
        }
    }
}