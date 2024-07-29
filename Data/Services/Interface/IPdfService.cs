using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Services.Interface
{
    public interface IPdfService
    {
        public void GenerateAttendenceSheet(Stream outPutStream, IEnumerable<MembersViewModel> data, string filter);
        public void GenerateMembersReport(Stream outPutStream, IEnumerable<MembersPaymentReportViewModel> data, AttendenceReportViewModel model);
        public void GenerateAdminsReport(Stream outPutStream, IEnumerable<AdminsPaymentViewModel> data, AttendenceReportViewModel model);
        public void GenerateDirectorsReport(Stream outPutStream, IEnumerable<DirectorsPaymentReportViewModel> datas, AttendenceReportViewModel model);
    }
}
