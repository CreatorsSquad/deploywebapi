using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class Logs
    {
        public NGCOREJWT_Log InsertLogs(string username, string usertype, string emailid, DateTime currentdate)
        {
            NGCOREJWT_Log log = new NGCOREJWT_Log();
            log.UserName = username;
            log.UserType = usertype;
            log.UserEmailID = emailid;
            log.LoginDate = currentdate;
            return log;
        }
    }
}
