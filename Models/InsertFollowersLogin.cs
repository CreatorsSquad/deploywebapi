using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class InsertFollowersLogin
    {
        public NGCOREJWT_FollowersLogin SaveFollowersLogin(string FollowersName, string EmailID, string MobileNumber, int UserLoginFKID)
        {
            NGCOREJWT_FollowersLogin followerlogin = new NGCOREJWT_FollowersLogin();
            followerlogin.FollowersName = FollowersName;
            followerlogin.EmailID = EmailID;
            followerlogin.MobileNumber = MobileNumber;
            followerlogin.UserLoginFKID = UserLoginFKID;
            followerlogin.OTP = 0;
            //flogin.IsCheckedOTP = false;
            followerlogin.IsActive = true;
            followerlogin.IsDeleted = false;
            followerlogin.IsRegistered = true;
            followerlogin.IsSubscribed = false;
            followerlogin.IsApproved = true;
            followerlogin.CreatedBy = "System";
            followerlogin.UpdatedBy = "System";
            followerlogin.CreatedDate = DateTime.Now;
            followerlogin.UpdatedDate = DateTime.Now;
            followerlogin.IsCheckedOTP = false;
            followerlogin.UserType = "Follower";

            return followerlogin;
        }
    }
}
