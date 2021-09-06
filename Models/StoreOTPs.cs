using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class StoreOTPs
    {
        public StoreOTP InsertOTP(int otp, string usertype, int celebrityfkid, int followerfkid, DateTime currentdate)
        {
            StoreOTP storeOTP = new StoreOTP();
            storeOTP.OneTimePassword = otp;
            storeOTP.UserType = usertype;
            storeOTP.CelebrityFKID = celebrityfkid;
            storeOTP.FollowerFKID = followerfkid;
            storeOTP.CreatedDate = currentdate;
            return storeOTP;
        }
    }
}
