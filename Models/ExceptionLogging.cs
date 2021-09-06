using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class ExceptionLogging
    {
        public NGCOREJWT_Exception_ErrorLog SendExcepToDB(Exception exception, string foldername)
        {
            NGCOREJWT_Exception_ErrorLog errorlog = new NGCOREJWT_Exception_ErrorLog();
            errorlog.ExceptionMessage = exception.Message.ToString();
            errorlog.ExceptionType = exception.GetType().Name.ToString();
            errorlog.ExceptionURL = foldername;
            errorlog.ExceptionSource = exception.StackTrace.ToString();
            errorlog.CreatedDate = DateTime.Now;

            return errorlog;
        }
    }
}
