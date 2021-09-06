using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class SendViaSMS
    {
        // SENT VIA FAST2SMS API
        public string SendviaSMS(string mobilenumber, string message)
        {
            string str_result = string.Empty;
            var client = new RestClient("https://www.fast2sms.com/dev/bulk");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "xTMJvp49nVYSs3lg7ZfEhuC6I1jDNqKk5FPOAbewXz2H0RBydc9W4ifIHUmQXD8EdVLCJA265OKxp7e3");
            request.AddParameter("sender_id", "FSTSMS");
            request.AddParameter("message", message);
            request.AddParameter("language", "english");
            request.AddParameter("route", "p");
            request.AddParameter("numbers", mobilenumber);
            IRestResponse response = client.Execute(request);
            // var statuscode = response.StatusCode;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                str_result = "success";
            }
            else
            {
                str_result = "failure";
            }
            return str_result;
        }
    }
}
