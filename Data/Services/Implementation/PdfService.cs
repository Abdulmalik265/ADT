using Core.Models;
using Data.Services.Interface;
using iText.IO.Font;
using iText.IO.Image;
using iText.IO.Source;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Data.Services.Implementation
{
    public class PdfService : IPdfService
    {
        private readonly PdfFont _rubikMedium;
        private readonly PdfFont _rubikRegular;
        private readonly PdfFont _rubikBold;
        private readonly Color _darkGreenColor;
        private readonly Color _darkRedColor;
        public PdfService()
        {
            var rubikRegular = FontProgramFactory.CreateFont("Fonts/RubikRegular.ttf");
            var rubikMedium = FontProgramFactory.CreateFont("Fonts/RubikMedium.ttf");
            var rubikBold = FontProgramFactory.CreateFont("Fonts/RubikBold.ttf");
            _rubikRegular = PdfFontFactory.CreateFont(rubikRegular);
            _rubikMedium = PdfFontFactory.CreateFont(rubikMedium);
            _rubikBold = PdfFontFactory.CreateFont(rubikBold);
            _darkGreenColor = new DeviceRgb(0, 100, 0);
            _darkRedColor = new DeviceRgb(100, 0, 0);
        }

        public void GenerateAdminsReport(Stream outPutStream, IEnumerable<AdminsPaymentViewModel> data, AttendenceReportViewModel model)
        {
            using var pdfWriter = new PdfWriter(outPutStream);
            using var pdf = new PdfDocument(pdfWriter);
            using var document = new Document(pdf, PageSize.A4);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new IndigeneReportNumberEventHandler());

            document.SetFont(_rubikRegular);
            document.SetFontSize(9);
            document.SetMargins(30, 30, 30, 30);

            var image = ImageDataFactory.Create("wwwroot/home/images/logo_2.jpg");
            var logo = new Image(image).ScaleToFit(90, 80)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var headerTable = new Table(new UnitValue[3]
            {
        new UnitValue(UnitValue.PERCENT, 15),
        new UnitValue(UnitValue.PERCENT, 70),
        new UnitValue(UnitValue.PERCENT, 15)
            }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));
            var titleCell = new Cell();

            titleCell.Add(new Paragraph("JAMA'ATUT TAJDIDUL ISLAMY").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetFontColor(_darkGreenColor));
            titleCell.Add(new Paragraph("MOVEMENT FOR ISLAMIC REVIVAL").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetMultipliedLeading(0.7f).SetFontSize(15));
            headerTable.AddCell(titleCell.SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            document.Add(new Paragraph("PAYMENT REPORT").SetFont(_rubikBold)
                .SetFontSize(16).SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.RED).SetMultipliedLeading(0.7f));

            document.Add(new Paragraph($"STATE PAYMENT REPORT").SetFont(_rubikBold)
                .SetFontSize(15).SetTextAlignment(TextAlignment.CENTER).SetMultipliedLeading(0.7f));
            document.Add(new Paragraph($"Date: {DateTime.Now.ToShortDateString()}")
                .SetTextAlignment(TextAlignment.RIGHT).SetFont(_rubikRegular).SetFontSize(12));

            document.Add(new Paragraph($"Year: {model.Year}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));
            document.Add(new Paragraph($"Month: {model.Month}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));

            float[] columnWidths = { 2, 2, 4, 4, 2 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

            Cell cell1 = new Cell().Add(new Paragraph("S/N").SetFont(_rubikBold));
            Cell cell2 = new Cell().Add(new Paragraph("LGA").SetFont(_rubikBold));
            Cell cell3 = new Cell().Add(new Paragraph("ADMIN(S)").SetFont(_rubikBold));
            Cell cell4 = new Cell().Add(new Paragraph("PHONE NUMBER").SetFont(_rubikBold));
            Cell cell5 = new Cell().Add(new Paragraph("AMOUNT").SetFont(_rubikBold));

            table.AddHeaderCell(cell1);
            table.AddHeaderCell(cell2);
            table.AddHeaderCell(cell3);
            table.AddHeaderCell(cell4);
            table.AddHeaderCell(cell5);

            decimal sum = 0.0m;

            if (data != null)
            {
                int i = 0;
                foreach (var adminPayment in data)
                {
                    Cell snCell = new Cell().Add(new Paragraph($"{i + 1}"));
                    Cell propertyIdCell = new Cell().Add(new Paragraph($"{adminPayment.LgaName}"));

                    table.AddCell(snCell);
                    table.AddCell(propertyIdCell);

                    var adminNames = string.Join(", ", adminPayment.Admins.Select(a => a.AdminName));
                    var adminPhones = string.Join(", ", adminPayment.Admins.Select(a => a.AdminPhoneNumber));
                    var totalAmounts = adminPayment.Admins.Sum(a => a.TotalAmount);

                    Cell propertyNameCell = new Cell().Add(new Paragraph(adminNames));
                    Cell landLordCell = new Cell().Add(new Paragraph(adminPhones));
                    Cell totalRateCell = new Cell().Add(new Paragraph($"{totalAmounts}"));

                    table.AddCell(propertyNameCell);
                    table.AddCell(landLordCell);
                    table.AddCell(totalRateCell);

                    sum += totalAmounts;
                    i++;
                }
            }

            Cell emptyCell1 = new Cell();
            Cell emptyCell2 = new Cell();
            Cell emptyCell3 = new Cell();
            Cell labelCell = new Cell().Add(new Paragraph("TOTAL")).SetFont(_rubikBold);
            Cell totalCell = new Cell().Add(new Paragraph($"NGN{sum}")).SetFont(_rubikBold);

            table.AddCell(emptyCell1);
            table.AddCell(labelCell);
            table.AddCell(emptyCell2);
            table.AddCell(emptyCell3);
            table.AddCell(totalCell);

            document.Add(table);
        }

        public void GenerateAttendenceSheet(Stream outputStream, IEnumerable<MembersViewModel> data, string filter)
        {
            using var pdfWriter = new PdfWriter(outputStream);
            using var pdf = new PdfDocument(pdfWriter);
            using var document = new Document(pdf, PageSize.A4);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new IndigeneReportNumberEventHandler());

            document.SetFont(_rubikRegular);
            document.SetFontSize(9);
            document.SetMargins(30, 30, 30, 30);

            var image = ImageDataFactory.Create("wwwroot/home/images/logo_2.jpg");
            var logo = new Image(image).ScaleToFit(90, 80)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var headerTable = new Table(new UnitValue[3]
            {
                new (UnitValue.PERCENT, 15),
                new UnitValue(UnitValue.PERCENT, 70),
                new UnitValue(UnitValue.PERCENT, 15)
            }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));
            var titleCell = new Cell();

            titleCell.Add(new Paragraph("JAMA'ATUT TAJDIDUL ISLAMY").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetFontColor(_darkGreenColor));
            titleCell.Add(new Paragraph("MOVEMENT FOR ISLAMIC REVIVAL").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetMultipliedLeading(0.7f).SetFontSize(15));
            titleCell.Add(new Paragraph($"ATTENDANCE SHEET\n FILTER: ({filter})\n\n\n").SetFont(_rubikBold).SetFontSize(11)
            .SetFontColor(_darkRedColor).SetMultipliedLeading(0.8f)
            .SetMargin(0).SetPadding(0).SetUnderline().SetTextAlignment(TextAlignment.CENTER));

            headerTable.AddCell(titleCell.SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            float[] columnWidth = { 3, 2, 3, 2, 5 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidth)).UseAllAvailableWidth();

            Cell cell1 = new Cell().Add(new Paragraph("Full Name").SetFont(_rubikMedium));
            Cell cell2 = new Cell().Add(new Paragraph("Phone Number").SetFont(_rubikMedium));
            Cell cell3 = new Cell().Add(new Paragraph("Local Government").SetFont(_rubikMedium));
            Cell cell4 = new Cell().Add(new Paragraph("State").SetFont(_rubikMedium));
            Cell cell5 = new Cell().Add(new Paragraph("Sign").SetFont(_rubikMedium));

            table.AddHeaderCell(cell1);
            table.AddHeaderCell(cell2);
            table.AddHeaderCell(cell3);
            table.AddHeaderCell(cell4);
            table.AddHeaderCell(cell5);


            if (data is not null)
            {
                int i = 0;
                foreach (var member in data)
                {

                    Cell FullNameCell = new Cell().Add(new Paragraph($"{member.FullName}"));
                    Cell PhoneNumberCell = new Cell().Add(new Paragraph($"{member.PhoneNumber}"));
                    Cell LgCell = new Cell().Add(new Paragraph($"{member.LocalGovernment.Name}"));
                    Cell StateCell = new Cell().Add(new Paragraph($"{member.LocalGovernment.State.Name}"));
                    Cell Sign = new Cell().Add(new Paragraph(""));



                    table.AddCell(FullNameCell);
                    table.AddCell(PhoneNumberCell);
                    table.AddCell(LgCell);
                    table.AddCell(StateCell);
                    table.AddCell(Sign);

                    i++;
                }
            }

            document.Add(table);
            document.Close();
            document.Flush();
        }

        public void GenerateDirectorsReport(Stream outPutStream, IEnumerable<DirectorsPaymentReportViewModel> datas, AttendenceReportViewModel model)
        {
            using var pdfWriter = new PdfWriter(outPutStream);
            using var pdf = new PdfDocument(pdfWriter);
            using var document = new Document(pdf, PageSize.A4);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new IndigeneReportNumberEventHandler());

            document.SetFont(_rubikRegular);
            document.SetFontSize(9);
            document.SetMargins(30, 30, 30, 30);

            var image = ImageDataFactory.Create("wwwroot/home/images/logo_2.jpg");
            var logo = new Image(image).ScaleToFit(90, 80)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var headerTable = new Table(new UnitValue[3]
            {
        new UnitValue(UnitValue.PERCENT, 15),
        new UnitValue(UnitValue.PERCENT, 70),
        new UnitValue(UnitValue.PERCENT, 15)
            }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));
            var titleCell = new Cell();

            titleCell.Add(new Paragraph("JAMA'ATUT TAJDIDUL ISLAMY").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetFontColor(_darkGreenColor));
            titleCell.Add(new Paragraph("MOVEMENT FOR ISLAMIC REVIVAL").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetMultipliedLeading(0.7f).SetFontSize(15));
            headerTable.AddCell(titleCell.SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            document.Add(new Paragraph("PAYMENT REPORT").SetFont(_rubikBold)
                .SetFontSize(16).SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.RED).SetMultipliedLeading(0.7f));

            document.Add(new Paragraph($"STATE PAYMENT REPORT").SetFont(_rubikBold)
                .SetFontSize(15).SetTextAlignment(TextAlignment.CENTER).SetMultipliedLeading(0.7f));
            document.Add(new Paragraph($"Date: {DateTime.Now.ToShortDateString()}")
                .SetTextAlignment(TextAlignment.RIGHT).SetFont(_rubikRegular).SetFontSize(12));

            document.Add(new Paragraph($"Year: {model.Year}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));
            document.Add(new Paragraph($"Month: {model.Month}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));

            float[] columnWidths = { 2, 2, 4, 4, 2 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

            Cell cell1 = new Cell().Add(new Paragraph("S/N").SetFont(_rubikBold));
            Cell cell2 = new Cell().Add(new Paragraph("STATE").SetFont(_rubikBold));
            Cell cell3 = new Cell().Add(new Paragraph("DIRECTOR(S)").SetFont(_rubikBold));
            Cell cell4 = new Cell().Add(new Paragraph("PHONE NUMBER").SetFont(_rubikBold));
            Cell cell5 = new Cell().Add(new Paragraph("AMOUNT").SetFont(_rubikBold));

            table.AddHeaderCell(cell1);
            table.AddHeaderCell(cell2);
            table.AddHeaderCell(cell3);
            table.AddHeaderCell(cell4);
            table.AddHeaderCell(cell5);

            decimal sum = 0.0m;

            if (datas != null)
            {
                int i = 0;
                foreach (var tenementRate in datas)
                {
                    Cell snCell = new Cell().Add(new Paragraph($"{i + 1}"));
                    Cell propertyIdCell = new Cell().Add(new Paragraph($"{tenementRate.StateName}"));

                    table.AddCell(snCell);
                    table.AddCell(propertyIdCell);

                    var directorNames = string.Join(", ", tenementRate.Directors.Select(d => d.DirectorName));
                    var directorPhones = string.Join(", ", tenementRate.Directors.Select(d => d.DirectorPhoneNumber));
                    var totalAmounts = tenementRate.Directors.Sum(d => d.TotalAmount);

                    Cell propertyNameCell = new Cell().Add(new Paragraph(directorNames));
                    Cell landLordCell = new Cell().Add(new Paragraph(directorPhones));
                    Cell totalRateCell = new Cell().Add(new Paragraph($"{totalAmounts}"));

                    table.AddCell(propertyNameCell);
                    table.AddCell(landLordCell);
                    table.AddCell(totalRateCell);

                    sum += totalAmounts;
                    i++;
                }
            }

            Cell emptyCell1 = new Cell();
            Cell emptyCell2 = new Cell();
            Cell emptyCell3 = new Cell();
            Cell labelCell = new Cell().Add(new Paragraph("TOTAL")).SetFont(_rubikBold);
            Cell totalCell = new Cell().Add(new Paragraph($"NGN{sum}")).SetFont(_rubikBold);

            table.AddCell(emptyCell1);
            table.AddCell(labelCell);
            table.AddCell(emptyCell2);
            table.AddCell(emptyCell3);
            table.AddCell(totalCell);

            document.Add(table);
        }

        public void GenerateMembersReport(Stream outPutStream, IEnumerable<MembersPaymentReportViewModel> data, AttendenceReportViewModel model)
        {
            using var pdfWriter = new PdfWriter(outPutStream);
            using var pdf = new PdfDocument(pdfWriter);
            using var document = new Document(pdf, PageSize.A4);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new IndigeneReportNumberEventHandler());

            document.SetFont(_rubikRegular);
            document.SetFontSize(9);
            document.SetMargins(30, 30, 30, 30);

            var image = ImageDataFactory.Create("wwwroot/home/images/logo_2.jpg");
            var logo = new Image(image).ScaleToFit(90, 80)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var headerTable = new Table(new UnitValue[3]
            {
                new (UnitValue.PERCENT, 15),
                new UnitValue(UnitValue.PERCENT, 70),
                new UnitValue(UnitValue.PERCENT, 15)
            }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

            headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));
            var titleCell = new Cell();

            titleCell.Add(new Paragraph("JAMA'ATUT TAJDIDUL ISLAMY").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetFontColor(_darkGreenColor));
            titleCell.Add(new Paragraph("MOVEMENT FOR ISLAMIC REVIVAL").SetFont(_rubikBold)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBorder(Border.NO_BORDER)
                .SetMultipliedLeading(0.7f).SetFontSize(15));
            headerTable.AddCell(titleCell.SetBorder(Border.NO_BORDER));
            document.Add(headerTable);

            document.Add(new Paragraph("PAYMENT REPORT").SetFont(_rubikBold)
           .SetFontSize(16).SetTextAlignment(TextAlignment.CENTER)
           .SetFontColor(ColorConstants.RED).SetMultipliedLeading(0.7f));

            document.Add(new Paragraph($"{data.First().LgName}").SetFont(_rubikBold)
               .SetFontSize(15).SetTextAlignment(TextAlignment.CENTER).SetMultipliedLeading(0.7f));
            document.Add(new Paragraph($"Date: {DateTime.Now.ToShortDateString()}")
           .SetTextAlignment(TextAlignment.RIGHT).SetFont(_rubikRegular).SetFontSize(12));

            document.Add(new Paragraph($"Year: {model.Year}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));
            document.Add(new Paragraph($"Month: {data.First().Month}").SetFontSize(15).SetFont(_rubikMedium)
                .SetMultipliedLeading(0.8f));
            var tableFooter = new Table(new UnitValue[4]
       {
            UnitValue.CreatePercentValue(30),
            UnitValue.CreatePercentValue(20),
            UnitValue.CreatePercentValue(25),
            UnitValue.CreatePercentValue(25)
       }).UseAllAvailableWidth();


            float[] columnWidths = { 2, 2, 4, 4, 2 };
            Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

            Cell cell1 = new Cell().Add(new Paragraph("S/N").SetFont(_rubikBold));
            Cell cell2 = new Cell().Add(new Paragraph("FULL NAME").SetFont(_rubikBold));
            Cell cell3 = new Cell().Add(new Paragraph("PHONE NO.").SetFont(_rubikBold));
            Cell cell4 = new Cell().Add(new Paragraph("LGA").SetFont(_rubikBold));
            Cell cell5 = new Cell().Add(new Paragraph("AMOUNT").SetFont(_rubikBold));

            table.AddHeaderCell(cell1);
            table.AddHeaderCell(cell2);
            table.AddHeaderCell(cell3);
            table.AddHeaderCell(cell4);
            table.AddHeaderCell(cell5);

            decimal sum = 0.0m;

            if (data is not null)
            {
                int i = 0;
                foreach (var tenementRate in data)
                {
                    Cell snCell = new Cell().Add(new Paragraph($"{i + 1}"));
                    Cell propertyIdCell = new Cell().Add(new Paragraph($"{tenementRate.FullName}"));
                    Cell propertyNameCell = new Cell().Add(new Paragraph($"{tenementRate.PhoneNumber}"));
                    Cell landLordCell = new Cell().Add(new Paragraph($"{tenementRate.LgName}"));
                    Cell totalRateCell = new Cell().Add(new Paragraph($"{tenementRate.Amount}"));

                    table.AddCell(snCell);
                    table.AddCell(propertyIdCell);
                    table.AddCell(propertyNameCell);
                    table.AddCell(landLordCell);
                    table.AddCell(totalRateCell);
                    sum = sum + tenementRate.Amount;
                    i++;
                }
            }

            Cell emptyCell1 = new Cell();
            Cell emptyCell2 = new Cell();
            Cell emptyCell3 = new Cell();
            Cell labelCell = new Cell().Add(new Paragraph("TOTAL")).SetFont(_rubikBold);
            Cell totalCell = new Cell().Add(new Paragraph($"NGN{sum}")).SetFont(_rubikBold);

            table.AddCell(emptyCell1);
            table.AddCell(labelCell);
            table.AddCell(emptyCell2);
            table.AddCell(emptyCell3);
            table.AddCell(totalCell);

            document.Add(table);
        }

    }

    public class IndigeneReportNumberEventHandler : IEventHandler
    {
        private readonly PdfFont _rubikregular;

        public IndigeneReportNumberEventHandler()
        {
            var tahomaFontProgram = FontProgramFactory.CreateFont("Fonts/RubikRegular.ttf");
            _rubikregular = PdfFontFactory.CreateFont(tahomaFontProgram);
        }

        public void HandleEvent(Event @event)
        {
            var docEvent = (PdfDocumentEvent)@event;
            var pdf = docEvent.GetDocument();
            var pageNumber = pdf.GetPageNumber(docEvent.GetPage());
            var totalPages = pdf.GetNumberOfPages();

            var pageSize = docEvent.GetPage().GetPageSize();
            var pdfCanvas = new PdfCanvas(docEvent.GetPage());
            var canvas = new Canvas(pdfCanvas, pageSize);

            canvas.ShowTextAligned(
                new Paragraph($"Page {pageNumber} of {totalPages}")
                    .SetFontSize(8)
                    .SetFont(_rubikregular),
                pageSize.GetRight() - 50,
                pageSize.GetBottom() + 50,
                TextAlignment.RIGHT
            );
        }
    }


}



































