using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CreatorsSquad.Helpers;
using CreatorsSquad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Razorpay.Api;

namespace CreatorsSquad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly NGCoreJWT_DbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signManager;
        private readonly AppSettings _appSettings;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly EmailConfiguration emailconf;
        int addfolowerscount = 0;
        // EmailController emailcont = new EmailController();
        ExceptionLogging exceptionlog = new ExceptionLogging();
        InsertFollowersLogin savefollowers = new InsertFollowersLogin();
        SendViaSMS viasms = new SendViaSMS();
        Logs savelogs = new Logs();
        StoreOTPs storeotp = new StoreOTPs();

        public AccountController(NGCoreJWT_DbContext context, EmailConfiguration _emailconf, IWebHostEnvironment hostingenvt,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IOptions<AppSettings> appSettings
            )
        {
            _context = context;
            _userManager = userManager;
            _signManager = signInManager;
            _appSettings = appSettings.Value;
            emailconf = _emailconf;
            _hostingEnvironment = hostingenvt;
        }

        // save the celebrity / artist details

        [Route("celebrityregister")]
        [HttpPost]
        public async Task<IActionResult> CelebrityRegistration([FromForm] InsertUserDetails formdata)
        {
            // Will hold all the errors related to registration
            List<string> errorList = new List<string>();
            try
            {
                var user = new IdentityUser
                {
                    Email = formdata.EmailID,
                    UserName = formdata.FullName,
                    //   PhoneNumber = formdata.MobileNumber,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user, "Squad$123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Celebrity");

                    var usercount = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == formdata.FullName && x.IsApproved == true
                    && x.IsActive == true && x.IsRegistered == true && x.EmailID == formdata.EmailID && x.PersonalityType == "Celebrity" && x.IsApproved == true).CountAsync();

                    //  var checkuser = (dynamic)null;

                    if (usercount == 1)
                    {
                        return BadRequest(new JsonResult("Celebrity Name / EmailID was already exists"));
                    }
                    else
                    {
                        NGCOREJWT_UserLogin userlogin = new NGCOREJWT_UserLogin();
                        userlogin.FullName = formdata.FullName.Trim('"');
                        userlogin.EmailID = formdata.EmailID.Trim('"');
                        userlogin.MobileNumber = formdata.MobileNumber.Trim('"');
                        userlogin.OTP = 0;
                        userlogin.IsActive = true;
                        userlogin.IsDeleted = false;
                        userlogin.IsRegistered = true;
                        userlogin.IsApproved = false;
                        //userlogin.CreatedBy = "System";
                        userlogin.UpdatedBy = "System";
                        userlogin.CreatedDate = DateTime.Now;
                        userlogin.UpdatedDate = DateTime.Now;
                        userlogin.IsCheckedOTP = false;
                        userlogin.PersonalityType = "Celebrity";

                        _context.NGCOREJWT_UserLogins.Add(userlogin);
                        await _context.SaveChangesAsync();
                        // checkuser = null;

                        await EmailNotification("", "", "New Celebrity named" + formdata.FullName + " has signed up the account", 5, formdata.FullName);
                    }


                    // Sending Confirmation Email

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                    //await _emailsender.SendEmailAsync(user.Email, "CreatorsSquad.com - Confirm Your Email", "Please confirm your e-mail by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                    return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successful" });
                }
                else
                {
                    return BadRequest(new JsonResult("Celebrity Name / EmailID was already exists"));
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Insert_Celebrity_LoginDetails");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return BadRequest(new JsonResult(errorList));
        }

        // save the follower / user details

        [Route("follower_register")]
        [HttpPost]
        public async Task<IActionResult> UserRegistration([FromForm] InsertFollowersInfo followerinfo)
        {
            // Will hold all the errors related to registration
            List<string> errorList = new List<string>();
            int chkcelebrity = 0;
            var chkfollower = (dynamic)null;
            try
            {
                if (followerinfo.CelebrityName == null || followerinfo.CelebrityName == "undefined")
                {
                    chkfollower = await _context.NGCOREJWT_FollowersLogins.Where(i => i.UserType == "Follower" && i.FollowersName == followerinfo.FollowersName
                    && i.IsApproved == true && i.EmailID == followerinfo.EmailID).FirstOrDefaultAsync();
                }
                else
                {
                    chkcelebrity = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == followerinfo.CelebrityName && i.IsActive == true
                    && i.IsApproved == true).CountAsync();
                }

                if (chkcelebrity == 1 || chkfollower == null)
                {
                    var follower = new IdentityUser
                    {
                        Email = followerinfo.EmailID,
                        UserName = followerinfo.FollowersName,
                        // PhoneNumber = followerinfo.MobileNumber,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(follower, "Squad$123");

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(follower, "Follower");

                        var usercount = await _context.NGCOREJWT_FollowersLogins.Where(x => x.IsActive == true && x.IsRegistered == true && x.IsApproved == true
                        && x.EmailID == followerinfo.EmailID && x.UserType == "Follower").CountAsync();

                        if (usercount >= 1)
                        {
                        }
                        else
                        {
                            var getuserid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == followerinfo.CelebrityName && i.IsActive == true &&
                            i.IsRegistered == true && i.IsApproved == true).FirstOrDefaultAsync();
                            // }
                            var followerlogin = savefollowers.SaveFollowersLogin(followerinfo.FollowersName.Trim('"'), followerinfo.EmailID.Trim('"'),
                                                followerinfo.MobileNumber.Trim('"'), getuserid == null ? 0 : getuserid.UserLoginPKID);

                            await _context.NGCOREJWT_FollowersLogins.AddAsync(followerlogin);

                            await _context.SaveChangesAsync();

                            var followerspkid = await _context.NGCOREJWT_FollowersLogins.Where(i => i.FollowersName == followerinfo.FollowersName.Trim('"')
                            && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true).FirstOrDefaultAsync();

                            var unlockedcontents = await _context.NGCOREJWT_UnlockedContents.Where(x => x.IsActive == true && x.CelebrityLoginFKID == followerspkid.UserLoginFKID).ToListAsync();

                            foreach (var saveulmfs in unlockedcontents)
                            {
                                NGCOREJWT_UnlockedContent_MF unlockedmfs = new NGCOREJWT_UnlockedContent_MF();
                                unlockedmfs.CelebrityLoginFKID = getuserid.UserLoginPKID;
                                unlockedmfs.FollowersLoginFKID = followerspkid.FollowersLoginPKID;
                                unlockedmfs.UnlockedContentFKID = saveulmfs.UnlockedContentPKID;
                                unlockedmfs.ContentPrice = saveulmfs.ContentPrice;
                                unlockedmfs.ContentPriceFKID = saveulmfs.ContentPriceFKID;
                                unlockedmfs.ContentType = saveulmfs.ContentType;
                                unlockedmfs.ContentCaption = saveulmfs.ContentCaption;
                                unlockedmfs.ContentDesc = saveulmfs.ContentDesc;
                                unlockedmfs.IconPath = saveulmfs.IconPath1;
                                unlockedmfs.ImagePath = saveulmfs.ImagePath1;
                                unlockedmfs.DupFileName = saveulmfs.FileName;
                                unlockedmfs.IsActive = true;
                                unlockedmfs.IsDeleted = false;
                                unlockedmfs.IsLocked = false;
                                unlockedmfs.CreatedBy = "System";
                                unlockedmfs.UpdatedBy = "System";
                                unlockedmfs.CreatedDate = saveulmfs.CreatedDate;
                                unlockedmfs.UpdatedDate = DateTime.Now;
                                unlockedmfs.GSTCharges = saveulmfs.UC_GSTCharges;
                                unlockedmfs.ServiceCharges = saveulmfs.UC_ServiceCharges;
                                unlockedmfs.TotalCharges = saveulmfs.UC_TotalCharges;

                                await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(unlockedmfs);
                                await _context.SaveChangesAsync();
                            }
                            await EmailNotification("Dear " + followerinfo.EmailID + ", a new follower signed up a account", "", "New Follower signed up named (" + followerinfo.FollowersName + ")", 4, followerinfo.CelebrityName);
                        }

                        // Sending Confirmation Email

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(follower);

                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { UserId = follower.Id, Code = code }, protocol: HttpContext.Request.Scheme);

                        //await _emailsender.SendEmailAsync(follower.Email, "CreatorsSquad.com - Confirm Your Email", "Please confirm your e-mail by clicking this link: <a href=\"" + callbackUrl + "\">click here</a>");

                        return Ok(new { username = follower.UserName, email = follower.Email, status = 1, message = "Registration Successful" });
                    }
                    else
                    {
                        return new JsonResult("EmailID was already exists!...");
                    }
                }
                else
                {
                    return new JsonResult("EmailID was already exists!...");
                }

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Insert_Celebrity_LoginDetails");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return BadRequest(new JsonResult(errorList));
        }

        // Login Method - CelebrityLogin
        [Route("celebritylogin")]
        [HttpPost]
        public async Task<IActionResult> CelebrityLogin([FromForm] ArtistViewModel loginView)
        {
            // Get the User from Database
            var user = await _userManager.FindByNameAsync(loginView.Username);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null)
            {
                var celebritycheck = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == user.UserName && i.IsApproved == true
                && i.PersonalityType == "Celebrity" && i.EmailID == user.Email && i.OTP == loginView.OTP && i.IsActive == true).FirstOrDefaultAsync();

                if (celebritycheck != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var logs = savelogs.InsertLogs(celebritycheck.FullName, celebritycheck.PersonalityType, celebritycheck.EmailID, DateTime.Now);
                    await _context.NGCOREJWT_Logs.AddAsync(logs);
                    await _context.SaveChangesAsync();
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim(JwtRegisteredClaimNames.Sub, loginView.Username.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                        new Claim("FollowersName",""),
                         }),

                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _appSettings.Site,
                        Audience = _appSettings.Audience,
                        Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                    };

                    // Generate Token

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return Ok(new { token = tokenHandler.WriteToken(token), expiration = token.ValidTo, username = user.UserName, userRole = roles.FirstOrDefault() });
                }
                else
                {
                    return BadRequest(new JsonResult("Valid Instagram Name"));
                }
            }
            else
            {
                return BadRequest(new JsonResult("Valid Instagram Name"));
            }

        }

        // Login Method - userLogin / followerslogin
        [Route("followerlogin")]
        [HttpPost]
        public async Task<IActionResult> FollowerLogin([FromBody] FollowerViewModel loginView)
        {
            // Get the User from Database
            var user = await _userManager.FindByEmailAsync(loginView.FollowersEmailID);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null)
            {
                var followercheck = await _context.NGCOREJWT_FollowersLogins.Where(i => i.FollowersName == user.UserName && i.IsApproved == true
                && i.UserType == "Follower" && i.EmailID == user.Email && i.OTP == loginView.OTP && i.IsActive == true).FirstOrDefaultAsync();

                var usercheck = await _context.NGCOREJWT_UserLogins.Where(i => i.UserLoginPKID == followercheck.UserLoginFKID && i.IsActive == true
                && i.IsApproved == true).FirstOrDefaultAsync();

                if (followercheck != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var logs = savelogs.InsertLogs(followercheck.FollowersName, followercheck.UserType, followercheck.EmailID, DateTime.Now);
                    await _context.NGCOREJWT_Logs.AddAsync(logs);
                    await _context.SaveChangesAsync();
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim(JwtRegisteredClaimNames.Sub, followercheck.FollowersName.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                        new Claim("FollowersName",""),
                         }),

                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _appSettings.Site,
                        Audience = _appSettings.Audience,
                        Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                    };

                    // Generate Token

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return Ok(new { token = tokenHandler.WriteToken(token), FollowerName = followercheck.FollowersName, expiration = token.ValidTo, username = usercheck.FullName, userRole = roles.FirstOrDefault() });
                }
                else
                {
                    return BadRequest(new JsonResult("Invalid OTP"));
                }
            }

            // return error
            ModelState.AddModelError("", "Username/Password was not Found");
            return Unauthorized(new { LoginError = "Please Check the Login Credentials - Invalid Username/Password was entered" });
        }

        // Validate the follower / user and generate the otp

        [Route("Follower_GenerateOTP")]
        [HttpPost]
        public async Task<JsonResult> GenerateOTP_Follower([FromForm] Follower_GenerateOTP generateOTP)
        {
            var user = await _userManager.FindByEmailAsync(generateOTP.FollowersEmailID);
            string strmobile = string.Empty;
            string stremailid = string.Empty;
            string result = string.Empty;
            if (user != null)
            {
                if (generateOTP.Username == "undefined")
                {
                    var chkcelebrity = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == generateOTP.Username && i.IsActive == true
                    && i.IsApproved == true && i.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                    var chkfollower = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == generateOTP.FollowersEmailID &&
                    i.IsActive == true && i.IsApproved == true && i.UserType == "Follower").FirstOrDefaultAsync();

                    if (chkfollower.UserLoginFKID == 0)
                    {
                        chkfollower.UserLoginFKID = chkcelebrity.UserLoginPKID;
                        await _context.SaveChangesAsync();
                    }
                }


                var chkuser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.FollowersName == user.UserName && i.IsApproved == true
                && i.UserType == "Follower" && i.EmailID == user.Email && i.IsActive == true && i.UserLoginFKID == _context.NGCOREJWT_UserLogins
                .Where(j => j.UserLoginPKID == i.UserLoginFKID && j.IsActive == true && j.IsApproved == true).Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                if (chkuser != null)
                {
                    strmobile = chkuser.MobileNumber;
                    stremailid = chkuser.EmailID;

                    Random randomvalue = new Random();
                    int generateotp = randomvalue.Next(1001, 9999);

                    chkuser.OTP = generateotp;
                    chkuser.UpdatedDate = DateTime.Now;
                    chkuser.IsCheckedOTP = false;

                    await _context.SaveChangesAsync();

                    await EmailNotification(generateOTP.FollowersEmailID, Convert.ToString(generateotp), "Creators Squad - Sending OTP", 7, generateOTP.Username);


                    string numbers = strmobile; // in a comma seperated list
                    string message = "Your OTP Number is " + generateotp + " ( Sent By : Creators Squad )";

                    result = viasms.SendviaSMS(numbers, message);
                    var otps = storeotp.InsertOTP(generateotp, chkuser.UserType, 0, chkuser.FollowersLoginPKID, DateTime.Now);
                    await _context.StoreOTPs.AddAsync(otps);
                    await _context.SaveChangesAsync();
                    result = "We have sent you a 4 digit SMS to your mobile";
                }
                else
                {
                    result = "Please register your account in Creators Squad!...";
                }
            }
            else
            {
                result = "Please register your account in Creators Squad!...";
            }
            return new JsonResult(result);
        }

        // Validate the Celebrity / Artist and generate the otp

        [Route("Celebrity_GenerateOTP/{CelebrityName}")]
        [HttpGet("{CelebrityName}")]
        public async Task<JsonResult> GenerateOTP_Celebrity(string CelebrityName)
        {
            var user = await _userManager.FindByNameAsync(CelebrityName);
            string strmobile = string.Empty;
            string stremailid = string.Empty;
            string result = string.Empty;
            if (user != null)
            {
                var chkuser = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == user.UserName && i.IsApproved == true
                && i.PersonalityType == "Celebrity" && i.EmailID == user.Email && i.IsActive == true).FirstOrDefaultAsync();

                if (chkuser != null)
                {
                    strmobile = chkuser.MobileNumber;
                    stremailid = chkuser.EmailID;

                    Random randomvalue = new Random();
                    int generateotp = randomvalue.Next(1001, 9999);

                    chkuser.OTP = generateotp;
                    chkuser.UpdatedDate = DateTime.Now;
                    chkuser.IsCheckedOTP = false;

                    await _context.SaveChangesAsync();

                    await EmailNotification(CelebrityName, Convert.ToString(generateotp), "Creators Squad - Sending OTP", 3, CelebrityName);


                    string numbers = strmobile; // in a comma seperated list
                    string message = "Your OTP Number is " + generateotp + " ( Sent By : Creators Squad )";

                    result = viasms.SendviaSMS(numbers, message);

                    var otps = storeotp.InsertOTP(generateotp, chkuser.PersonalityType, chkuser.UserLoginPKID, 0, DateTime.Now);
                    await _context.StoreOTPs.AddAsync(otps);
                    await _context.SaveChangesAsync();
                    result = "We have sent you a 4 digit SMS to your mobile";
                }
                else
                {
                    result = "Your account is in pending status!...";
                }
            }
            else
            {
                result = "Please register your account in Creators Squad!...";
            }
            return new JsonResult(result);
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new JsonResult("ERROR");
            }

            if (user.EmailConfirmed)
            {
                return Redirect("/login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return RedirectToAction("EmailConfirmed", "Notifications", new { userId, code });
            }
            else
            {
                List<string> errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.ToString());
                }
                return new JsonResult(errors);
            }
        }

        public async Task<ActionResult> EmailNotification(string FullName, string FileName, string subject, int followerscount, string celebrityname)
        {
            try
            {
                var credentials = new NetworkCredential("connectmuthu28@gmail.com", "ForverJoya$2019");
                string body;

                if (followerscount == 1 || followerscount == 0)
                {
                    body = FullName + ". Please check it out..";
                }
                else if (followerscount == 2)
                {
                    body = "The celebrity named " + FullName + "  has uploaded " + FileName;
                }
                else if (followerscount == 4)
                {
                    body = FullName + ". Please check it out..";
                }
                else if (followerscount == 5)
                {
                    body = subject;
                }
                else if (followerscount == 6)
                {
                    body = FullName;
                }
                else
                {
                    body = "Dear " + FullName + ", we have sent you a 4 digit OTP. Please check...." + "The OTP is " + FileName;
                }

                var mail = new MailMessage();

                var celebritycheck = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == Convert.ToString(followerscount == 2 ? FullName : celebrityname) && i.IsActive == true
                                     && i.IsRegistered == true && i.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                if (followerscount == 3 || followerscount == 0 || followerscount == 1 || followerscount == 4
                    || followerscount == 5 || followerscount == 6 || followerscount == 7)
                {
                    if (celebritycheck != null)
                    {
                        if (followerscount != 7)
                        {
                            var tempath = Path.Combine(
                                   _hostingEnvironment.ContentRootPath,
                                   "EmailTemplates", "emailtemplate.html");

                            StreamReader reader = new StreamReader(tempath);

                            string readFile = reader.ReadToEnd();

                            string myMessage = "";
                            myMessage = readFile;
                            myMessage = myMessage.Replace("$$message$$", body);

                            mail = new MailMessage()
                            {
                                From = new MailAddress(emailconf.From),
                                Subject = subject,
                                Body = myMessage.ToString()
                            };
                            var recepients = await _context.NGCOREJWT_PersonalEmailIDs.Where(i => i.IsActive == true).ToListAsync();

                            StringBuilder sbemail = new StringBuilder();
                            string appemailid = string.Empty;

                            foreach (var emailids in recepients)
                            {
                                mail.CC.Add(new MailAddress(emailids.PEmailID));
                                sbemail.Append(emailids.PEmailID);
                                sbemail.Append(",");
                            }

                            sbemail.Append(celebritycheck.EmailID);
                            appemailid = sbemail.ToString().TrimEnd(',');

                            mail.To.Add(new MailAddress(celebritycheck.EmailID));

                            mail.IsBodyHtml = true;

                            var smtpclient = new SmtpClient()
                            {
                                Port = emailconf.Port,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false,
                                Host = emailconf.SmtpServer,
                                EnableSsl = true,
                                Credentials = credentials
                            };

                            await smtpclient.SendMailAsync(mail);

                            NGCOREJWT_Email_Notification notification = new NGCOREJWT_Email_Notification();
                            notification.SenderEmail = mail.From.ToString();
                            notification.Recepients = appemailid;
                            notification.Subject = mail.Subject;
                            notification.MessageBody = body;
                            notification.CelebrityFKID = celebritycheck.UserLoginPKID;
                            notification.CreatedBy = "System";
                            notification.CreatedDate = DateTime.Now;

                            await _context.NGCOREJWT_Email_Notifications.AddAsync(notification);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            await Follower_EmailNotification(body, subject, celebritycheck.FullName, followerscount, FullName);
                        }
                    }
                    else
                    {
                        if (followerscount == 7)
                        {
                            await Follower_EmailNotification(body, subject, celebritycheck.FullName, followerscount, FullName);
                        }
                    }
                }
                else
                {
                    await Follower_EmailNotification(body, subject, celebritycheck.FullName, followerscount, FullName);
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "EmailNotification");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        public async Task<ActionResult> Follower_EmailNotification(string body, string subject, string celebrityname, int followerscount, string followersemailid)
        {
            try
            {
                var credentials = new NetworkCredential(emailconf.From, emailconf.Password);

                var followercheck = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.IsApproved == true
                                    && i.IsRegistered == true && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.FullName == celebrityname &&
                                    j.PersonalityType == "Celebrity" && j.IsActive == true).Select(s => s.UserLoginPKID).FirstOrDefault()).ToListAsync();

                var getfollower = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.IsApproved == true && i.EmailID == followersemailid
                                   && i.IsRegistered == true && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.FullName == celebrityname &&
                                   j.PersonalityType == "Celebrity" && j.IsActive == true).Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();
                var followers_mail = new MailMessage()
                {
                    From = new MailAddress(emailconf.From),
                    Subject = subject,
                    Body = followerscount == 7 ? body : "Dear Users, " + body
                };
                var recepients = await _context.NGCOREJWT_PersonalEmailIDs.Where(i => i.IsActive == true).ToListAsync();
                StringBuilder strbuild = new StringBuilder();
                string appemailids = string.Empty;

                foreach (var emailids in recepients)
                {
                    followers_mail.CC.Add(new MailAddress(emailids.PEmailID));
                    strbuild.Append(emailids.PEmailID);
                    strbuild.Append(",");
                }
                if (followerscount != 7)
                {
                    foreach (var femailid in followercheck)
                    {
                        followers_mail.To.Add(new MailAddress(femailid.EmailID));
                        strbuild.Append(femailid.EmailID);
                        strbuild.Append(",");
                    }
                }
                else
                {
                    followers_mail.To.Add(new MailAddress(getfollower.EmailID));
                    strbuild.Append(getfollower.EmailID);
                    strbuild.Append(",");
                }

                appemailids = strbuild.ToString().TrimEnd(',');
                followers_mail.IsBodyHtml = true;

                var smtpclient = new SmtpClient()
                {
                    Port = emailconf.Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = emailconf.SmtpServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                await smtpclient.SendMailAsync(followers_mail);

                NGCOREJWT_Email_Notification notification = new NGCOREJWT_Email_Notification();
                notification.SenderEmail = followers_mail.From.ToString();
                notification.Recepients = appemailids;
                notification.Subject = followers_mail.Subject;
                notification.MessageBody = body;
                notification.CreatedBy = "System";
                notification.CreatedDate = DateTime.Now;

                await _context.NGCOREJWT_Email_Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Follower_EmailNotification");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Content Controller methods


        [HttpGet("{id}")]
        public async Task<ActionResult<NGCOREJWT_UserLogin>> GetUserLogins(int id)
        {
            var awaituserlogin = await _context.NGCOREJWT_UserLogins.FindAsync(id);

            if (awaituserlogin == null)
            {
                return NotFound();
            }

            return awaituserlogin;
        }

        // validate the Celebrity details

        [Route("checkcelebdetails/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<JsonResult> CheckCelebrityDetails(string FullName)
        {
            var chkuser = await _context.NGCOREJWT_UserLogins.FirstOrDefaultAsync(x => x.FullName == FullName && x.IsActive == true
                            && x.PersonalityType == "Celebrity");

            string strmobile = string.Empty;
            string stremailid = string.Empty;
            string result = string.Empty;

            if (chkuser != null)
            {
                strmobile = chkuser.MobileNumber;
                stremailid = chkuser.EmailID;

                Random randomvalue = new Random();
                int generateotp = randomvalue.Next(1001, 9999);

                chkuser.OTP = generateotp;
                chkuser.UpdatedDate = DateTime.Now;
                chkuser.IsCheckedOTP = false;

                await _context.SaveChangesAsync();

                await EmailNotification(FullName, Convert.ToString(generateotp), "Creators Squad - Sending OTP", 3, "");


                string numbers = strmobile; // in a comma seperated list
                string message = "Your OTP Number is " + generateotp + " ( Sent By : Creators Squad )";

                result = viasms.SendviaSMS(numbers, message);
            }
            else
            {
                result = "Please register your account in Creators Squad!...";
            }
            return new JsonResult(result);
        }

        // validate the Follower details

        [Route("checkfollowerdet/{Followername}")]
        [HttpGet("{Followername}")]
        public async Task<JsonResult> CheckFollowerDetails(string Followername)
        {
            var chkfollower = await _context.NGCOREJWT_FollowersLogins.FirstOrDefaultAsync(x => x.FollowersName == Followername && x.IsActive == true);

            string strmobile = string.Empty;
            string stremailid = string.Empty;
            string result = string.Empty;

            if (chkfollower != null)
            {
                strmobile = chkfollower.MobileNumber;
                stremailid = chkfollower.EmailID;

                Random randomvalue = new Random();
                int generateotp = randomvalue.Next(1001, 9999);

                chkfollower.OTP = generateotp;
                chkfollower.UpdatedDate = DateTime.Now;
                chkfollower.IsCheckedOTP = false;

                await _context.SaveChangesAsync();

                await email.EmailNotification(Followername, Convert.ToString(generateotp), "Creators Squad - Sending OTP", 3,"");


                string numbers = strmobile; // in a comma seperated list
                string message = "Your OTP Number is " + generateotp + " ( Sent By : Creators Squad )";

                result = SendviaSMS(numbers, message);
                result = "success";
            }
            else
            {
                result = "Please register your account in Creators Squad!...";
            }
            return new JsonResult(result);
        }


        // Resend the OTP value to celebrity

        [Route("resendotpvalue/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<JsonResult> ResendOtpValue(string FullName)
        {
            var user = await _userManager.FindByNameAsync(FullName);
            string result = string.Empty;
            if (user != null)
            {
                var chkuser = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == FullName && x.IsActive == true
                && x.IsApproved == true && x.PersonalityType == "Celebrity" && x.EmailID == user.Email).FirstOrDefaultAsync();

                string strmobile = string.Empty;
                string stremailid = string.Empty;

                if (chkuser != null)
                {
                    strmobile = chkuser.MobileNumber;
                    stremailid = chkuser.EmailID;

                    Random randomvalue = new Random();
                    int generateotp = randomvalue.Next(1001, 9999);

                    chkuser.OTP = chkuser.OTP;
                    chkuser.UpdatedDate = DateTime.Now;
                    chkuser.IsCheckedOTP = false;

                    await _context.SaveChangesAsync();

                    await EmailNotification(FullName, Convert.ToString(chkuser.OTP), "Creators Squad - Sending OTP", 3, FullName);

                    string numbers = strmobile; // in a comma seperated list
                    string message = "Your OTP Number is " + chkuser.OTP + " ( Sent By : Creators Squad )";
                    result = viasms.SendviaSMS(numbers, message);
                    var otps = storeotp.InsertOTP(generateotp, chkuser.PersonalityType, chkuser.UserLoginPKID, 0, DateTime.Now);
                    await _context.StoreOTPs.AddAsync(otps);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    result = "Please register your account in Creators Squad!...";
                }
            }

            return new JsonResult(result);
        }

        // Resend the OTP value to followers

        [Route("follower_resendotp/{Followersemailid}")]
        [HttpGet("{Followersemailid}")]
        public async Task<JsonResult> Follower_ResendOTP(string Followersemailid)
        {
            var user = await _userManager.FindByEmailAsync(Followersemailid);
            string result = string.Empty;

            if (user != null)
            {
                var chkuser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.FollowersName == user.UserName && i.IsApproved == true
                && i.UserType == "Follower" && i.EmailID == user.Email && i.IsActive == true).FirstOrDefaultAsync();

                if (chkuser != null)
                {
                    string strmobile = string.Empty;
                    string stremailid = string.Empty;
                    strmobile = chkuser.MobileNumber;
                    stremailid = chkuser.EmailID;

                    Random randomvalue = new Random();
                    int generateotp = randomvalue.Next(1001, 9999);

                    chkuser.OTP = chkuser.OTP;
                    chkuser.UpdatedDate = DateTime.Now;
                    chkuser.IsCheckedOTP = false;

                    await _context.SaveChangesAsync();

                    await EmailNotification("", Convert.ToString(chkuser.OTP), "Creators Squad - Sending OTP", 3, "");

                    string numbers = strmobile; // in a comma seperated list
                    string message = "Your OTP Number is " + chkuser.OTP + " ( Sent By : Creators Squad )";
                    result = viasms.SendviaSMS(numbers, message);
                    result = "success";
                    var otps = storeotp.InsertOTP(generateotp, chkuser.UserType, 0, chkuser.FollowersLoginPKID, DateTime.Now);
                    await _context.StoreOTPs.AddAsync(otps);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    result = "Please register your account in Creators Squad!...";
                }
            }
            return new JsonResult(result);
        }



        // OTP Verification to celebrity

        [Route("updateotpverification/{OTP}")]
        [HttpPut("{OTP}")]
        public async Task<JsonResult> UpdateOtpVerification(int OTP)
        {
            string result = string.Empty;
            try
            {
                var otpcount = await _context.NGCOREJWT_UserLogins.Where(x => x.OTP == OTP && x.IsActive == true && x.IsCheckedOTP == false
                               && x.IsRegistered == true).CountAsync();

                if (otpcount == 1)
                {
                    var updateotp = await _context.NGCOREJWT_UserLogins.FirstOrDefaultAsync(x => x.OTP == OTP && x.IsActive == true && x.IsCheckedOTP == false);
                    updateotp.UpdatedDate = DateTime.Now;
                    updateotp.IsCheckedOTP = true;
                    result = "success";
                }
                else
                {
                    result = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Update_Celebrity_OTP_ToCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(result);
        }

        // OTP Verification to followers

        [Route("verifyfollowerotp/{OTP}")]
        [HttpPut("{OTP}")]
        public async Task<JsonResult> FollowerOTP_Verification(int OTP)
        {
            string result = string.Empty;
            try
            {
                var otpcount = await _context.NGCOREJWT_FollowersLogins.Where(x => x.OTP == OTP && x.IsActive == true && x.IsCheckedOTP == false
                               && x.IsRegistered == true).CountAsync();

                if (otpcount == 1)
                {
                    var updateotp = await _context.NGCOREJWT_FollowersLogins.FirstOrDefaultAsync(x => x.OTP == OTP && x.IsActive == true && x.IsCheckedOTP == false);
                    updateotp.UpdatedDate = DateTime.Now;
                    updateotp.IsCheckedOTP = true;
                    result = "success";
                }
                else
                {
                    result = "failure";
                }

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Verify_Follower_OTP_ToFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            await _context.SaveChangesAsync();

            return new JsonResult(result);
        }

        // get the user details

        [Route("getuserdetails/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<int> GetUserDetails(string FullName)
        {
            var chkuser = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == FullName &&
            x.IsActive == true && x.PersonalityType == "Celebrity").CountAsync();

            return chkuser;

        }

        // update the profile

        [Route("updateprofile")]
        [HttpPut()]
        public async Task<ActionResult<IEnumerable<NGCOREJWT_UserLogin>>> UpdateCelebProfile([FromForm] UpdateCP_formdata updfdata)
        {
            try
            {
                var updprof = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == updfdata.Username && i.IsActive == true
                && i.IsRegistered == true).FirstOrDefaultAsync();

                string newpath = Path.Combine("ProfileCoverImg");

                var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, updfdata.Username, DateTime.Now.ToString("dd-MM-yyyy")));

                if (!directoryinfo.Exists)
                {
                    directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, updfdata.Username, DateTime.Now.ToString("dd-MM-yyyy")));
                }

                var imgfilename = updfdata.FileName.FileName.Trim('"');
                if (System.IO.File.Exists(imgfilename))
                {
                    System.IO.File.Delete(imgfilename);
                }

                var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                var pathToSave = Path.Combine(finalpath, imgfilename);

                using (var stream = new FileStream(pathToSave, FileMode.Create))
                {
                    await updfdata.FileName.CopyToAsync(stream);
                }
                if (updprof != null)
                {
                    updprof.ProfileBio = updfdata.Profilebio;
                    updprof.FileName = imgfilename;
                    updprof.FileType = Path.GetExtension(imgfilename);
                    updprof.CreatedDate = DateTime.Now;
                    updprof.CreatedBy = updfdata.CreatedBy;
                    await _context.SaveChangesAsync();
                }

                await EmailNotification(updfdata.Username + " has uploaded " + imgfilename, "", "New Picture Uploaded", 6, updfdata.Username);

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Update_ProfileCover_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Get the Profile Bio Details

        [Route("getprofilebio/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<List<InsertUserDetails>> GetCelebBio(string FullName)
        {
            var updprofbio = await (from usrlogin in _context.NGCOREJWT_UserLogins
                                    where usrlogin.FullName == FullName && usrlogin.IsActive == true
                                    select new InsertUserDetails()
                                    {
                                        FullName = usrlogin.FullName,
                                        ProfileBio = usrlogin.ProfileBio,
                                        FollowersCount = usrlogin.FollowersCount,
                                        YoutubeLink = usrlogin.CreatedBy
                                    }).ToListAsync();
            return updprofbio;
        }

        // update the profile cover picture

        [Route("updateprofilecover")]
        [HttpPost]
        //  [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<NGCOREJWT_UserProfileDetail>> UpdateCoverPicture([FromForm] InsertImageVideo files)
        {
            try
            {
                var newpath = Path.Combine("ProfileStorage");
                var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, files.FullName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));

                if (!directoryinfo.Exists)
                {
                    directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, files.FullName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));
                }
                var fileName = files.FileName.FileName.Trim('"');

                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                var pathToSave = Path.Combine(finalpath, fileName);
                var fullPath = (dynamic)null;
                string strextension = Path.GetExtension(fileName);
                if (strextension == ".jpg" || strextension == ".png" || strextension == ".jpeg")
                {
                    fullPath = Path.Combine(finalpath, fileName);
                }
                else
                {
                    fullPath = Path.Combine(finalpath, fileName);
                }

                // var filesize = files.MultiFiles.Length;

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    files.FileName.CopyTo(stream);
                }

                var updprof = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == files.FullName && i.IsActive == true && i.IsRegistered == true).FirstOrDefaultAsync();

                if (updprof != null)
                {
                    var updusrprof = await _context.NGCOREJWT_UserProfileDetails.Where(i => i.UserLoginFKID == updprof.UserLoginPKID && i.IsActive == true).FirstOrDefaultAsync();

                    if (updusrprof == null)
                    {
                        NGCOREJWT_UserProfileDetail usrprofdet = new NGCOREJWT_UserProfileDetail();
                        usrprofdet.UserLoginFKID = updprof.UserLoginPKID;
                        usrprofdet.FileName = fileName;
                        usrprofdet.FileType = Path.GetExtension(fileName);
                        usrprofdet.IsActive = true;
                        usrprofdet.IsDeleted = false;
                        usrprofdet.CreatedBy = "System";
                        usrprofdet.UpdatedBy = "System";
                        usrprofdet.CreatedDate = DateTime.Now;
                        usrprofdet.UpdatedDate = DateTime.Now;

                        _context.NGCOREJWT_UserProfileDetails.Add(usrprofdet);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        updusrprof.FileName = fileName;
                        updusrprof.FileType = Path.GetExtension(fileName);
                        //   updusrprof.FileSize = null;
                        //   updusrprof.CreatedBy = "System";
                        //   updusrprof.UpdatedBy = "System";
                        // updusrprof.UpdatedDate = DateTime.Now;
                        updusrprof.CreatedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                // Send Email Notification

                var credentials = new NetworkCredential("connectmuthu28@gmail.com", "ForverJoya$2019");

                var mail = new MailMessage()
                {
                    From = new MailAddress("connectmuthu28@gmail.com"),
                    Subject = "New Profile Cover Uploaded",
                    Body = files.FullName + " has uploaded " + files.FileName.FileName
                };

                mail.IsBodyHtml = true;
                mail.To.Add(new MailAddress("suryamsd.96@gmail.com"));

                var smtpclient = new SmtpClient()
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Credentials = credentials
                };
                smtpclient.Send(mail);
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Update_ProfileStorage_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Get the File Extension

        [Route("getfilextension/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<JsonResult> GetFileExtension(string FullName)
        {
            var fileName = await GetFileName(FullName);
            string strextension = Path.GetExtension(fileName);
            return new JsonResult(strextension);
        }

        // Get the Update Cover Picture

        [Route("bindprofilepicture/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<FileStream> BindProfilePicture(string FullName)
        {
            var path = (dynamic)null;
            var filepath = (dynamic)null;
            try
            {
                var files = await (from user in _context.NGCOREJWT_UserLogins
                                   join usrdet in _context.NGCOREJWT_UserProfileDetails on user.UserLoginPKID equals usrdet.UserLoginFKID
                                   where user.IsActive == true && user.FullName == FullName && usrdet.IsActive == true
                                   select new NGCOREJWT_UserProfileDetail()
                                   {
                                       FileName = usrdet.FileName.Trim('"'),
                                       CreatedDate = usrdet.CreatedDate
                                   }).FirstOrDefaultAsync();
                var memory = new MemoryStream();

                if (files != null)
                {
                    filepath = Path.Combine("ProfileStorage", FullName, Convert.ToDateTime(files.CreatedDate).ToString("dd-MM-yyyy"));
                    path = Path.Combine(
                            _hostingEnvironment.ContentRootPath, filepath,
                            files.FileName);

                    if (System.IO.File.Exists(path))
                    {
                        path = Path.Combine(
                             _hostingEnvironment.ContentRootPath, filepath,
                             files.FileName);
                    }
                    else
                    {
                        var sampleavatar = Path.Combine("ProfileCoverImg", "sampleavatar");
                        path = Path.Combine(
                            _hostingEnvironment.ContentRootPath, sampleavatar,
                            "sample avatar.png");
                    }

                }
                else
                {
                    var sampleavatar = Path.Combine("ProfileCoverImg", "sampleavatar");
                    path = Path.Combine(
                        _hostingEnvironment.ContentRootPath, sampleavatar,
                        "sample avatar.png");
                }
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "ProfileStorage_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        // Bind the Profile Cover Image

        [Route("bindprofilecover/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<FileStream> BindProfileCover(string FullName)
        {
            var path = (dynamic)null;
            var filepath = (dynamic)null;
            var sampleavatar = (dynamic)null;
            var memory = new MemoryStream();
            try
            {
                int usercount = await _context.NGCOREJWT_UserLogins.Where(u => u.FullName == FullName && u.IsActive == true && u.IsRegistered == true
                          && u.IsApproved == true && u.PersonalityType == "Celebrity" && u.FileName != null).CountAsync();
                if (usercount == 1)
                {
                    var files = await (from user in _context.NGCOREJWT_UserLogins
                                       where user.IsActive == true && user.FullName == FullName && user.IsRegistered == true
                                       && user.IsApproved == true
                                       select new NGCOREJWT_UserLogin()
                                       {
                                           FileName = user.FileName.Trim('"'),
                                           CreatedDate = user.CreatedDate
                                       }).FirstOrDefaultAsync();

                    if (files != null)
                    {
                        filepath = Path.Combine("ProfileCoverImg", FullName, Convert.ToDateTime(files.CreatedDate).ToString("dd-MM-yyyy"));
                        path = Path.Combine(
                               _hostingEnvironment.ContentRootPath, filepath,
                               files.FileName);

                        if (System.IO.File.Exists(path))
                        {
                            path = Path.Combine(
                                _hostingEnvironment.ContentRootPath, filepath,
                                files.FileName);
                        }
                        else
                        {
                            sampleavatar = Path.Combine("ProfileCoverImg", "sampleavatar");
                            path = Path.Combine(
                                _hostingEnvironment.ContentRootPath, sampleavatar,
                                "sample avatar.png");
                        }
                    }
                    else
                    {
                        sampleavatar = Path.Combine("ProfileCoverImg", "sampleavatar");
                        path = Path.Combine(
                            _hostingEnvironment.ContentRootPath, sampleavatar,
                            "sample avatar.png");
                    }
                }
                else
                {
                    sampleavatar = Path.Combine("ProfileCoverImg", "sampleavatar");
                    path = Path.Combine(
                        _hostingEnvironment.ContentRootPath, sampleavatar,
                        "sample avatar.png");
                }
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Bind_ProfileCoverImg_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }


        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        // Get the File Name

        // Get the Profile Bio Details

        //[Route("getfilename/{FullName}")]
        //[HttpGet("{FullName}")]
        public async Task<string> GetFileName(string FullName)
        {
            var filepath = (dynamic)null;
            var loginpk = await (from usrlogin in _context.NGCOREJWT_UserLogins
                                 join userprof in _context.NGCOREJWT_UserProfileDetails on usrlogin.UserLoginPKID equals userprof.UserLoginFKID
                                 where usrlogin.FullName == FullName && usrlogin.IsActive == true && usrlogin.IsApproved == true
                                 && usrlogin.IsRegistered == true && usrlogin.PersonalityType == "Celebrity" && userprof.IsActive == true
                                 select new NGCOREJWT_UserProfileDetail()
                                 {
                                     FileName = userprof.FileName.Trim('"')
                                 }).FirstOrDefaultAsync();
            if (loginpk == null)
            {
                filepath = "sample avatar.png";
            }
            else
            {
                filepath = loginpk.FileName;
            }
            return filepath;
        }

        //public async Task<string> GetArtistImgFileName(string FullName)
        //{
        //    var loginpk = await (from usrlogin in _context.NGCOREJWT_UserLogins
        //                         where usrlogin.FullName == FullName && usrlogin.IsActive == true
        //                         select new UserLogin()
        //                         {
        //                             UserLoginPKID = usrlogin.UserLoginPKID
        //                         }).FirstOrDefaultAsync();

        //    var fulname = await (from artist in _context.ArtistCollections
        //                         where artist.UserLoginFKID == loginpk.UserLoginPKID && artist.IsActive == true
        //                         select new ArtistCollection()
        //                         {
        //                             FileName = artist.FileName.Trim('"')
        //                         }).FirstOrDefaultAsync();

        //    return fulname.FileName;
        //}

        //// Update the Poster Title

        //[Route("updatepostertitle")]
        //[HttpPut]
        //public async Task<ActionResult<UserService>> UpdatePosterTitle([FromForm] InsertServicePost updservice)
        //{
        //    try
        //    {
        //        var updpts = await _context.UserServices.Where(j => j.UserServicePKID == updservice.UserServicePKID && j.IsActive == true).FirstOrDefaultAsync();

        //        if (updpts != null)
        //        {
        //            updpts.PosterTitleFKID = updservice.PosterTitleFKID;
        //            updpts.PosterPriceFKID = updservice.PosterPriceFKID;
        //            updpts.NewService = updservice.NewService.Trim('"');
        //            updpts.Price = updservice.Price;
        //            updpts.UpdatedDate = DateTime.Now;

        //            await _context.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var exceptions = exceptionlog.SendExcepToDB(ex, "UpdatePosterTitle_Price_ByCelebrity");
        //        _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
        //        await _context.SaveChangesAsync();
        //    }
        //    return NoContent();
        //}

        //// Insert the Poster Title

        //[Route("insertnewservice")]
        //[HttpPost]
        //public async Task<ActionResult<UserService>> InsertServiceorPost([FromForm] InsertServicePost service)
        //{
        //    try
        //    {
        //        var getloginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == service.FullName && i.IsActive == true && i.IsRegistered == true).FirstOrDefaultAsync();

        //        if (getloginid != null)
        //        {
        //            UserService usrservice = new UserService();
        //            usrservice.UserLoginFKID = getloginid.UserLoginPKID;
        //            usrservice.PosterTitleFKID = service.PosterTitleFKID;
        //            usrservice.PosterPriceFKID = service.PosterPriceFKID;
        //            usrservice.NewService = service.NewService.Trim('"');
        //            usrservice.Price = service.Price;
        //            usrservice.IsActive = true;
        //            usrservice.IsLocked = false;
        //            usrservice.IsDeleted = false;
        //            usrservice.CreatedBy = "System";
        //            usrservice.UpdatedBy = "System";
        //            usrservice.CreatedDate = DateTime.Now;
        //            usrservice.UpdatedDate = DateTime.Now;

        //            _context.UserServices.Add(usrservice);
        //            await _context.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var exceptions = exceptionlog.SendExcepToDB(ex, "InsertNewServices_ByCelebrity");
        //        _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
        //        await _context.SaveChangesAsync();
        //    }
        //    return NoContent();
        //}

        //// Sync and bind the new services created by the celebrity

        //[Route("getnewservices/{FullName}")]
        //[HttpGet("{FullName}")]
        //public async Task<List<UserService>> GetNewServices(string FullName)
        //{
        //    var newservice = (dynamic)null;
        //    var getloginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == FullName && i.IsActive == true && i.IsRegistered == true).FirstOrDefaultAsync();

        //    if (getloginid != null)
        //    {
        //        var getserviceid = await _context.UserServices.Where(i => i.UserLoginFKID == getloginid.UserLoginPKID && i.IsActive == true).FirstOrDefaultAsync();

        //        if (getserviceid != null)
        //        {
        //            newservice = await (from celservices in _context.UserServices
        //                                where celservices.UserLoginFKID == getserviceid.UserLoginFKID && celservices.IsActive == true
        //                                select new UserService()
        //                                {
        //                                    UserServicePKID = celservices.UserServicePKID,
        //                                    NewService = celservices.NewService,
        //                                    Price = celservices.Price
        //                                }).ToListAsync();
        //        }
        //    }

        //    return newservice;
        //}

        //[Route("getexistingservices/{UserServicePKID}")]
        //[HttpGet("{UserServicePKID}")]
        //public async Task<List<NGCOREJWT_UserService>> GetExistingServices(int UserServicePKID)
        //{
        //    var syncexisting = (dynamic)null;
        //    var getserviceid = await _context.NGCOREJWT_UserServices.Where(i => i.UserServicePKID == UserServicePKID && i.IsActive == true).FirstOrDefaultAsync();

        //    if (getserviceid != null)
        //    {
        //        syncexisting = await (from celservices in _context.NGCOREJWT_UserServices
        //                              join pts in _context.PosterTitles on celservices.PosterTitleFKID equals pts.PosterTitlePKID
        //                              join pps in _context.PosterPrices on celservices.PosterPriceFKID equals pps.PosterPricePKID
        //                              where celservices.UserServicePKID == getserviceid.UserServicePKID && celservices.IsActive == true
        //                              && pts.IsActive == true && pps.IsActive == true
        //                              select new UserService()
        //                              {
        //                                  UserServicePKID = celservices.UserServicePKID,
        //                                  PosterTitleFKID = celservices.PosterTitleFKID,
        //                                  PosterPriceFKID = celservices.PosterPriceFKID,
        //                                  NewService = celservices.NewService,
        //                                  Price = celservices.Price
        //                              }).ToListAsync();
        //    }


        //    return syncexisting;
        //}

        //// Bind the Poster Titles

        //[Route("bindpostertitles")]
        //[HttpGet]
        //public async Task<List<PosterTitle>> BindPosterTitles()
        //{
        //    var getpts = await (from pts in _context.PosterTitles
        //                        where pts.IsActive == true
        //                        select new PosterTitle()
        //                        {
        //                            PosterTitlePKID = pts.PosterTitlePKID,
        //                            PostersTitle = pts.PostersTitle
        //                        }).ToListAsync();
        //    return getpts;
        //}

        // Bind the Poster Prices

        [Route("bindposterprices")]
        [HttpGet]
        public async Task<List<NGCOREJWT_ContentsPrice>> BindPosterPrices()
        {
            var getpts = await (from pps in _context.NGCOREJWT_ContentsPrices
                                where pps.IsActive == true
                                select new NGCOREJWT_ContentsPrice()
                                {
                                    ContentPricePKID = pps.ContentPricePKID,
                                    ContentPrice = pps.ContentPrice
                                }).ToListAsync();
            return getpts;
        }


        // Sync the Album poster prices

        [Route("bindalbumprices")]
        [HttpGet]
        public async Task<List<NGCOREJWT_ContentsPrice>> BindAlbumPrices()
        {
            var posterprice = await (from albums in _context.NGCOREJWT_ContentsPrices
                                     where albums.IsActive == true
                                     select new NGCOREJWT_ContentsPrice()
                                     {
                                         ContentPricePKID = albums.ContentPricePKID,
                                         ContentPrice = albums.ContentPrice
                                     }).ToListAsync();
            return posterprice;
        }

        // Check the record count in UserLogin Table

        [Route("userloginrecount/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<int> CheckUserLogin(string FullName)
        {
            var usrlogin = await _context.NGCOREJWT_UserLogins.Where(u => u.FullName == FullName && u.IsActive == true).CountAsync();
            return usrlogin;
        }

        // Check the record count in UserProfileDetails Table

        [Route("profiledetrecount/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<int> CheckUserProfileDetails(string FullName)
        {
            var profdet = await (from login in _context.NGCOREJWT_UserLogins
                                 join usrprof in _context.NGCOREJWT_UserProfileDetails on login.UserLoginPKID equals usrprof.UserLoginFKID
                                 where login.IsActive == true && usrprof.IsActive == true && login.FullName == FullName
                                 select new NGCOREJWT_UserProfileDetail()).CountAsync();
            return profdet;
        }


        //[Route("getalbumdet_popup/{albumpostpricepkid}/{albumcaption}")]
        //[HttpGet("{albumpostpricepkid}/{albumcaption}")]
        //public async Task<List<GetAlbumDetails_Popup>> GetAlbumDetails_Popup(int albumpostpricepkid, string albumcaption)
        //{
        //    var albumdetpopup = (dynamic)null;
        //    var getserviceid = await _context.AlbumPosterPrices.Where(i => i.AlbumPosterPricePKID == albumpostpricepkid && i.IsActive == true).FirstOrDefaultAsync();

        //    if (getserviceid != null)
        //    {
        //        albumdetpopup = await (from album in _context.AlbumCollections
        //                               join price in _context.AlbumPosterPrices on album.PosterPriceFKID equals price.AlbumPosterPricePKID
        //                               group album by new
        //                               {
        //                                   album.AlbumCaption,
        //                                   album.IsActive,
        //                                   price.AlbumPostersPrice,
        //                                   price.AlbumPosterPricePKID
        //                               }
        //                            into albums
        //                               where albums.Key.AlbumPosterPricePKID == albumpostpricepkid
        //                               && albums.Key.IsActive == true && albums.Key.AlbumCaption == albumcaption
        //                               select new GetAlbumDetails_Popup()
        //                               {
        //                                   AlbumCaption = albums.Key.AlbumCaption,
        //                                   PriceInfo = albums.Key.AlbumPostersPrice,
        //                                   PosterPricePKID = albums.Key.AlbumPosterPricePKID
        //                               }).ToListAsync();
        //    }

        //    return albumdetpopup;
        //}


        // save the pdf invoice and send to email
        [Route("sendpdf_mail")]
        [HttpPost]
        public async Task<ActionResult> SendPDF_Mail([FromForm] SendPDF_Email sendpdf)
        {
            try
            {
                var usermgr = await _userManager.FindByEmailAsync(sendpdf.FollowerEmailID);
                var findname = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == usermgr.Email && i.IsActive == true && i.IsApproved == true
                && i.UserType == "Follower" && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(l => l.FullName == sendpdf.CelebrityName).Select
                (s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();
                string newpath = Path.Combine("InvoiceFile");
                var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, sendpdf.CelebrityName, findname.FollowersName,
                                    Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));
                var pdfile = Path.GetFileName(sendpdf.FileName_Type.FileName);
                var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);
                var pathToSave = Path.Combine(finalpath, pdfile + ".pdf");

                using (var stream = new FileStream(pathToSave, FileMode.Create))
                {
                    await sendpdf.FileName_Type.CopyToAsync(stream);
                }

                var credentials = new NetworkCredential(emailconf.From, emailconf.Password);

                var pdfmail = new MailMessage()
                {
                    From = new MailAddress(emailconf.From),
                    Subject = "Unlocked Content Video - PDF Invoice",
                    Body = "Please check the PDF Invoice"
                };

                var recepients = await _context.NGCOREJWT_PersonalEmailIDs.Where(i => i.IsActive == true).ToListAsync();
                StringBuilder strbuild = new StringBuilder();
                string appemailids = string.Empty;

                foreach (var emailids in recepients)
                {
                    pdfmail.To.Add(new MailAddress(emailids.PEmailID));
                    strbuild.Append(emailids.PEmailID);
                    strbuild.Append(",");
                }
                appemailids = strbuild.ToString().TrimEnd(',');
                pdfmail.IsBodyHtml = true;

                pdfmail.Attachments.Add(new Attachment(sendpdf.FileName_Type.OpenReadStream(), sendpdf.FileName_Type.FileName, "application/pdf"));

                var smtpclient = new SmtpClient()
                {
                    Port = emailconf.Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = emailconf.SmtpServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                await smtpclient.SendMailAsync(pdfmail);

                NGCOREJWT_Email_Notification notification = new NGCOREJWT_Email_Notification();
                notification.SenderEmail = pdfmail.From.ToString();
                notification.Recepients = appemailids;
                notification.Subject = pdfmail.Subject;
                notification.MessageBody = pdfmail.Subject;
                notification.CreatedBy = "System";
                notification.CreatedDate = DateTime.Now;

                await _context.NGCOREJWT_Email_Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "GeneratePDF_EmailNotification");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }




        [Route("getusersemailid/{celebrityname}/{followersname}")]
        [HttpGet("{celebrityname}/{followersname}")]
        public async Task<IEnumerable<NGCOREJWT_FollowersLogin>> GetFollowersEmailID(string celebrityname, string followersname)
        {
            var useremailid = await (from usr in _context.NGCOREJWT_UserLogins
                                     join flog in _context.NGCOREJWT_FollowersLogins on usr.UserLoginPKID equals flog.UserLoginFKID
                                     where flog.IsActive == true && usr.IsActive == true && flog.IsApproved == true && flog.IsRegistered == true
                                     && flog.FollowersName == followersname && usr.IsActive == true && usr.IsApproved == true && usr.IsRegistered == true
                                     && usr.PersonalityType == "Celebrity" && usr.FullName == celebrityname
                                     select new NGCOREJWT_FollowersLogin()
                                     {
                                         EmailID = flog.EmailID,
                                         MobileNumber = flog.MobileNumber
                                     }).ToListAsync();
            return useremailid;
        }

        // Find the followers name 

        [Route("findfollowersname/{followeremailid}/{celebrityname}")]
        [HttpGet("{followeremailid}/{celebrityname}")]
        public async Task<NGCOREJWT_FollowersLogin> FindFollowersName(string followeremailid, string celebrityname)
        {
            var verifyfollower = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == followeremailid && i.IsActive == true &&
                                 i.IsRegistered == true && i.IsApproved == true).CountAsync();
            var follower = (dynamic)null;

            try
            {
                if (verifyfollower >= 1)
                {
                    var celebritypkid = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == celebrityname && x.IsActive == true
                    && x.IsRegistered == true && x.IsApproved == true && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                    if (celebritypkid.ViewersVisitCount == null)
                    {
                        celebritypkid.ViewersVisitCount = 0;
                    }
                    celebritypkid.ViewersVisitCount += 1;

                    await _context.SaveChangesAsync();

                    var indfollcount = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == followeremailid.Trim('"') && i.IsActive == true
                    && i.IsRegistered == true && i.IsApproved == true && i.UserLoginFKID == celebritypkid.UserLoginPKID).CountAsync();

                    if (indfollcount == 0)
                    {
                        var followersinfo = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == followeremailid.Trim('"') && i.IsActive == true
                        && i.IsRegistered == true && i.IsApproved == true).FirstOrDefaultAsync();

                        var followerlogin = savefollowers.SaveFollowersLogin(followersinfo.FollowersName.Trim('"'), followersinfo.EmailID.Trim('"'),
                                                   followersinfo.MobileNumber.Trim('"'), celebritypkid.UserLoginPKID);

                        await _context.NGCOREJWT_FollowersLogins.AddAsync(followerlogin);

                        //if (celebritypkid.FollowersCount == null)
                        //{
                        //    celebritypkid.FollowersCount = 0;
                        //}
                        //celebritypkid.FollowersCount += 1;
                        await _context.SaveChangesAsync();

                        var lastupdfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.EmailID == followeremailid.Trim('"') && f.IsActive == true
                        && f.IsRegistered == true && f.IsApproved == true && f.UserLoginFKID == celebritypkid.UserLoginPKID).FirstOrDefaultAsync();

                        var unlockedcontents = await _context.NGCOREJWT_UnlockedContents.Where(x => x.IsActive == true && x.CelebrityLoginFKID == lastupdfollower.UserLoginFKID).ToListAsync();

                        foreach (var saveulmfs in unlockedcontents)
                        {
                            NGCOREJWT_UnlockedContent_MF unlockedmfs = new NGCOREJWT_UnlockedContent_MF();
                            unlockedmfs.CelebrityLoginFKID = saveulmfs.CelebrityLoginFKID;
                            unlockedmfs.FollowersLoginFKID = lastupdfollower.FollowersLoginPKID; //followersinfo.UserLoginFKID;
                            unlockedmfs.UnlockedContentFKID = saveulmfs.UnlockedContentPKID;
                            unlockedmfs.ContentPrice = saveulmfs.ContentPrice;
                            unlockedmfs.ContentPriceFKID = saveulmfs.ContentPriceFKID;
                            unlockedmfs.ContentType = saveulmfs.ContentType;
                            unlockedmfs.ContentCaption = saveulmfs.ContentCaption;
                            unlockedmfs.ContentDesc = saveulmfs.ContentDesc;
                            unlockedmfs.IconPath = saveulmfs.IconPath1;
                            unlockedmfs.ImagePath = saveulmfs.ImagePath1;
                            unlockedmfs.DupFileName = saveulmfs.FileName;
                            unlockedmfs.IsActive = true;
                            unlockedmfs.IsDeleted = false;
                            unlockedmfs.IsLocked = false;
                            unlockedmfs.CreatedBy = "System";
                            unlockedmfs.UpdatedBy = "System";
                            unlockedmfs.CreatedDate = saveulmfs.CreatedDate;
                            unlockedmfs.UpdatedDate = DateTime.Now;

                            await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(unlockedmfs);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {

                    }
                    follower = await (from fls in _context.NGCOREJWT_FollowersLogins
                                      join user in _context.NGCOREJWT_UserLogins on fls.UserLoginFKID equals user.UserLoginPKID
                                      where fls.IsActive == true && fls.EmailID == followeremailid && fls.IsRegistered == true
                                      && fls.IsApproved == true && user.FullName == celebrityname && user.PersonalityType == "Celebrity"
                                      select new NGCOREJWT_FollowersLogin()
                                      {
                                          FollowersName = fls.FollowersName,
                                          UserLoginFKID = fls.UserLoginFKID
                                      }).FirstOrDefaultAsync();

                }
                else
                {
                    follower = await (from fls in _context.NGCOREJWT_FollowersLogins
                                      join user in _context.NGCOREJWT_UserLogins on fls.UserLoginFKID equals user.UserLoginPKID
                                      where fls.IsActive == true && fls.EmailID == followeremailid && fls.IsRegistered == true
                                      && fls.IsApproved == true && user.FullName == celebrityname && user.PersonalityType == "Celebrity"
                                      select new NGCOREJWT_FollowersLogin()
                                      {
                                          FollowersName = fls.FollowersName,
                                          UserLoginFKID = fls.UserLoginFKID
                                      }).FirstOrDefaultAsync();
                }

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Create_NewFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return follower;
        }

        // Find the celebrity name 

        [Route("findcelebrityname/{celebrityname}")]
        [HttpGet("{celebrityname}")]
        public async Task<NGCOREJWT_UserLogin> FindCelebrityName(string celebrityname)
        {
            var followerlogin = await (from userlog in _context.NGCOREJWT_UserLogins
                                       where userlog.IsActive == true && userlog.IsRegistered == true
                                       && userlog.IsApproved == true && userlog.FullName == celebrityname && userlog.PersonalityType == "Celebrity"
                                       select new NGCOREJWT_UserLogin()
                                       {
                                           FullName = userlog.FullName,
                                           ProfileBio = userlog.ProfileBio
                                       }).FirstOrDefaultAsync();
            return followerlogin;
        }

        // sum the celebrity amount

        //[Route("celebritypayment/{celebrityname}")]
        //[HttpGet("{celebrityname}")]
        //public async Task<PaymentSection> CelebrityPayment(string celebrityname)
        //{
        //    var celebpay = await (from userlog in _context.UserLogins
        //                          where userlog.IsActive == true && userlog.IsRegistered == true
        //                          && userlog.IsApproved == true && userlog.FullName == celebrityname && userlog.PersonalityType == "Celebrity"
        //                          select new PaymentSection()
        //                          {
        //                              Share_Celebrities = _context.PaymentSections.Where(x => x.Celebrity_FKID == userlog.UserLoginPKID).Sum(i => i.Share_Celebrities)
        //                          }).FirstOrDefaultAsync();
        //    return celebpay;
        //}

        // dashboard statistics

        [Route("dashstatistics/{celebrityname}")]
        [HttpGet("{celebrityname}")]
        public async Task<DashboardStat> DashboardStatistics(string celebrityname)
        {
            //var lastpost = await (from user in _context.UserLogins
            //                      join unlock in _context.UnlockedContent_MFs on user.UserLoginPKID equals unlock.CelebrityLoginFKID
            //                      where user.IsActive == true && user.IsRegistered == true && user.IsApproved == true
            //                      && user.FullName == celebrityname && user.PersonalityType == "Celebrity"
            //                      orderby unlock.UnlockedContent_MF_PKID descending
            //                      select unlock.CreatedDate).LastAsync();
            var dashboard = (dynamic)null;
            var chkuser = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == celebrityname && i.IsActive == true &&
            i.PersonalityType == "Celebrity").FirstOrDefaultAsync();
            int unlockedcount = await _context.NGCOREJWT_UnlockedContents.Where(j => j.CelebrityLoginFKID == chkuser.UserLoginPKID).CountAsync();

            if (unlockedcount >= 1)
            {
                dashboard = await (from userlog in _context.NGCOREJWT_UserLogins
                                   where userlog.IsActive == true && userlog.IsRegistered == true && userlog.IsApproved == true
                                   && userlog.FullName == celebrityname && userlog.PersonalityType == "Celebrity"
                                   select new DashboardStat()
                                   {
                                       LastPost = Convert.ToDateTime(_context.NGCOREJWT_UnlockedContents.Where(i => i.CelebrityLoginFKID == userlog.UserLoginPKID).OrderBy(o => o.UnlockedContentPKID).Select(u => u.CreatedDate).Last()),
                                       ViewersCount = userlog.ViewersVisitCount == null ? 0 : userlog.ViewersVisitCount,
                                       CelebrityShare = _context.NGCOREJWT_PaymentSections.Where(x => x.Celebrity_FKID == userlog.UserLoginPKID).Sum(i => i.Share_Celebrities),
                                       FollowersCount = userlog.FollowersCount == null ? 0 : userlog.FollowersCount
                                   }).FirstOrDefaultAsync();
            }
            else
            {
                dashboard = await (from userlog in _context.NGCOREJWT_UserLogins
                                   where userlog.IsActive == true && userlog.IsRegistered == true && userlog.IsApproved == true
                                   && userlog.FullName == celebrityname && userlog.PersonalityType == "Celebrity"
                                   select new DashboardStat()
                                   {
                                       LastPost = null,
                                       ViewersCount = userlog.ViewersVisitCount == null ? 0 : userlog.ViewersVisitCount,
                                       CelebrityShare = _context.NGCOREJWT_PaymentSections.Where(x => x.Celebrity_FKID == userlog.UserLoginPKID).Sum(i => i.Share_Celebrities),
                                       FollowersCount = userlog.FollowersCount == null ? 0 : userlog.FollowersCount
                                   }).FirstOrDefaultAsync();
            }


            return dashboard;
        }

        [Route("checksubscribers/{celebrityname}/{followersname}")]
        [HttpGet("{celebrityname}/{followersname}")]
        public async Task<SubscriberCheck> CheckSubscriber(string celebrityname, string followersname)
        {
            var chksubscriber = await (from userlog in _context.NGCOREJWT_UserLogins
                                       join flogin in _context.NGCOREJWT_FollowersLogins on userlog.UserLoginPKID equals flogin.UserLoginFKID
                                       where userlog.IsActive == true && userlog.IsRegistered == true && userlog.IsApproved == true
                                       && userlog.FullName == celebrityname && userlog.PersonalityType == "Celebrity"
                                       && flogin.FollowersName == followersname && flogin.IsActive == true
                                       select new SubscriberCheck()
                                       {
                                           IsSubscribed = flogin.IsSubscribed == false ? "SUBSCRIBE" : "SUBSCRIBED",
                                           TruerFalse = flogin.IsSubscribed
                                       }).FirstOrDefaultAsync();
            return chksubscriber;
        }

        // Bind the Account Types

        [Route("bindaccountypes")]
        [HttpGet]
        public async Task<List<NGCOREJWT_AccountsType>> BindAccountTypes()
        {
            var acctype = await (from account in _context.NGCOREJWT_AccountsTypes
                                 where account.IsActive == true
                                 select new NGCOREJWT_AccountsType()
                                 {
                                     AccountsTypePKID = account.AccountsTypePKID,
                                     AccountType = account.AccountType
                                 }).ToListAsync();
            return acctype;
        }

        // save the celebrity payoutbilling details
        [Route("insertpayoutdet")]
        [HttpPost]
        public async Task<ActionResult<NGCOREJWT_UserLogin>> InsertPayoutBillingDet([FromForm] PayoutBillingFormData payout)
        {
            try
            {
                var celebcount = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == payout.CelebrityName && x.IsApproved == true
                           && x.IsActive == true && x.IsRegistered == true && x.PersonalityType == "Celebrity" && x.IsApproved == true).CountAsync();

                if (celebcount == 1)
                {
                    var payoutcount = await _context.NGCOREJWT_PayoutBillings.Where(x => x.IsActive == true).CountAsync();
                    if (payoutcount == 0)
                    {
                        NGCOREJWT_PayoutBilling payoutBilling = new NGCOREJWT_PayoutBilling();
                        payoutBilling.PanCardNo = payout.PanCardNo;
                        payoutBilling.AadhaarCardNo = payout.AadhaarCardNo;
                        payoutBilling.BillingName = payout.BillingName;
                        payoutBilling.UpiAddress = payout.UpiAddress;
                        payoutBilling.IfscCode = payout.IfscCode;
                        payoutBilling.AccountNo = payout.AccountNo;
                        payoutBilling.AccountName = payout.AccountName;
                        payoutBilling.AccountTypeFKID = payout.AccountsTypePKID;
                        payoutBilling.CelebrityFKID = _context.NGCOREJWT_UserLogins.Where(i => i.FullName == payout.CelebrityName && i.IsActive == true).Select(s => s.UserLoginPKID).FirstOrDefault();
                        payoutBilling.IsDeleted = false;
                        payoutBilling.IsActive = true;
                        payoutBilling.CreatedBy = "System";
                        payoutBilling.UpdatedBy = "System";
                        payoutBilling.CreatedDate = DateTime.Now;
                        payoutBilling.UpdatedDate = DateTime.Now;

                        await _context.NGCOREJWT_PayoutBillings.AddAsync(payoutBilling);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "InsertCelebrity_New_PayoutBillingDetails");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Displaying the Payout Billings Details

        [Route("dispayoutdet/{CelebrityName}")]
        [HttpGet("{CelebrityName}")]
        public async Task<List<FillPayoutBillings>> DispPayoutBillingDet(string CelebrityName)
        {
            var acctype = await (from account in _context.NGCOREJWT_PayoutBillings
                                 join acctypes in _context.NGCOREJWT_AccountsTypes on account.AccountTypeFKID equals acctypes.AccountsTypePKID
                                 join usrlogin in _context.NGCOREJWT_UserLogins on account.CelebrityFKID equals usrlogin.UserLoginPKID
                                 where account.IsActive == true && usrlogin.IsActive == true && usrlogin.IsRegistered == true
                                 && acctypes.IsActive == true && usrlogin.FullName == CelebrityName
                                 select new FillPayoutBillings()
                                 {
                                     AadhaarCardNo = account.AadhaarCardNo,
                                     AccountName = account.AccountName,
                                     AccountNo = account.AccountNo,
                                     AccountType = acctypes.AccountType,
                                     BillingName = account.BillingName,
                                     IfscCode = account.IfscCode,
                                     PanCardNo = account.PanCardNo,
                                     UpiAddress = account.UpiAddress,
                                     AccountsTypePKID = account.AccountTypeFKID
                                 }).ToListAsync();
            return acctype;
        }

        // Disable the Payout button

        [Route("disablepayoutbtn/{CelebrityName}")]
        [HttpGet("{CelebrityName}")]
        public async Task<int> DisablePayoutButton(string CelebrityName)
        {
            int paycount = await (from account in _context.NGCOREJWT_PayoutBillings
                                  join user in _context.NGCOREJWT_UserLogins on account.CelebrityFKID equals user.UserLoginPKID
                                  where account.IsActive == true && user.IsActive == true && user.IsRegistered == true
                                  && user.FullName == CelebrityName && user.PersonalityType == "Celebrity"
                                  select new NGCOREJWT_PayoutBilling()).CountAsync();
            return paycount;
        }

        // Update the Payout Billing Details

        [Route("updatepayoutbill")]
        [HttpPut]
        public async Task<ActionResult<UpdatePayoutBillingDet>> UpdatePayoutBillings([FromForm] UpdatePayoutBillingDet updpaydet)
        {
            try
            {
                var celebrityid = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == updpaydet.CelebrityName && x.PersonalityType == "Celebrity" &&
                                  x.IsActive == true && x.IsRegistered == true && x.IsApproved == true).FirstOrDefaultAsync();
                var payoutbill = await _context.NGCOREJWT_PayoutBillings.Where(j => j.CelebrityFKID == celebrityid.UserLoginPKID && j.IsActive == true).FirstOrDefaultAsync();

                if (payoutbill != null)
                {
                    payoutbill.AccountNo = updpaydet.AccountNo;
                    payoutbill.IfscCode = updpaydet.IfscCode;
                    payoutbill.UpiAddress = updpaydet.UpiAddress;
                    payoutbill.AccountName = updpaydet.AccountName;
                    payoutbill.AccountTypeFKID = updpaydet.AccountsTypePKID;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UpdateCelebrity_Payout_Billing_Details");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Follower / User Notification - Get the new uploaded videos,audios and document contents

        [Route("notification_getuc_user/{CelebrityName}/{FollowersName}")]
        [HttpGet("{CelebrityName}/{FollowersName}")]
        public async Task<List<Follower_Notification_Details>> UnlockedContent_Notification_Follower(string CelebrityName, string FollowersName)
        {
            var celebfollower = await (from userlogin in _context.NGCOREJWT_UserLogins
                                       join fologin in _context.NGCOREJWT_FollowersLogins on userlogin.UserLoginPKID equals fologin.UserLoginFKID
                                       where userlogin.IsActive == true && fologin.IsActive == true && userlogin.FullName == CelebrityName
                                       && fologin.FollowersName == FollowersName
                                       select new GetCelebrity_Follower_Details()
                                       {
                                           CelebrityFKID = userlogin.UserLoginPKID,
                                           FollowersFKID = fologin.FollowersLoginPKID,
                                           IsSubscribed = fologin.IsSubscribed
                                       }).FirstOrDefaultAsync();

            var notify = await (from unlockmf in _context.NGCOREJWT_UnlockedContent_MFs
                                join st in _context.NGCOREJWT_Store_Thumbnails on unlockmf.UnlockedContentFKID equals st.UnlockedContentFKID
                                where unlockmf.IsActive == true && st.IsActive == true && unlockmf.CelebrityLoginFKID == celebfollower.CelebrityFKID
                                && unlockmf.FollowersLoginFKID == celebfollower.FollowersFKID && celebfollower.IsSubscribed == true
                                group unlockmf by new
                                {
                                    st.CreatedDate,
                                    st.ContentCaptions,
                                    st.ThumbnailImage,
                                    st.ContentType,
                                    st.FileName,
                                    unlockmf.IsLocked
                                } into unlockmfs
                                orderby unlockmfs.Key.CreatedDate descending
                                select new Follower_Notification_Details()
                                {
                                    ThumbnailImage = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(unlockmfs.Key.ThumbnailImage)),
                                    ContentCaptions = unlockmfs.Key.ContentCaptions,
                                    CreatedDate = Convert.ToDateTime(unlockmfs.Key.CreatedDate),
                                    ContentType = unlockmfs.Key.ContentType,
                                    VidAudFileName = unlockmfs.Key.FileName,
                                    ContentIsLocked = unlockmfs.Key.IsLocked
                                }).ToListAsync();
            return notify;
        }

        // Follower / User Notification Count - Get unlocked videos,audios and document contents

        [Route("notificationcount_getuc_user/{CelebrityName}/{FollowersName}")]
        [HttpGet("{CelebrityName}/{FollowersName}")]
        public async Task<int> UnlockedContent_NotificationCount_Follower(string CelebrityName, string FollowersName)
        {
            int notifycount = await (from sts in _context.NGCOREJWT_Store_Thumbnails
                                     join usrlog in _context.NGCOREJWT_UserLogins on sts.CelebrityFKID equals usrlog.UserLoginPKID
                                     join flogin in _context.NGCOREJWT_FollowersLogins on usrlog.UserLoginPKID equals flogin.UserLoginFKID
                                     where usrlog.FullName == CelebrityName && usrlog.IsActive == true && sts.IsActive == true
                                     && flogin.IsSubscribed == true && flogin.FollowersName == FollowersName
                                     select new NGCOREJWT_Store_Thumbnail()).CountAsync();
            return notifycount;
        }

        // Celebrity / Artist Notification - Get unlocked videos,audios and document by the follower

        [Route("notification_getuc_artist/{CelebrityName}")]
        [HttpGet("{CelebrityName}")]
        public async Task<List<Celebrity_Notification_Details>> UnlockedContent_Notification_Celeb(string CelebrityName)
        {
            var celebnotify = await (from unlockmf in _context.NGCOREJWT_UnlockedContent_MFs
                                     join st in _context.NGCOREJWT_Store_Thumbnails on unlockmf.UnlockedContentFKID equals st.UnlockedContentFKID
                                     join usrlog in _context.NGCOREJWT_UserLogins on st.CelebrityFKID equals usrlog.UserLoginPKID
                                     join flogin in _context.NGCOREJWT_FollowersLogins on unlockmf.FollowersLoginFKID equals flogin.FollowersLoginPKID
                                     where usrlog.FullName == CelebrityName && usrlog.IsActive == true && flogin.IsActive == true &&
                                     unlockmf.IsActive == true && st.IsActive == true && unlockmf.IsLocked == true
                                     orderby unlockmf.CreatedDate descending
                                     select new Celebrity_Notification_Details()
                                     {
                                         ThumbnailImage = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(st.ThumbnailImage)),
                                         ContentCaptions = flogin.FollowersName + " has unlocked the " + st.ContentType + " - " + st.ContentCaptions,
                                         CreatedDate = Convert.ToDateTime(unlockmf.CreatedDate),
                                         ContentType = st.ContentType,
                                         VidAudFileName = st.FileName
                                     }).ToListAsync();
            return celebnotify;
        }

        // Celebrity / Artist Notification Count - Get unlocked videos,audios and document contents

        [Route("notificationcount_getuc_artist/{CelebrityName}")]
        [HttpGet("{CelebrityName}")]
        public async Task<int> UnlockedContent_NotificationCount_Celebrity(string CelebrityName)
        {
            int notifycount_celeb = await (from unlmfs in _context.NGCOREJWT_UnlockedContent_MFs
                                           join usrlog in _context.NGCOREJWT_UserLogins on unlmfs.CelebrityLoginFKID equals usrlog.UserLoginPKID
                                           where usrlog.FullName == CelebrityName && usrlog.IsActive == true && unlmfs.IsActive == true
                                           && unlmfs.IsLocked == true && usrlog.IsActive == true
                                           select new NGCOREJWT_UnlockedContent_MF()).CountAsync();
            return notifycount_celeb;
        }



        // Subscribed by the follower

        [Route("updatesubscribe/{celebrityname}/{followersname}")]
        [HttpPut("{celebrityname}/{followersname}")]
        public async Task<ActionResult<NGCOREJWT_FollowersLogin>> UpdateSubscribed(string celebrityname, string followersname)
        {
            try
            {
                var fologin = await _context.NGCOREJWT_FollowersLogins.Where(i => i.FollowersName == followersname && i.IsActive == true && i.IsApproved == true
                && i.IsRegistered == true && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.IsActive == true && j.FullName == celebrityname
                && j.IsApproved == true && j.IsRegistered == true && j.PersonalityType == "Celebrity").Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                if (fologin != null)
                {
                    fologin.IsSubscribed = true;
                    fologin.UpdatedDate = DateTime.Now;
                }

                var getcelebdetails = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == celebrityname && i.IsActive == true && i.IsApproved == true
                && i.IsRegistered == true && i.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                if (getcelebdetails != null)
                {
                    if (getcelebdetails.FollowersCount == null)
                    {
                        getcelebdetails.FollowersCount = 0;
                    }
                    getcelebdetails.FollowersCount += 1;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "User_Subscription");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Audio Controller related methods

        // Upload the audio

        [Route("insertaudio")]
        [HttpPost]
        public async Task<ActionResult<NGCOREJWT_AudioCollection>> UploadAudio([FromForm] AudioFormData audio)
        {
            try
            {
                if (audio != null && audio.AudioFileName.Length > 0)
                {
                    // string foldername = "AudioCollections";
                    //  string userfolder = audio.UserName;
                    string newpath = Path.Combine("AudioCollections");
                    //    foreach (var file in audio.FileName)
                    //    {
                    var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, audio.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));

                    if (!directoryinfo.Exists)
                    {
                        directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, audio.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));
                    }
                    var fileName = Path.GetFileName(audio.AudioFileName.FileName.Trim('"'));

                    var audiothumb = Path.GetFileName(audio.AudioThumbnail.FileName.Trim('"'));


                    if (System.IO.File.Exists(fileName) || System.IO.File.Exists(audiothumb))
                    {
                        System.IO.File.Delete(fileName);
                        System.IO.File.Delete(audiothumb);
                    }

                    var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                    var pathToSave = Path.Combine(finalpath, fileName);

                    var audiothumbpath = Path.Combine(finalpath, audiothumb);

                    using (var stream = new FileStream(pathToSave, FileMode.Create))
                    {
                        await audio.AudioFileName.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(audiothumbpath, FileMode.Create))
                    {
                        await audio.AudioThumbnail.CopyToAsync(stream);
                    }

                    var getloginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == audio.UserName && i.IsActive == true &&
                    i.IsRegistered == true).FirstOrDefaultAsync();

                    if (getloginid != null)
                    {
                        NGCOREJWT_AudioCollection audios = new NGCOREJWT_AudioCollection();
                        audios.UserLoginFKID = getloginid.UserLoginPKID;
                        audios.ContentPriceFKID = audio.AlbumPosterPriceFKID;
                        audios.FileName = audio.AudioFileName.FileName.Trim('"');
                        audios.ContentType = Path.GetExtension(audio.AudioFileName.FileName.Trim('"'));
                        //MemoryStream ms = new MemoryStream();
                        //await audio.AudioThumbnail.CopyToAsync(ms);
                        //audios.AudioData = ms.ToArray();
                        audios.AudioCaption = audio.AudioCaption.Trim('"');
                        audios.AudioDesc = audio.AudioDesc.Trim('"');
                        audios.Audio_GSTCharges = audio.A_GSTCharges;
                        audios.Audio_ServiceCharges = audio.A_ServiceCharges;
                        audios.Audio_TotalCharges = audio.A_TotalCharges;
                        // audios.AudioThumbnail = audiothumb;
                        //DirectoryInfo di = new DirectoryInfo(pathToSave);
                        //FileInfo[] fileinfo = di.GetFiles();
                        //videos.VideoSize = fileinfo.Length;
                        audios.IsLocked = false;
                        audios.IsActive = true;
                        audios.IsDeleted = false;
                        audios.CreatedBy = "System";
                        audios.UpdatedBy = "System";
                        audios.CreatedDate = DateTime.Now;
                        audios.UpdatedDate = DateTime.Now;

                        _context.NGCOREJWT_AudioCollections.Add(audios);

                        NGCOREJWT_UnlockedContent unlockaudio = new NGCOREJWT_UnlockedContent();
                        unlockaudio.CelebrityLoginFKID = getloginid.UserLoginPKID;
                        unlockaudio.ContentPriceFKID = audio.AlbumPosterPriceFKID;
                        unlockaudio.ContentType = Path.GetExtension(audio.AudioFileName.FileName.Trim('"'));
                        unlockaudio.ContentCaption = audio.AudioCaption.Trim('"');
                        unlockaudio.FileName = audio.AudioFileName.FileName.Trim('"');
                        unlockaudio.ContentDesc = audio.AudioCaption.Trim('"');
                        unlockaudio.IconPath = "fa fa-lock";
                        unlockaudio.ImagePath = "assets/useraudiothumbnail.png";
                        unlockaudio.ContentPrice = audio.PostersPrice.Trim('"');
                        unlockaudio.IconPath1 = "fa fa-lock";
                        unlockaudio.ImagePath1 = "assets/useraudiothumbnail.png";
                        unlockaudio.IsActive = true;
                        unlockaudio.IsDeleted = false;
                        unlockaudio.IsLocked = false;
                        unlockaudio.CreatedBy = "System";
                        unlockaudio.UpdatedBy = "System";
                        unlockaudio.CreatedDate = DateTime.Now;
                        unlockaudio.UpdatedDate = DateTime.Now;
                        unlockaudio.UC_GSTCharges = audio.A_GSTCharges;
                        unlockaudio.UC_ServiceCharges = audio.A_ServiceCharges;
                        unlockaudio.UC_TotalCharges = audio.A_TotalCharges;
                        //MemoryStream audioms = new MemoryStream();
                        //await audio.AudioThumbnail.CopyToAsync(audioms);
                        //unlockaudio.UC_ThumbnailImage = audioms.ToArray();

                        _context.NGCOREJWT_UnlockedContents.Add(unlockaudio);
                        await _context.SaveChangesAsync();

                        var getfollowers = await _context.NGCOREJWT_FollowersLogins.Where(i => i.UserLoginFKID == getloginid.UserLoginPKID && i.IsActive == true
                        && i.IsRegistered == true && i.IsApproved == true).ToListAsync();

                        var contentid = await _context.NGCOREJWT_UnlockedContents.Where(i => i.ContentCaption == audio.AudioCaption &&
                        i.FileName == audio.AudioFileName.FileName.Trim('"') && i.ContentPrice == audio.PostersPrice &&
                        i.ContentType == Path.GetExtension(audio.AudioFileName.FileName.Trim('"')) && i.ContentPriceFKID == audio.AlbumPosterPriceFKID).FirstOrDefaultAsync();

                        if (getfollowers == null || getfollowers.Count == 0)
                        {
                            NGCOREJWT_Store_Thumbnail storeaudiothumb = new NGCOREJWT_Store_Thumbnail();
                            MemoryStream audthumb = new MemoryStream();
                            await audio.AudioThumbnail.CopyToAsync(audthumb);
                            storeaudiothumb.ThumbnailImage = audthumb.ToArray();
                            storeaudiothumb.ThumbnailPath = audio.AudioThumbnail.FileName.Trim('"');
                            storeaudiothumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                            storeaudiothumb.CelebrityFKID = getloginid.UserLoginPKID;
                            storeaudiothumb.ContentCaptions = audio.AudioCaption;
                            storeaudiothumb.UnlockedContentCount = 0;
                            storeaudiothumb.ContentType = contentid.ContentType;
                            storeaudiothumb.FileName = audio.AudioFileName.FileName.Trim('"');
                            storeaudiothumb.ContentPriceFKID = audio.AlbumPosterPriceFKID;
                            storeaudiothumb.IsActive = true;
                            storeaudiothumb.IsDeleted = false;
                            storeaudiothumb.CreatedBy = "System";
                            storeaudiothumb.CreatedDate = DateTime.Now;
                            storeaudiothumb.UpdatedBy = "System";
                            storeaudiothumb.UpdatedDate = DateTime.Now;

                            await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storeaudiothumb);

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            int audcount = 0;
                            foreach (var insertnewaudio in getfollowers)
                            {
                                NGCOREJWT_UnlockedContent_MF insertaudios = new NGCOREJWT_UnlockedContent_MF();
                                insertaudios.CelebrityLoginFKID = getloginid.UserLoginPKID;
                                insertaudios.FollowersLoginFKID = insertnewaudio.FollowersLoginPKID;
                                insertaudios.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                insertaudios.ContentPriceFKID = audio.AlbumPosterPriceFKID;
                                insertaudios.ContentPrice = audio.PostersPrice;
                                insertaudios.ContentType = contentid.ContentType;
                                insertaudios.ContentCaption = audio.AudioCaption;
                                insertaudios.ContentDesc = audio.AudioDesc;
                                insertaudios.IconPath = "fa fa-lock";
                                insertaudios.ImagePath = "assets/useraudiothumbnail.png";
                                insertaudios.FileName = null;
                                insertaudios.DupFileName = audio.AudioFileName.FileName.Trim('"');
                                insertaudios.IsActive = true;
                                insertaudios.IsDeleted = false;
                                insertaudios.IsLocked = false;
                                insertaudios.CreatedBy = "System";
                                insertaudios.CreatedDate = DateTime.Now;
                                insertaudios.UpdatedBy = "System";
                                insertaudios.UpdatedDate = DateTime.Now;
                                insertaudios.GSTCharges = audio.A_GSTCharges;
                                insertaudios.ServiceCharges = audio.A_ServiceCharges;
                                insertaudios.TotalCharges = audio.A_TotalCharges;
                                //MemoryStream mss = new MemoryStream();
                                //await audio.AudioThumbnail.CopyToAsync(mss);
                                //insertaudios.UC_ThumbnailImage_MF = mss.ToArray();

                                await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(insertaudios);
                                audcount += 1;
                                if (audcount == 1)
                                {
                                    NGCOREJWT_Store_Thumbnail storeaudiothumb = new NGCOREJWT_Store_Thumbnail();
                                    MemoryStream audthumb = new MemoryStream();
                                    await audio.AudioThumbnail.CopyToAsync(audthumb);
                                    storeaudiothumb.ThumbnailImage = audthumb.ToArray();
                                    storeaudiothumb.ThumbnailPath = audio.AudioThumbnail.FileName.Trim('"');
                                    storeaudiothumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                    storeaudiothumb.CelebrityFKID = getloginid.UserLoginPKID;
                                    storeaudiothumb.ContentCaptions = audio.AudioCaption;
                                    storeaudiothumb.UnlockedContentCount = 0;
                                    storeaudiothumb.ContentType = contentid.ContentType;
                                    storeaudiothumb.FileName = audio.AudioFileName.FileName.Trim('"');
                                    storeaudiothumb.ContentPriceFKID = audio.AlbumPosterPriceFKID;
                                    storeaudiothumb.IsActive = true;
                                    storeaudiothumb.IsDeleted = false;
                                    storeaudiothumb.CreatedBy = "System";
                                    storeaudiothumb.CreatedDate = DateTime.Now;
                                    storeaudiothumb.UpdatedBy = "System";
                                    storeaudiothumb.UpdatedDate = DateTime.Now;

                                    await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storeaudiothumb);

                                    await _context.SaveChangesAsync();
                                }

                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    await EmailNotification(audio.UserName, audio.AudioCaption, "New Audio Uploaded", 2, "");
                    //  }
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UploadAudio_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // To open the Audio file

        [Route("openaudiofile/{FullName}/{FileName}")]
        [HttpGet("{FullName}/{FileName}")]
        public async Task<FileStream> OpenAudioFile(string FullName, string FileName)
        {
            var path = (dynamic)null;
            try
            {
                var files = await (from user in _context.NGCOREJWT_UserLogins
                                   join audio in _context.NGCOREJWT_AudioCollections on user.UserLoginPKID equals audio.UserLoginFKID
                                   where user.IsActive == true && user.FullName == FullName && audio.FileName == FileName && audio.IsActive == true
                                   select new NGCOREJWT_VideoCollection()
                                   {
                                       FileName = audio.FileName.Trim('"'),
                                       CreatedDate = audio.CreatedDate
                                   }).FirstOrDefaultAsync();

                var filepath = Path.Combine("AudioCollections", FullName, Convert.ToDateTime(files.CreatedDate).ToString("dd-MM-yyyy"));

                path = Path.Combine(
                         _hostingEnvironment.ContentRootPath, filepath,
                         files.FileName);

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "AudioCollections_OpenAudio");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        // Load the Audio Collections

        [Route("loadaudiocollections/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<List<Audioshowdet>> LoadAudioCollections(string FullName)
        {
            var audiodet = await (from userlog in _context.NGCOREJWT_UserLogins
                                  join audios in _context.NGCOREJWT_AudioCollections on userlog.UserLoginPKID equals audios.UserLoginFKID
                                  join price in _context.NGCOREJWT_ContentsPrices on audios.ContentPriceFKID equals price.ContentPricePKID
                                  where audios.UserLoginFKID == userlog.UserLoginPKID && audios.IsActive == true && userlog.FullName == FullName
                                  && price.IsActive == true && userlog.IsActive == true && userlog.IsRegistered == true &&
                                  userlog.PersonalityType == "Celebrity" && userlog.IsApproved == true
                                  where userlog.IsActive == true && userlog.FullName == FullName && audios.IsActive == true
                                  orderby audios.CreatedDate descending
                                  select new Audioshowdet()
                                  {
                                      AudioCaption = audios.AudioCaption,
                                      AudioDesc = audios.AudioDesc,
                                      FileName = audios.FileName,
                                      Createdate = Convert.ToDateTime(audios.CreatedDate),
                                      PriceInfo = audios.Audio_TotalCharges,
                                      IsLocked = audios.IsLocked == false ? "Locked" : "",
                                      AlbumPosterPricePKID = price.ContentPricePKID,
                                      AudioCollectionPKID = audios.AudioCollectionPKID,
                                      AudioThumbnail = audios.AudioData == null ? "assets/useraudiothumbnail.png" : null
                                  }).ToListAsync();
            return audiodet;
        }

        // audio modal popup

        [Route("getaudiodet_popup/{audioposterpricepkid}/{audiocollectionpkid}/{audiocaptions}")]
        [HttpGet("{audioposterpricepkid}/{audiocollectionpkid}/{audiocaptions}")]
        public async Task<List<GetAudioDetails_Popup>> GetAudioDetails_Popup(int audioposterpricepkid, int audiocollectionpkid, string audiocaptions)
        {
            var audiodetpopup = (dynamic)null;
            var getserviceid = await _context.NGCOREJWT_ContentsPrices.Where(i => i.ContentPricePKID == audioposterpricepkid && i.IsActive == true).FirstOrDefaultAsync();

            if (getserviceid != null)
            {
                if (audiocollectionpkid == 0)
                {
                    audiodetpopup = await (from audcoll in _context.NGCOREJWT_AudioCollections
                                           join albumprice in _context.NGCOREJWT_ContentsPrices on audcoll.ContentPriceFKID equals albumprice.ContentPricePKID
                                           where audcoll.ContentPriceFKID == getserviceid.ContentPricePKID && audcoll.IsActive == true
                                           && audcoll.AudioCaption == audiocaptions
                                           select new GetAudioDetails_Popup()
                                           {
                                               AudioCaption = audcoll.AudioCaption,
                                               PriceInfo = Convert.ToInt32(audcoll.Audio_TotalCharges) * 100 / 100 + "/-",
                                               AudioCollectionPKID = audcoll.AudioCollectionPKID,
                                               AlbumPosterPricePKID = albumprice.ContentPricePKID
                                           }).ToListAsync();
                }
                else
                {
                    audiodetpopup = await (from audcoll in _context.NGCOREJWT_UnlockedContents
                                           join albumprice in _context.NGCOREJWT_ContentsPrices on audcoll.ContentPriceFKID equals albumprice.ContentPricePKID
                                           where audcoll.ContentPriceFKID == getserviceid.ContentPricePKID && audcoll.IsActive == true
                                           && audcoll.ContentCollectionFKID == audiocollectionpkid || audcoll.ContentCaption == audiocaptions
                                           select new GetAudioDetails_Popup()
                                           {
                                               AudioCaption = audcoll.ContentCaption,
                                               PriceInfo = Convert.ToInt32(audcoll.UC_TotalCharges) * 100 / 100 + "/-",
                                               AudioCollectionPKID = audcoll.ContentCollectionFKID,
                                               AlbumPosterPricePKID = albumprice.ContentPricePKID
                                           }).ToListAsync();
                }

            }

            return audiodetpopup;
        }

        // Update the Audio caption and price - UpdateAudioCap_Price

        [Route("updateaudiocap_price")]
        [HttpPut]
        public async Task<ActionResult<Update_AudioCap_Price_Celeb>> UpdateAudioCap_Price([FromForm] Update_AudioCap_Price_Celeb updateacap_price)
        {
            try
            {
                string resultmsg = string.Empty;

                var celebrityid = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == updateacap_price.CelebrityName && x.PersonalityType == "Celebrity" &&
                                  x.IsActive == true && x.IsRegistered == true && x.IsApproved == true).FirstOrDefaultAsync();

                var upd_audiocoll = await _context.NGCOREJWT_AudioCollections.Where(v => v.IsActive == true && v.FileName == updateacap_price.AudioFileName &&
                                v.UserLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == updateacap_price.AudioFileName &&
                                    v.CelebrityLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == updateacap_price.AudioFileName &&
                             v.CelebrityLoginFKID == celebrityid.UserLoginPKID).ToListAsync();

                var upd_thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == updateacap_price.AudioFileName &&
                                 v.CelebrityFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                if (upd_audiocoll != null && upd_unlock != null && upd_unlock_mfs != null && upd_thumbnails != null)
                {
                    upd_audiocoll.Audio_GSTCharges = updateacap_price.Audio_GSTCharges;
                    upd_audiocoll.Audio_ServiceCharges = updateacap_price.Audio_ServiceCharges;
                    upd_audiocoll.Audio_TotalCharges = updateacap_price.Audio_TotalCharges;
                    upd_audiocoll.ContentPriceFKID = updateacap_price.PosterPriceFKID;
                    upd_audiocoll.AudioCaption = updateacap_price.AudioCaption;
                    upd_audiocoll.UpdatedDate = DateTime.Now;

                    upd_unlock.UC_GSTCharges = updateacap_price.Audio_GSTCharges;
                    upd_unlock.UC_ServiceCharges = updateacap_price.Audio_ServiceCharges;
                    upd_unlock.UC_TotalCharges = updateacap_price.Audio_TotalCharges;
                    upd_unlock.ContentPriceFKID = updateacap_price.PosterPriceFKID;
                    upd_unlock.ContentPrice = updateacap_price.PriceInfo;
                    upd_unlock.ContentCaption = updateacap_price.AudioCaption;
                    upd_unlock.UpdatedDate = DateTime.Now;

                    foreach (var unlock_mfs in upd_unlock_mfs)
                    {
                        unlock_mfs.ContentPriceFKID = updateacap_price.PosterPriceFKID;
                        unlock_mfs.ContentPrice = updateacap_price.PriceInfo;
                        unlock_mfs.ContentCaption = updateacap_price.AudioCaption;
                        unlock_mfs.UpdatedDate = DateTime.Now;
                        unlock_mfs.GSTCharges = updateacap_price.Audio_GSTCharges;
                        unlock_mfs.ServiceCharges = updateacap_price.Audio_ServiceCharges;
                        unlock_mfs.TotalCharges = updateacap_price.Audio_TotalCharges;
                    }

                    upd_thumbnails.ContentPriceFKID = updateacap_price.PosterPriceFKID;
                    upd_thumbnails.ContentCaptions = updateacap_price.AudioCaption;
                    upd_thumbnails.UpdatedDate = DateTime.Now;

                    resultmsg = null;
                }
                else
                {
                    resultmsg = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UpdateCelebrity_Audio_Caption_Price");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Sync the Audio caption and price from the Audio Collection table - EditAudioCap_Price_Artist

        [Route("bindaudiodetails_celeb/{celebrityname}/{editaudiofile}")]
        [HttpGet("{celebrityname}/{editaudiofile}")]
        public async Task<Edit_AudioCap_Price_Celeb> GetCelebAudioDetails(string celebrityname, string editaudiofile)
        {
            var audiodet = await (from audio in _context.NGCOREJWT_AudioCollections
                                  join usr in _context.NGCOREJWT_UserLogins on audio.UserLoginFKID equals usr.UserLoginPKID
                                  join price in _context.NGCOREJWT_ContentsPrices on audio.ContentPriceFKID equals price.ContentPricePKID
                                  where price.IsActive == true && usr.FullName == celebrityname && usr.IsActive == true
                                  && audio.IsActive == true && audio.FileName == editaudiofile
                                  select new Edit_AudioCap_Price_Celeb()
                                  {
                                      AudioCaption = audio.AudioCaption,
                                      PosterPriceFKID = audio.ContentPriceFKID,
                                      PostersPrice = price.ContentPrice
                                  }).FirstOrDefaultAsync();
            return audiodet;
        }

        // Audio Deletion by the Celebrity

        [Route("deleteaudios/{celebrityname}/{audiocaptions}/{audiofilename}")]
        [HttpDelete("{celebrityname}/{audiocaptions}/{audiofilename}")]
        public async Task<JsonResult> DeleteAudios(string celebrityname, string audiocaptions, string audiofilename)
        {
            string result = string.Empty;
            try
            {
                var celebid = await _context.NGCOREJWT_UserLogins.Where(i => i.IsActive == true && i.IsRegistered == true && i.IsApproved == true &&
                          i.PersonalityType == "Celebrity" && i.FullName == celebrityname).FirstOrDefaultAsync();

                var audiocoll = await _context.NGCOREJWT_AudioCollections.Where(v => v.IsActive == true && v.FileName == audiofilename &&
                               v.AudioCaption == audiocaptions && v.UserLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlockcontent = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == audiofilename &&
                              v.ContentCaption == audiocaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlock = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == audiofilename &&
                              v.ContentCaption == audiocaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).ToListAsync();

                var thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == audiofilename &&
                             v.ContentCaptions == audiocaptions && v.CelebrityFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                if (audiocoll != null && unlockcontent != null && unlock != null && thumbnails != null)
                {
                    audiocoll.IsActive = false;
                    audiocoll.UpdatedDate = DateTime.Now;
                    audiocoll.IsDeleted = true;

                    unlockcontent.IsActive = false;
                    unlockcontent.UpdatedDate = DateTime.Now;
                    unlockcontent.IsDeleted = true;

                    foreach (var unlocks in unlock)
                    {
                        unlocks.IsActive = false;
                        unlocks.UpdatedDate = DateTime.Now;
                        unlocks.IsDeleted = true;
                    }

                    thumbnails.IsActive = false;
                    thumbnails.UpdatedDate = DateTime.Now;
                    thumbnails.IsDeleted = true;

                    result = null;
                }
                else
                {
                    result = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "DeleteCelebrity_Audios");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(result);
        }

        // Bind the user audios

        [Route("binduseraudios/{CelebrityName}/{FollowersName}")]
        [HttpGet("{CelebrityName}/{FollowersName}")]
        public async Task<List<UserAudioDetails>> BindUserAudios(string CelebrityName, string FollowersName)
        {
            List<UserAudioDetails> lockedaudio = (dynamic)null;

            var floginaudio = await GetCelebFollowersDetails(CelebrityName, FollowersName);

            lockedaudio = await (from unlock in _context.NGCOREJWT_UnlockedContent_MFs
                                 join userlog in _context.NGCOREJWT_UserLogins on unlock.CelebrityLoginFKID equals userlog.UserLoginPKID
                                 join fls in _context.NGCOREJWT_FollowersLogins on unlock.FollowersLoginFKID equals fls.FollowersLoginPKID
                                 join price in _context.NGCOREJWT_ContentsPrices on unlock.ContentPriceFKID equals price.ContentPricePKID
                                 join audstore in _context.NGCOREJWT_Store_Thumbnails on unlock.UnlockedContentFKID equals audstore.UnlockedContentFKID
                                 where floginaudio.CelebrityName == CelebrityName && unlock.FollowersLoginFKID == floginaudio.FollowersLoginPKID
                                 && unlock.IsActive == true && price.IsActive == true && unlock.ContentType == ".mp3"
                                 && fls.FollowersName == floginaudio.FollowersName
                                 group unlock by new
                                 {
                                     unlock.ContentCaption,
                                     unlock.ContentDesc,
                                     unlock.FileName,
                                     unlock.IsLocked,
                                     price.ContentPricePKID,
                                     unlock.TotalCharges,
                                     unlock.IconPath,
                                     audstore.ThumbnailImage,
                                     unlock.CreatedDate
                                 }
                                 into unlockaudio
                                 orderby unlockaudio.Key.CreatedDate descending
                                 select new UserAudioDetails()
                                 {
                                     AudioCollectionPKID = 0,
                                     AudioCaption = unlockaudio.Key.ContentCaption,
                                     AudioDesc = unlockaudio.Key.ContentDesc,
                                     Createdate = Convert.ToDateTime(unlockaudio.Key.CreatedDate),
                                     FileName = unlockaudio.Key.FileName,
                                     PriceInfo = unlockaudio.Key.IsLocked == false ? Convert.ToInt32(unlockaudio.Key.TotalCharges) + "/-(Inclusive of all taxes)" : unlockaudio.Key.FileName,
                                     IsLocked = unlockaudio.Key.IsLocked == false ? "Locked" : "UnLocked",
                                     Hardcoded = unlockaudio.Key.IsLocked == false ? "UNLOCK POST @" : "",
                                     AlbumPosterPricePKID = unlockaudio.Key.ContentPricePKID,
                                     IconPath = unlockaudio.Key.IconPath,
                                     ImagePath = unlockaudio.Key.IsLocked == false ? "assets/useraudiothumbnail.png" : string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(unlockaudio.Key.ThumbnailImage))

                                 }).ToListAsync();
            return lockedaudio;
        }

        public async Task<Celebrity_FollowersName> GetCelebFollowersDetails(string celebname, string followersname)
        {
            var firstdef = await (from usr in _context.NGCOREJWT_UserLogins
                                  join flog in _context.NGCOREJWT_FollowersLogins on usr.UserLoginPKID equals flog.UserLoginFKID
                                  where flog.IsActive == true && usr.IsActive == true && flog.IsApproved == true && flog.IsRegistered == true
                                  && flog.FollowersName == followersname && usr.IsActive == true && usr.IsApproved == true && usr.IsRegistered == true
                                  && usr.PersonalityType == "Celebrity" && usr.FullName == celebname
                                  select new Celebrity_FollowersName()
                                  {
                                      CelebrityName = usr.FullName,
                                      FollowersName = flog.FollowersName,
                                      FollowersLoginPKID = flog.FollowersLoginPKID,
                                      CelebrityFKID = usr.UserLoginPKID,
                                      EmailID = flog.EmailID
                                  }).FirstOrDefaultAsync();
            return firstdef;
        }

        // Get Audio Unlocked Content Invoice Details

        [Route("audio_invdetails")]
        [HttpPost]
        public async Task<List<GeneratePDFInvoice_Video>> InvoiceDetails_Audio([FromForm] UnlockedPost_AudioFormdata audiounlockpost)
        {
            var genpdfinvoice_audio = (dynamic)null;
            var checkcelebrity = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == audiounlockpost.CelebrityName && x.IsActive == true
                                 && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

            var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == audiounlockpost.FollowersName
            && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == audiounlockpost.CelebrityName
            && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

            int albumposterpkid = audiounlockpost.AlbumPostersPriceFKID;
            var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkcelebrity.UserLoginPKID
            && f.IsActive == true && f.IsLocked == true && f.ContentCaption == audiounlockpost.ContentCaption && f.ContentPriceFKID == albumposterpkid
            && f.FollowersLoginFKID == findfollower.FollowersLoginPKID).FirstOrDefaultAsync();

            if (updlockedcontent_mfs != null)
            {
                genpdfinvoice_audio = await (from gpinv in _context.NGCOREJWT_GenerateInvoices
                                             where gpinv.IsActive == true && gpinv.UnlockedContent_MF_FKID == updlockedcontent_mfs.UnlockedContent_MF_PKID
                                             select new GeneratePDFInvoice_Video()
                                             {
                                                 FollowerEmailID = findfollower.EmailID,
                                                 InvoiceNo = gpinv.InvoiceNo,
                                                 InvoiceDate = Convert.ToDateTime(gpinv.CreatedDate),
                                                 InvoiceDesc = "Streaming Service",
                                                 Amount = gpinv.Amount,
                                                 GSTCharges = gpinv.GSTCharges,
                                                 ServiceCharges = gpinv.ServiceCharges,
                                                 TotalCharges = gpinv.TotalCharges
                                             }).ToListAsync();

            }
            return genpdfinvoice_audio;
        }

        [Route("audio_alreadyexists/{CelebrityName}/{FollowersEmailID}/{AudioCaption}")]
        [HttpGet("{CelebrityName}/{FollowersEmailID}/{AudioCaption}")]
        public async Task<int> AudioAlreadyExists(string CelebrityName, string FollowersEmailID, string AudioCaption)
        {
            int audiocount = 0;
            var celebfollowerdet = await GetCelebFollowersDetails_UnLock(CelebrityName, FollowersEmailID);
            if (celebfollowerdet == null)
            {
                audiocount = 0;
            }
            else
            {
                audiocount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true &&
                u.ContentCaption == AudioCaption && u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();
            }

            return audiocount;
        }

        // unlock the audio content from the audio list - UnlockAudioPost
        public async Task<ActionResult<UnlockedPost_AudioFormdata>> SaveFollowersData_UnlockAudioFormData(UnlockedPost_AudioFormdata audiounlockpost)
        {
            try
            {
                var checkuser = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == audiounlockpost.CelebrityName && x.IsActive == true
                && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                var followerscount = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == audiounlockpost.FollowersEmailID
                && i.IsActive == true && i.IsApproved == true && i.UserType == "Follower").CountAsync();

                var posterprice = await _context.NGCOREJWT_ContentsPrices.Where(i => i.IsActive == true &&
                i.ContentPricePKID == audiounlockpost.AlbumPostersPriceFKID).FirstOrDefaultAsync();

                if (followerscount == 0)
                {
                    var follower = new IdentityUser
                    {
                        Email = audiounlockpost.FollowersEmailID,
                        UserName = audiounlockpost.FollowersName,
                        // PhoneNumber = videounlockpost.FollowersMobileNumber,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(follower, "Squad$123");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(follower, "Follower");
                        addfolowerscount = 1;

                        var followerlogin = savefollowers.SaveFollowersLogin(audiounlockpost.FollowersName.Trim('"'), audiounlockpost.FollowersEmailID.Trim('"'),
                                            audiounlockpost.FollowersMobileNumber.Trim('"'), checkuser.UserLoginPKID);

                        await _context.NGCOREJWT_FollowersLogins.AddAsync(followerlogin);
                        await _context.SaveChangesAsync();

                        var checkfollower = await GetCelebFollowersDetails_UnLock(audiounlockpost.CelebrityName, audiounlockpost.FollowersEmailID);

                        var unlockedcontents = await _context.NGCOREJWT_UnlockedContents.Where(x => x.IsActive == true && x.CelebrityLoginFKID == checkuser.UserLoginPKID).ToListAsync();

                        foreach (var saveulmfs in unlockedcontents)
                        {
                            NGCOREJWT_UnlockedContent_MF unlockedmfs = new NGCOREJWT_UnlockedContent_MF();
                            unlockedmfs.CelebrityLoginFKID = checkuser.UserLoginPKID;
                            unlockedmfs.FollowersLoginFKID = checkfollower.FollowersLoginPKID;
                            unlockedmfs.UnlockedContentFKID = saveulmfs.UnlockedContentPKID;
                            unlockedmfs.ContentPrice = saveulmfs.ContentPrice;
                            unlockedmfs.ContentPriceFKID = saveulmfs.ContentPriceFKID;
                            unlockedmfs.ContentType = saveulmfs.ContentType;
                            unlockedmfs.ContentCaption = saveulmfs.ContentCaption;
                            unlockedmfs.ContentDesc = saveulmfs.ContentDesc;
                            unlockedmfs.IconPath = saveulmfs.IconPath1;
                            unlockedmfs.ImagePath = saveulmfs.ImagePath1;
                            unlockedmfs.DupFileName = saveulmfs.FileName;
                            unlockedmfs.IsActive = true;
                            unlockedmfs.IsDeleted = false;
                            unlockedmfs.IsLocked = false;
                            unlockedmfs.CreatedBy = "System";
                            unlockedmfs.UpdatedBy = "System";
                            unlockedmfs.CreatedDate = saveulmfs.CreatedDate;
                            unlockedmfs.UpdatedDate = DateTime.Now;
                            unlockedmfs.GSTCharges = saveulmfs.UC_GSTCharges;
                            unlockedmfs.ServiceCharges = saveulmfs.UC_ServiceCharges;
                            unlockedmfs.TotalCharges = saveulmfs.UC_TotalCharges;

                            await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(unlockedmfs);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == audiounlockpost.FollowersName
                && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == audiounlockpost.CelebrityName
                && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkuser.UserLoginPKID
               && f.IsActive == true && f.IsLocked == false && f.ContentCaption == audiounlockpost.ContentCaption
               && f.FollowersLoginFKID == findfollower.FollowersLoginPKID).ToListAsync();

                foreach (var uplc_mfs in updlockedcontent_mfs)
                {
                    uplc_mfs.IsLocked = true;
                    uplc_mfs.CreatedDate = DateTime.Now;
                    uplc_mfs.IconPath = "fa fa-unlock";
                    uplc_mfs.ImagePath = "assets/audiothumbnail.jpg";
                    uplc_mfs.FileName = uplc_mfs.DupFileName;

                    // Increment the UnlockedContent count of audio

                    var unlockedcount = await _context.NGCOREJWT_Store_Thumbnails.Where(i => i.IsActive == true && i.UnlockedContentFKID == uplc_mfs.UnlockedContentFKID
                                        && i.ContentCaptions == uplc_mfs.ContentCaption).FirstOrDefaultAsync();

                    if (unlockedcount != null)
                    {
                        if (unlockedcount.UnlockedContentCount == null)
                        {
                            unlockedcount.UnlockedContentCount = 0;
                        }
                        unlockedcount.UnlockedContentCount += 1;
                        unlockedcount.ContentType = uplc_mfs.ContentType;
                        unlockedcount.FileName = uplc_mfs.FileName;
                    }

                    // Payment Section

                    NGCOREJWT_PaymentSection paysection = new NGCOREJWT_PaymentSection();
                    paysection.Celebrity_FKID = findfollower.UserLoginFKID;
                    paysection.Followers_FKID = findfollower.FollowersLoginPKID;
                    paysection.ContentPrice_FKID = audiounlockpost.AlbumPostersPriceFKID;
                    paysection.UnlockedContentMF_FKID = uplc_mfs.UnlockedContent_MF_PKID;

                    string shares = string.Empty;
                    //decimal bank = 0;
                    //decimal finalbank_share = 0;
                    //shares = uplc_mfs.PostersPrice.Split("/-")[0];
                    //bank = Convert.ToDecimal(shares);
                    //finalbank_share = bank * 2 / 100;
                    paysection.Share_Bank = 0;

                    decimal ours = 0;
                    decimal finalours_share = 0;
                    shares = posterprice.ContentPrice.Split("/-")[0];
                    ours = Convert.ToDecimal(shares);
                    finalours_share = ours * 20 / 100;
                    paysection.Share_Ours = finalours_share;

                    decimal celebrity = 0;
                    decimal finalcelebrity_share = 0;
                    //shares = uplc_mfs.PostersPrice;
                    celebrity = Convert.ToDecimal(shares);
                    finalcelebrity_share = celebrity * 80 / 100;
                    paysection.Share_Celebrities = finalcelebrity_share;

                    paysection.ContentPrice = uplc_mfs.ContentPrice;
                    paysection.IsActive = true;
                    paysection.IsDeleted = false;
                    paysection.CreatedBy = "System";
                    paysection.UpdatedBy = "System";
                    paysection.CreatedDate = DateTime.Now;
                    paysection.UpdatedDate = DateTime.Now;

                    await _context.NGCOREJWT_PaymentSections.AddAsync(paysection);

                    // Generate Invoices

                    NGCOREJWT_GenerateInvoice geninvoice = new NGCOREJWT_GenerateInvoice();
                    geninvoice.CelebrityFKID = checkuser.UserLoginPKID;
                    geninvoice.FollowerFKID = findfollower.FollowersLoginPKID;
                    geninvoice.ContentPriceFKID = audiounlockpost.AlbumPostersPriceFKID;
                    geninvoice.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    geninvoice.FollowerEmailID = findfollower.EmailID;
                    geninvoice.InvoiceNo = "CS-AUD-" + uplc_mfs.UnlockedContent_MF_PKID + "_" + Convert.ToDateTime(DateTime.Now).ToString("ddMMyyyyhhmmss");
                    geninvoice.InvDescription = "Streaming Service";
                    geninvoice.Amount = Convert.ToDecimal(shares);
                    geninvoice.GSTCharges = Convert.ToDecimal(uplc_mfs.GSTCharges);
                    geninvoice.ServiceCharges = Convert.ToDecimal(uplc_mfs.ServiceCharges);
                    geninvoice.TotalCharges = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    geninvoice.IsActive = true;
                    geninvoice.IsDeleted = false;
                    geninvoice.CreatedBy = "System";
                    geninvoice.CreatedDate = DateTime.Now;

                    await _context.NGCOREJWT_GenerateInvoices.AddAsync(geninvoice);

                    // save customer payment details

                    CustPaymentDetail custPayment = new CustPaymentDetail();
                    custPayment.CelebrityFKID = checkuser.UserLoginPKID;
                    custPayment.FollowerFKID = findfollower.FollowersLoginPKID;
                    custPayment.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    custPayment.Amount = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    custPayment.Currency = "INR";
                    custPayment.PaymentCapture = 1;
                    custPayment.RzpayOrderID = audiounlockpost.RzpayOrderID;
                    custPayment.RzpayPaymentID = audiounlockpost.RzpayPaymentID;
                    custPayment.RzpaySignature = audiounlockpost.RzpaySignature;
                    custPayment.PaymentStatus = "Success";
                    custPayment.IsActive = true;
                    custPayment.IsDeleted = false;
                    custPayment.CreatedDate = DateTime.Now;

                    await _context.CustPaymentDetails.AddAsync(custPayment);
                }

                await _context.SaveChangesAsync();
                if (addfolowerscount == 0)
                {
                    await EmailNotification("Dear " + audiounlockpost.CelebrityName + ", your follower named " + audiounlockpost.FollowersName + " has unlocked a audio", audiounlockpost.ContentCaption, " Unlocked your " + audiounlockpost.ContentCaption + " (Post @ " + audiounlockpost.PriceInfo + ")", addfolowerscount, audiounlockpost.CelebrityName);
                }
                else
                {
                    await EmailNotification("Dear " + audiounlockpost.CelebrityName + ", you got a new follower named " + audiounlockpost.FollowersName, audiounlockpost.ContentCaption, " Unlocked your " + audiounlockpost.ContentCaption + " (Post @ " + audiounlockpost.PriceInfo + ")", addfolowerscount, audiounlockpost.CelebrityName);
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockAudioPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }


        // register the user and unlocked the audio post
        [Route("unlockaudiopost")]
        [HttpPost]
        public async Task<JsonResult> UnlockAudioPost([FromForm] UnlockedPost_AudioFormdata audiounlockpost)
        {
            string audio_alertmessage = string.Empty;
            try
            {
                //var celebfollowerdet = await GetCelebFollowersDetails_UnLock(audiounlockpost.CelebrityName, audiounlockpost.FollowersEmailID);

                //if (celebfollowerdet == null)
                //{
                //    // payment gateway

                //    audio_alertmessage = "Payment Gateway";
                //}
                //else
                //{
                //    int audio_alreadyexist = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true && u.ContentCaption == audiounlockpost.ContentCaption
                //    && u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();

                //    if (audio_alreadyexist == 0)
                //    {
                var usercount = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == audiounlockpost.CelebrityName
                      && x.IsActive == true && x.IsRegistered == true && x.PersonalityType == "Celebrity").CountAsync();

                if (usercount == 1)
                {
                    await SaveFollowersData_UnlockAudioFormData(audiounlockpost);
                }
                audio_alertmessage = "Not Exists";
                //    }
                //    else
                //    {
                //        audio_alertmessage = "Already Exists";
                //    }
                //}
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockAudioPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(audio_alertmessage);
        }

        // Unlocked Audio - Followers Unlocked Audio Count for Celebrity View

        [Route("groupbyaudio_count/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<ActionResult<IEnumerable<UnlockAudioCount>>> GroupbyAudio_Celebrity(string FullName)
        {
            var audiocount = await (from astore in _context.NGCOREJWT_Store_Thumbnails
                                    join usrlogin in _context.NGCOREJWT_UserLogins on astore.CelebrityFKID equals usrlogin.UserLoginPKID
                                    join price in _context.NGCOREJWT_ContentsPrices on astore.ContentPriceFKID equals price.ContentPricePKID
                                    where astore.IsActive == true && usrlogin.FullName == FullName && astore.ContentType == ".mp3"
                                    && usrlogin.IsApproved == true && usrlogin.IsRegistered == true && usrlogin.PersonalityType == "Celebrity"
                                    orderby astore.CreatedDate descending
                                    select new UnlockAudioCount()
                                    {
                                        AudioCount = astore.UnlockedContentCount,
                                        AudioCaption = astore.ContentCaptions,
                                        FileName = astore.FileName,
                                        AudioThumbnail = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(astore.ThumbnailImage)),
                                        AudioPriceInfo = price.ContentPrice,
                                        ALikedCount = astore.UC_LikedCount == null ? 0 : astore.UC_LikedCount
                                    }).ToListAsync();

            return audiocount;
        }

        // Get the audio liked count - GetUC_AudioLikedCount

        [Route("getucaudio_likedcount/{CelebrityName}/{FollowersName}/{AudioCaptions}/{PosterPriceFKID}")]
        [HttpGet("{CelebrityName}/{FollowersName}/{AudioCaptions}/{PosterPriceFKID}")]
        public async Task<List<GetAudio_LikedCount>> GetUC_AudioLikedCount(string CelebrityName, string FollowersName,
                                                                          string AudioCaptions, int PosterPriceFKID)
        {
            var getlikedaudio = (dynamic)null;
            try
            {
                var chkceluser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.FollowersName == FollowersName
                && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.IsActive == true && j.IsApproved == true && j.IsRegistered == true
                && j.FullName == CelebrityName && j.PersonalityType == "Celebrity").Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                int likedcount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == AudioCaptions
                && j.ContentPriceFKID == PosterPriceFKID && j.IsLocked == true && j.IsLiked == true).CountAsync();

                getlikedaudio = await (from ucmfs in _context.NGCOREJWT_UnlockedContent_MFs
                                       where ucmfs.IsActive == true && ucmfs.IsLocked == true && ucmfs.ContentCaption == AudioCaptions
                                       && ucmfs.ContentPriceFKID == PosterPriceFKID && ucmfs.FollowersLoginFKID == chkceluser.FollowersLoginPKID
                                       && ucmfs.CelebrityLoginFKID == chkceluser.UserLoginFKID
                                       select new GetAudio_LikedCount()
                                       {
                                           AIsLiked = ucmfs.IsLiked == null ? false : true,
                                           UC_ALikedCount = likedcount
                                       }).ToListAsync();

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockedContent_Get_Audio_LikedCount");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getlikedaudio;
        }

        // Update the audio liked count - UpdateUC_AudioLikedCount

        [Route("updateucaudio_likedcount")]
        [HttpPut]
        public async Task<List<GetAudio_LikedCount>> UpdateUC_AudioLikedCount([FromForm] Update_UC_LikedAudioCount upducaudlikedcount)
        {
            var getlikedaudio = (dynamic)null;
            try
            {
                var chkceluser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.FollowersName == upducaudlikedcount.FollowersName
                && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.IsActive == true && j.IsApproved == true && j.IsRegistered == true
                && j.FullName == upducaudlikedcount.CelebrityName && j.PersonalityType == "Celebrity").Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                var upducmfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == upducaudlikedcount.AudioCaptions
                && j.ContentPriceFKID == upducaudlikedcount.PosterPriceFKID && j.IsLocked == true && j.CelebrityLoginFKID == chkceluser.UserLoginFKID
                && j.FollowersLoginFKID == chkceluser.FollowersLoginPKID && j.IsLiked == null).FirstOrDefaultAsync();

                upducmfs.IsLiked = true;
                upducmfs.UpdatedDate = DateTime.Now;

                var store = await _context.NGCOREJWT_Store_Thumbnails.Where(k => k.IsActive == true && k.ContentCaptions == upducaudlikedcount.AudioCaptions
                && k.ContentPriceFKID == upducaudlikedcount.PosterPriceFKID && k.CelebrityFKID == chkceluser.UserLoginFKID).FirstOrDefaultAsync();

                store.UpdatedDate = DateTime.Now;
                if (store.UC_LikedCount == null)
                {
                    store.UC_LikedCount = 0;
                }
                store.UC_LikedCount += 1;

                await _context.SaveChangesAsync();

                int a_likedcount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == upducaudlikedcount.AudioCaptions
                && j.ContentPriceFKID == upducaudlikedcount.PosterPriceFKID && j.IsLocked == true && j.IsLiked == true).CountAsync();

                getlikedaudio = await (from ucmfs in _context.NGCOREJWT_UnlockedContent_MFs
                                       where ucmfs.IsActive == true && ucmfs.IsLocked == true && ucmfs.ContentCaption == upducaudlikedcount.AudioCaptions
                                       && ucmfs.ContentPriceFKID == upducaudlikedcount.PosterPriceFKID && ucmfs.FollowersLoginFKID == chkceluser.FollowersLoginPKID
                                       && ucmfs.CelebrityLoginFKID == chkceluser.UserLoginFKID && ucmfs.IsLiked == true
                                       select new GetAudio_LikedCount()
                                       {
                                           AIsLiked = ucmfs.IsLiked,
                                           UC_ALikedCount = a_likedcount
                                       }).ToListAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockedContent_Update_Audio_LikedCount");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getlikedaudio;
        }

        // Insert the audio comments

        [Route("postaudiocomments")]
        [HttpPost]
        public async Task<IActionResult> SuggestComments_Audio([FromForm] AudioSuggestComments asuggcomments)
        {
            // var videosuggcomments = (dynamic)null;
            try
            {
                var chkusers = await GetCelebFollowersDetails(asuggcomments.CelebrityName, asuggcomments.Followersname);

                var contentmfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == chkusers.CelebrityFKID
                && f.IsActive == true && f.IsLocked == true && f.ContentCaption == asuggcomments.AudioCaption &&
                f.FollowersLoginFKID == chkusers.FollowersLoginPKID && f.FileName == asuggcomments.AudioFileName).FirstOrDefaultAsync();

                if (contentmfs != null)
                {
                    NGCOREJWT_SuggestComment suggestcomment = new NGCOREJWT_SuggestComment();
                    suggestcomment.CelebrityFKID = contentmfs.CelebrityLoginFKID;
                    suggestcomment.FollowerFKID = contentmfs.FollowersLoginFKID;
                    suggestcomment.UnlockedContent_MF_FKID = contentmfs.UnlockedContentFKID;
                    suggestcomment.Comments = asuggcomments.AudioComments;
                    suggestcomment.IsActive = true;
                    suggestcomment.IsDeleted = false;
                    suggestcomment.CreatedBy = "System";
                    suggestcomment.CreatedDate = DateTime.Now;

                    await _context.NGCOREJWT_SuggestComments.AddAsync(suggestcomment);
                    await _context.SaveChangesAsync();

                    //videosuggcomments = await (from suggcmts in _context.NGCOREJWT_SuggestComments
                    //                           where suggcmts.IsActive == true && suggcmts.UnlockedContent_MF_FKID == contentmfs.UnlockedContent_MF_PKID
                    //                           select new GetVideoSuggestComments()
                    //                           {
                    //                               vsubscribername = vsuggcomments.Followersname,
                    //                               vcomments = suggcmts.Comments,
                    //                               vcommentdate = Convert.ToDateTime(suggcmts.CreatedDate)
                    //                           }).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Audio_InsertUserComments");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Get the video comments - GetAudioComments

        [Route("getaudiocomments/{CelebrityName}/{FollowersName}/{Acaptions}/{Afilename}")]
        [HttpGet("{CelebrityName}/{FollowersName}/{Acaptions}/{Afilename}")]
        public async Task<List<GetAudioSuggestComments>> GetAudioComments(string CelebrityName, string FollowersName,
                                                                          string Acaptions, string Afilename)
        {
            var getacomments = (dynamic)null;
            try
            {
                var chkusers = await GetCelebFollowersDetails(CelebrityName, FollowersName);

                var getvideocmts = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == Acaptions
                && j.FileName == Afilename && j.IsLocked == true && j.CelebrityLoginFKID == chkusers.CelebrityFKID).FirstOrDefaultAsync();

                getacomments = await (from scmts in _context.NGCOREJWT_SuggestComments
                                      join flog in _context.NGCOREJWT_FollowersLogins on scmts.FollowerFKID equals flog.FollowersLoginPKID
                                      join unlockmfs in _context.NGCOREJWT_UnlockedContents on scmts.UnlockedContent_MF_FKID equals unlockmfs.UnlockedContentPKID
                                      where scmts.UnlockedContent_MF_FKID == getvideocmts.UnlockedContentFKID
                                      select new GetAudioSuggestComments()
                                      {
                                          asubscribername = flog.FollowersName,
                                          acomments = scmts.Comments,
                                          acommentdate = Convert.ToDateTime(scmts.CreatedDate)
                                      }).ToListAsync();

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Audio_UserComments");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getacomments;
        }

        // Video Controller related methods

        // Upload the video to the project folder

        [Route("insertvideocollection")]
        [HttpPost]
        public async Task<ActionResult<NGCOREJWT_VideoCollection>> UploadVideo([FromForm] VideoFormData video)
        {
            try
            {
                if (video != null && video.VideoFileName.Length > 0)
                {
                    // string foldername = "VideoCollections";
                    //  string userfolder = video.UserName;

                    string newpath = Path.Combine("VideoCollections");
                    // foreach (var file in video.FileName)
                    //  {
                    var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, video.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));

                    if (!directoryinfo.Exists)
                    {
                        directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, video.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));
                    }
                    var videofile = Path.GetFileName(video.VideoFileName.FileName);

                    var thumbnail = Path.GetFileName(video.Thumbnail.FileName);

                    if (System.IO.File.Exists(videofile) || System.IO.File.Exists(thumbnail))
                    {
                        System.IO.File.Delete(videofile);
                        System.IO.File.Delete(thumbnail);
                    }

                    var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                    //  var finalthumbpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                    var pathToSave = Path.Combine(finalpath, videofile);

                    var savethumbpath = Path.Combine(finalpath, thumbnail);

                    using (var stream = new FileStream(pathToSave, FileMode.Create))
                    {
                        await video.VideoFileName.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(savethumbpath, FileMode.Create))
                    {
                        await video.Thumbnail.CopyToAsync(stream);
                    }

                    var getloginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == video.UserName && i.IsActive == true &&
                    i.IsRegistered == true).FirstOrDefaultAsync();

                    if (getloginid != null)
                    {
                        NGCOREJWT_VideoCollection videos = new NGCOREJWT_VideoCollection();
                        videos.UserLoginFKID = getloginid.UserLoginPKID;
                        videos.ContentPriceFKID = video.AlbumPosterPriceFKID;
                        videos.FileName = videofile.Trim('"');
                        //  videos.VideoThumbnail = //video.Thumbnail;
                        videos.ContentType = Path.GetExtension(videofile.Trim('"'));
                        //MemoryStream ms = new MemoryStream();
                        //await video.Thumbnail.CopyToAsync(ms);
                        //videos.VideoData = ms.ToArray();
                        videos.VideoCaption = video.VideoCaption.Trim('"');
                        videos.VideoDesc = video.VideoDesc.Trim('"');
                        //  videos.VideoThumbnail = thumbnail.Trim('"');
                        //DirectoryInfo di = new DirectoryInfo(pathToSave);
                        //FileInfo[] fileinfo = di.GetFiles();
                        //videos.VideoSize = fileinfo.Length;
                        videos.Video_GSTCharges = video.V_GSTCharges;
                        videos.Video_ServiceCharges = video.V_ServiceCharges;
                        videos.Video_TotalCharges = video.V_TotalCharges;

                        videos.IsActive = true;
                        videos.IsDeleted = false;
                        videos.IsLocked = false;
                        videos.CreatedBy = "System";
                        videos.UpdatedBy = "System";
                        videos.CreatedDate = DateTime.Now;
                        videos.UpdatedDate = DateTime.Now;

                        _context.NGCOREJWT_VideoCollections.Add(videos);

                        NGCOREJWT_UnlockedContent unlockvideo = new NGCOREJWT_UnlockedContent();
                        unlockvideo.CelebrityLoginFKID = getloginid.UserLoginPKID;
                        unlockvideo.ContentPriceFKID = video.AlbumPosterPriceFKID;
                        unlockvideo.ContentType = Path.GetExtension(videofile.Trim('"'));
                        unlockvideo.ContentCaption = video.VideoCaption.Trim('"');
                        unlockvideo.FileName = videofile.Trim('"');
                        unlockvideo.ContentDesc = video.VideoDesc.Trim('"');
                        unlockvideo.IconPath = "fa fa-lock";
                        unlockvideo.ImagePath = "assets/placeholder-video.png";
                        unlockvideo.ContentPrice = video.PostersPrice.Trim('"');
                        unlockvideo.IconPath1 = "fa fa-lock";
                        unlockvideo.ImagePath1 = "assets/placeholder-video.png";
                        unlockvideo.IsActive = true;
                        unlockvideo.IsDeleted = false;
                        unlockvideo.IsLocked = false;
                        unlockvideo.CreatedBy = "System";
                        unlockvideo.UpdatedBy = "System";
                        unlockvideo.CreatedDate = DateTime.Now;
                        unlockvideo.UpdatedDate = DateTime.Now;
                        unlockvideo.UC_GSTCharges = video.V_GSTCharges;
                        unlockvideo.UC_ServiceCharges = video.V_ServiceCharges;
                        unlockvideo.UC_TotalCharges = video.V_TotalCharges;
                        //MemoryStream videoms = new MemoryStream();
                        //await video.Thumbnail.CopyToAsync(videoms);
                        //unlockvideo.UC_ThumbnailImage = videoms.ToArray();

                        _context.NGCOREJWT_UnlockedContents.Add(unlockvideo);
                        await _context.SaveChangesAsync();

                        var contentid = await _context.NGCOREJWT_UnlockedContents.Where(i => i.ContentCaption == video.VideoCaption &&
                       i.FileName == videofile.Trim('"') && i.ContentPrice == video.PostersPrice && i.ContentType == Path.GetExtension(videofile.Trim('"'))
                       && i.ContentPriceFKID == video.AlbumPosterPriceFKID).FirstOrDefaultAsync();

                        var getfollowers = await _context.NGCOREJWT_FollowersLogins.Where(i => i.UserLoginFKID == getloginid.UserLoginPKID && i.IsActive == true
                         && i.IsRegistered == true && i.IsApproved == true).ToListAsync();

                        if (getfollowers == null || getfollowers.Count == 0)
                        {
                            NGCOREJWT_Store_Thumbnail storevideothumb = new NGCOREJWT_Store_Thumbnail();
                            MemoryStream vidthumb = new MemoryStream();
                            await video.Thumbnail.CopyToAsync(vidthumb);
                            storevideothumb.ThumbnailImage = vidthumb.ToArray();
                            storevideothumb.ThumbnailPath = video.Thumbnail.FileName.Trim('"');
                            storevideothumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                            storevideothumb.CelebrityFKID = getloginid.UserLoginPKID;
                            storevideothumb.ContentCaptions = video.VideoCaption;
                            storevideothumb.UnlockedContentCount = 0;
                            storevideothumb.ContentType = contentid.ContentType;
                            storevideothumb.FileName = videofile.Trim('"');
                            storevideothumb.ContentPriceFKID = video.AlbumPosterPriceFKID;
                            storevideothumb.IsActive = true;
                            storevideothumb.IsDeleted = false;
                            storevideothumb.CreatedBy = "System";
                            storevideothumb.CreatedDate = DateTime.Now;
                            storevideothumb.UpdatedBy = "System";
                            storevideothumb.UpdatedDate = DateTime.Now;

                            await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storevideothumb);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {

                            int vidcount = 0;
                            foreach (var insertnewvideo in getfollowers)
                            {
                                NGCOREJWT_UnlockedContent_MF insertvideos = new NGCOREJWT_UnlockedContent_MF();
                                insertvideos.CelebrityLoginFKID = getloginid.UserLoginPKID;
                                insertvideos.FollowersLoginFKID = insertnewvideo.FollowersLoginPKID;
                                insertvideos.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                insertvideos.ContentPriceFKID = video.AlbumPosterPriceFKID;
                                insertvideos.ContentPrice = video.PostersPrice;
                                insertvideos.ContentType = contentid.ContentType;
                                insertvideos.ContentCaption = video.VideoCaption;
                                insertvideos.ContentDesc = video.VideoDesc;
                                insertvideos.IconPath = "fa fa-lock";
                                insertvideos.ImagePath = "assets/placeholder-video.png";
                                insertvideos.FileName = null; ;
                                insertvideos.DupFileName = videofile.Trim('"');
                                //  insertvideos.Thumbnail = thumbnail.Trim('"');
                                insertvideos.IsActive = true;
                                insertvideos.IsDeleted = false;
                                insertvideos.IsLocked = false;
                                insertvideos.CreatedBy = "System";
                                insertvideos.CreatedDate = DateTime.Now;
                                insertvideos.UpdatedBy = "System";
                                insertvideos.UpdatedDate = DateTime.Now;
                                insertvideos.GSTCharges = video.V_GSTCharges;
                                insertvideos.ServiceCharges = video.V_ServiceCharges;
                                insertvideos.TotalCharges = video.V_TotalCharges;
                                //MemoryStream videomf = new MemoryStream();
                                //await video.Thumbnail.CopyToAsync(videomf);
                                //insertvideos.UC_ThumbnailImage_MF = videomf.ToArray();

                                await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(insertvideos);
                                // await _context.SaveChangesAsync();
                                vidcount += 1;
                                if (vidcount == 1)
                                {
                                    NGCOREJWT_Store_Thumbnail storevideothumb = new NGCOREJWT_Store_Thumbnail();
                                    MemoryStream vidthumb = new MemoryStream();
                                    await video.Thumbnail.CopyToAsync(vidthumb);
                                    storevideothumb.ThumbnailImage = vidthumb.ToArray();
                                    storevideothumb.ThumbnailPath = video.Thumbnail.FileName.Trim('"');
                                    storevideothumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                    storevideothumb.CelebrityFKID = getloginid.UserLoginPKID;
                                    storevideothumb.ContentCaptions = video.VideoCaption;
                                    storevideothumb.UnlockedContentCount = 0;
                                    storevideothumb.ContentType = contentid.ContentType;
                                    storevideothumb.FileName = videofile.Trim('"');
                                    storevideothumb.ContentPriceFKID = video.AlbumPosterPriceFKID;
                                    storevideothumb.IsActive = true;
                                    storevideothumb.IsDeleted = false;
                                    storevideothumb.CreatedBy = "System";
                                    storevideothumb.CreatedDate = DateTime.Now;
                                    storevideothumb.UpdatedBy = "System";
                                    storevideothumb.UpdatedDate = DateTime.Now;

                                    await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storevideothumb);
                                    await _context.SaveChangesAsync();
                                }

                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    await EmailNotification(video.UserName, video.VideoCaption, "New Video Uploaded", 2, "");
                    //  }
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UploadVideo_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        [Route("loadvideodetails/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<ActionResult<IEnumerable<VideoDetails>>> GroupbyVideoCollections(string FullName)
        {
            //var loginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == FullName && i.IsActive == true
            //              && i.IsRegistered == true).FirstOrDefaultAsync();

            var groupbyvideo = await (from video in _context.NGCOREJWT_VideoCollections
                                      join usrlogin in _context.NGCOREJWT_UserLogins on video.UserLoginFKID equals usrlogin.UserLoginPKID
                                      join price in _context.NGCOREJWT_ContentsPrices on video.ContentPriceFKID equals price.ContentPricePKID
                                      where video.UserLoginFKID == usrlogin.UserLoginPKID && video.IsActive == true && usrlogin.FullName == FullName
                                      && price.IsActive == true && usrlogin.IsActive == true && usrlogin.IsRegistered == true &&
                                      usrlogin.PersonalityType == "Celebrity" && usrlogin.IsApproved == true
                                      orderby video.CreatedDate descending
                                      select new VideoDetails()
                                      {
                                          VideoCaption = video.VideoCaption,
                                          VideoDesc = video.VideoDesc,
                                          FileName = video.FileName,
                                          Createdate = Convert.ToDateTime(video.CreatedDate),
                                          PriceInfo = video.Video_TotalCharges,
                                          IsLocked = video.IsLocked == false ? "Locked" : "",
                                          AlbumPosterPricePKID = price.ContentPricePKID,
                                          VideoCollectionPKID = video.VideoCollectionPKID,
                                          VideoThumbnail = video.VideoData == null ? "assets/placeholder-video.png" : null
                                          //video.VideoData == null ? string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(video.VideoData)) : "assets/placeholder-video.png"
                                      }).ToListAsync();

            return groupbyvideo;
        }

        // Bind the user videos

        [Route("binduservideos/{CelebrityName}/{FollowersName}")]
        [HttpGet("{CelebrityName}/{FollowersName}")]
        public async Task<List<UserVideoDetails>> BindUserVideos_AfterLogin(string CelebrityName, string FollowersName)
        {
            List<UserVideoDetails> lockedvideo = (dynamic)null;

            var flogin = await GetCelebFollowersDetails(CelebrityName, FollowersName);

            lockedvideo = await (from unlock in _context.NGCOREJWT_UnlockedContent_MFs
                                 join userlog in _context.NGCOREJWT_UserLogins on unlock.CelebrityLoginFKID equals userlog.UserLoginPKID
                                 join fls in _context.NGCOREJWT_FollowersLogins on unlock.FollowersLoginFKID equals fls.FollowersLoginPKID
                                 join price in _context.NGCOREJWT_ContentsPrices on unlock.ContentPriceFKID equals price.ContentPricePKID
                                 join vidstore in _context.NGCOREJWT_Store_Thumbnails on unlock.UnlockedContentFKID equals vidstore.UnlockedContentFKID
                                 where flogin.CelebrityName == CelebrityName && unlock.FollowersLoginFKID == flogin.FollowersLoginPKID
                                 && unlock.IsActive == true && price.IsActive == true && unlock.ContentType == ".mp4" || unlock.ContentType == ".avi"
                                 && fls.FollowersName == flogin.FollowersName
                                 group unlock by new
                                 {
                                     unlock.ContentCaption,
                                     unlock.ContentDesc,
                                     unlock.FileName,
                                     unlock.IsLocked,
                                     price.ContentPricePKID,
                                     unlock.TotalCharges,
                                     unlock.IconPath,
                                     vidstore.ThumbnailImage,
                                     unlock.CreatedDate
                                 }
                                 into unlockvideo
                                 orderby unlockvideo.Key.CreatedDate descending
                                 select new UserVideoDetails()
                                 {
                                     VideoCollectionPKID = 0,
                                     VideoCaption = unlockvideo.Key.ContentCaption,
                                     VideoDesc = unlockvideo.Key.ContentDesc,
                                     Createdate = Convert.ToDateTime(unlockvideo.Key.CreatedDate),
                                     FileName = unlockvideo.Key.FileName,
                                     PriceInfo = unlockvideo.Key.IsLocked == false ? Convert.ToInt32(unlockvideo.Key.TotalCharges) + "/- (Inclusive of all taxes)" : unlockvideo.Key.FileName,
                                     IsLocked = unlockvideo.Key.IsLocked == false ? "Locked" : "UnLocked",
                                     Hardcoded = unlockvideo.Key.IsLocked == false ? "UNLOCK POST @" : "",
                                     AlbumPosterPricePKID = unlockvideo.Key.ContentPricePKID,
                                     IconPath = unlockvideo.Key.IconPath,
                                     ImagePath = unlockvideo.Key.IsLocked == false ? "assets/placeholder-video.png" : string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(unlockvideo.Key.ThumbnailImage))

                                 }).ToListAsync();
            return lockedvideo;
        }


        // Unlocked Video - Followers Unlocked Video Count for Celebrity View

        [Route("groupbyvideo_count/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<ActionResult<IEnumerable<UnlockVideoCount>>> GroupbyVideo_Celebrity(string FullName)
        {
            var videocount = await (from vstore in _context.NGCOREJWT_Store_Thumbnails
                                    join usrlogin in _context.NGCOREJWT_UserLogins on vstore.CelebrityFKID equals usrlogin.UserLoginPKID
                                    join price in _context.NGCOREJWT_ContentsPrices on vstore.ContentPriceFKID equals price.ContentPricePKID
                                    where vstore.IsActive == true && usrlogin.FullName == FullName && vstore.ContentType == ".mp4"
                                    && usrlogin.IsApproved == true && usrlogin.IsRegistered == true && usrlogin.PersonalityType == "Celebrity"
                                    orderby vstore.CreatedDate descending
                                    select new UnlockVideoCount()
                                    {
                                        VideoCount = vstore.UnlockedContentCount,
                                        VideoCaption = vstore.ContentCaptions,
                                        FileName = vstore.FileName,
                                        VideoThumbnail = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(vstore.ThumbnailImage)),
                                        VideoPriceInfo = price.ContentPrice,
                                        VLikedCount = vstore.UC_LikedCount == null ? 0 : vstore.UC_LikedCount
                                    }).ToListAsync();

            return videocount;
        }

        // video modal popup

        [Route("getvideodet_popup/{videopostpricepkid}/{videocollectionpkid}/{videoscaption}")]
        [HttpGet("{videopostpricepkid}/{videocollectionpkid}/{videoscaption}")]
        public async Task<List<GetVideoDetails_Popup>> GetVideoDetails_Popup(int videopostpricepkid, int videocollectionpkid, string videoscaption)
        {
            var videodetpopup = (dynamic)null;
            var getserviceid = await _context.NGCOREJWT_ContentsPrices.Where(i => i.ContentPricePKID == videopostpricepkid && i.IsActive == true).FirstOrDefaultAsync();

            if (getserviceid != null)
            {
                if (videocollectionpkid == 0)
                {
                    videodetpopup = await (from videocall in _context.NGCOREJWT_VideoCollections
                                           join albumprice in _context.NGCOREJWT_ContentsPrices on videocall.ContentPriceFKID equals albumprice.ContentPricePKID
                                           where videocall.ContentPriceFKID == getserviceid.ContentPricePKID && videocall.IsActive == true
                                           && videocall.VideoCaption == videoscaption
                                           select new GetVideoDetails_Popup()
                                           {
                                               VideoCaption = videocall.VideoCaption,
                                               PriceInfo = Convert.ToInt32(videocall.Video_TotalCharges) * 100 / 100 + "/-",
                                               VideoCollectionPKID = videocall.VideoCollectionPKID,
                                               AlbumPosterPricePKID = albumprice.ContentPricePKID
                                           }).ToListAsync();
                }
                else
                {
                    videodetpopup = await (from videocall in _context.NGCOREJWT_UnlockedContents
                                           join albumprice in _context.NGCOREJWT_ContentsPrices on videocall.ContentPriceFKID equals albumprice.ContentPricePKID
                                           where videocall.ContentPriceFKID == getserviceid.ContentPricePKID && videocall.IsActive == true
                                           && videocall.ContentCollectionFKID == videocollectionpkid || videocall.ContentCaption == videoscaption
                                           select new GetVideoDetails_Popup()
                                           {
                                               VideoCaption = videocall.ContentCaption,
                                               PriceInfo = Convert.ToInt32(videocall.UC_TotalCharges) * 100 / 100 + "/-",
                                               VideoCollectionPKID = videocall.ContentCollectionFKID,
                                               AlbumPosterPricePKID = albumprice.ContentPricePKID
                                           }).ToListAsync();
                }

            }

            return videodetpopup;
        }

        // Update the Video caption and price - UpdateVideoCap_Price

        [Route("updatevideocap_price")]
        [HttpPut]
        public async Task<ActionResult<Update_VideoCap_Price_Celeb>> UpdateVideoCap_Price([FromForm] Update_VideoCap_Price_Celeb updatevcap_price)
        {
            try
            {
                string resultmsg = string.Empty;

                var celebrityid = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == updatevcap_price.CelebrityName && x.PersonalityType == "Celebrity" &&
                                  x.IsActive == true && x.IsRegistered == true && x.IsApproved == true).FirstOrDefaultAsync();

                var upd_videocoll = await _context.NGCOREJWT_VideoCollections.Where(v => v.IsActive == true && v.FileName == updatevcap_price.VideoFileName &&
                                v.UserLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == updatevcap_price.VideoFileName &&
                                    v.CelebrityLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == updatevcap_price.VideoFileName &&
                             v.CelebrityLoginFKID == celebrityid.UserLoginPKID).ToListAsync();

                var upd_thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == updatevcap_price.VideoFileName &&
                                 v.CelebrityFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                if (upd_videocoll != null && upd_unlock != null && upd_unlock_mfs != null && upd_thumbnails != null)
                {
                    upd_videocoll.Video_GSTCharges = updatevcap_price.Video_GSTCharges;
                    upd_videocoll.Video_ServiceCharges = updatevcap_price.Video_ServiceCharges;
                    upd_videocoll.Video_TotalCharges = updatevcap_price.Video_TotalCharges;
                    upd_videocoll.ContentPriceFKID = updatevcap_price.PosterPriceFKID;
                    upd_videocoll.VideoCaption = updatevcap_price.VideoCaption;
                    upd_videocoll.UpdatedDate = DateTime.Now;

                    upd_unlock.UC_GSTCharges = updatevcap_price.Video_GSTCharges;
                    upd_unlock.UC_ServiceCharges = updatevcap_price.Video_ServiceCharges;
                    upd_unlock.UC_TotalCharges = updatevcap_price.Video_TotalCharges;
                    upd_unlock.ContentPriceFKID = updatevcap_price.PosterPriceFKID;
                    upd_unlock.ContentPrice = updatevcap_price.PriceInfo;
                    upd_unlock.ContentCaption = updatevcap_price.VideoCaption;
                    upd_unlock.UpdatedDate = DateTime.Now;

                    foreach (var unlock_mfs in upd_unlock_mfs)
                    {
                        unlock_mfs.ContentPriceFKID = updatevcap_price.PosterPriceFKID;
                        unlock_mfs.ContentPrice = updatevcap_price.PriceInfo;
                        unlock_mfs.ContentCaption = updatevcap_price.VideoCaption;
                        unlock_mfs.UpdatedDate = DateTime.Now;

                        unlock_mfs.GSTCharges = updatevcap_price.Video_GSTCharges;
                        unlock_mfs.ServiceCharges = updatevcap_price.Video_ServiceCharges;
                        unlock_mfs.TotalCharges = updatevcap_price.Video_TotalCharges;
                    }

                    upd_thumbnails.ContentPriceFKID = updatevcap_price.PosterPriceFKID;
                    upd_thumbnails.ContentCaptions = updatevcap_price.VideoCaption;
                    upd_thumbnails.UpdatedDate = DateTime.Now;

                    resultmsg = null;
                }
                else
                {
                    resultmsg = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UpdateCelebrity_Video_Caption_Price");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Sync the Video caption and price from the Video Collection table - EditVideoCap_Price_Artist

        [Route("bindvideodetails_celeb/{celebrityname}/{editvideofile}")]
        [HttpGet("{celebrityname}/{editvideofile}")]
        public async Task<Edit_VideoCap_Price_Celeb> GetCelebVideoDetails(string celebrityname, string editvideofile)
        {
            var videodet = await (from video in _context.NGCOREJWT_VideoCollections
                                  join usr in _context.NGCOREJWT_UserLogins on video.UserLoginFKID equals usr.UserLoginPKID
                                  join price in _context.NGCOREJWT_ContentsPrices on video.ContentPriceFKID equals price.ContentPricePKID
                                  where price.IsActive == true && usr.FullName == celebrityname && usr.IsActive == true
                                  && video.IsActive == true && video.FileName == editvideofile
                                  select new Edit_VideoCap_Price_Celeb()
                                  {
                                      VideoCaption = video.VideoCaption,
                                      PosterPriceFKID = video.ContentPriceFKID,
                                      PostersPrice = price.ContentPrice
                                  }).FirstOrDefaultAsync();
            return videodet;
        }


        // Video Deletion by the Celebrity

        [Route("deletevideos/{celebrityname}/{videocaptions}/{videofilename}")]
        [HttpDelete("{celebrityname}/{videocaptions}/{videofilename}")]
        public async Task<JsonResult> DeleteVideos(string celebrityname, string videocaptions, string videofilename)
        {
            string result = string.Empty;
            try
            {
                var celebid = await _context.NGCOREJWT_UserLogins.Where(i => i.IsActive == true && i.IsRegistered == true && i.IsApproved == true &&
                              i.PersonalityType == "Celebrity" && i.FullName == celebrityname).FirstOrDefaultAsync();

                var videocoll = await _context.NGCOREJWT_VideoCollections.Where(v => v.IsActive == true && v.FileName == videofilename &&
                               v.VideoCaption == videocaptions && v.UserLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlockcontent = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == videofilename &&
                              v.ContentCaption == videocaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlock = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == videofilename &&
                              v.ContentCaption == videocaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).ToListAsync();

                var thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == videofilename &&
                             v.ContentCaptions == videocaptions && v.CelebrityFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                if (videocoll != null && unlockcontent != null && unlock != null && thumbnails != null)
                {
                    videocoll.IsActive = false;
                    videocoll.UpdatedDate = DateTime.Now;
                    videocoll.IsDeleted = true;

                    unlockcontent.IsActive = false;
                    unlockcontent.UpdatedDate = DateTime.Now;
                    unlockcontent.IsDeleted = true;

                    foreach (var unlocks in unlock)
                    {
                        unlocks.IsActive = false;
                        unlocks.UpdatedDate = DateTime.Now;
                        unlocks.IsDeleted = true;
                    }

                    thumbnails.IsActive = false;
                    thumbnails.UpdatedDate = DateTime.Now;
                    thumbnails.IsDeleted = true;

                    result = null;
                }
                else
                {
                    result = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "DeleteCelebrity_Videos");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(result);
        }

        // Get Video Unlocked Content Invoice Details

        [Route("video_invdetails")]
        [HttpPost]
        public async Task<List<GeneratePDFInvoice_Video>> InvoiceDetails_Video([FromForm] UnlockedPost_VideoFormdata videounlock)
        {
            var genpdfinvoice_video = (dynamic)null;
            var checkcelebrity = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == videounlock.CelebrityName && x.IsActive == true
                                 && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

            var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == videounlock.FollowersName
            && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == videounlock.CelebrityName
            && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

            int albumposterpkid = videounlock.AlbumPostersPriceFKID;
            var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkcelebrity.UserLoginPKID
            && f.IsActive == true && f.IsLocked == true && f.ContentCaption == videounlock.ContentCaption && f.ContentPriceFKID == albumposterpkid &&
            f.FollowersLoginFKID == findfollower.FollowersLoginPKID).FirstOrDefaultAsync();

            if (updlockedcontent_mfs != null)
            {
                genpdfinvoice_video = await (from gpinv in _context.NGCOREJWT_GenerateInvoices
                                             where gpinv.IsActive == true && gpinv.UnlockedContent_MF_FKID == updlockedcontent_mfs.UnlockedContent_MF_PKID
                                             select new GeneratePDFInvoice_Video()
                                             {
                                                 FollowerEmailID = findfollower.EmailID,
                                                 InvoiceNo = gpinv.InvoiceNo,
                                                 InvoiceDate = Convert.ToDateTime(gpinv.CreatedDate),
                                                 InvoiceDesc = "Streaming Service",
                                                 Amount = gpinv.Amount,
                                                 GSTCharges = gpinv.GSTCharges,
                                                 ServiceCharges = gpinv.ServiceCharges,
                                                 TotalCharges = gpinv.TotalCharges
                                             }).ToListAsync();

            }
            return genpdfinvoice_video;
        }

        // unlock to view the video content from the list - VideoUnlockPost
        public async Task<ActionResult<UnlockedPost_VideoFormdata>> SaveFollowersData_UnlockVideoFormData(UnlockedPost_VideoFormdata videounlockpost)
        {
            try
            {
                var checkuser = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == videounlockpost.CelebrityName && x.IsActive == true
                && x.PersonalityType == "Celebrity" && x.IsApproved == true).FirstOrDefaultAsync();

                var followerscount = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.IsApproved == true &&
                i.UserType == "Follower" && i.EmailID == videounlockpost.FollowersEmailID).CountAsync();

                var posterprice = await _context.NGCOREJWT_ContentsPrices.Where(i => i.IsActive == true &&
                i.ContentPricePKID == videounlockpost.AlbumPostersPriceFKID).FirstOrDefaultAsync();

                if (followerscount == 0)
                {
                    var follower = new IdentityUser
                    {
                        Email = videounlockpost.FollowersEmailID,
                        UserName = videounlockpost.FollowersName,
                        // PhoneNumber = videounlockpost.FollowersMobileNumber,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(follower, "Squad$123");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(follower, "Follower");

                        addfolowerscount = 1;

                        var followerlogin = savefollowers.SaveFollowersLogin(videounlockpost.FollowersName.Trim('"'), videounlockpost.FollowersEmailID.Trim('"'),
                                            videounlockpost.FollowersMobileNumber.Trim('"'), checkuser.UserLoginPKID);

                        await _context.NGCOREJWT_FollowersLogins.AddAsync(followerlogin);
                        await _context.SaveChangesAsync();

                        var checkfollower = await GetCelebFollowersDetails_UnLock(videounlockpost.CelebrityName, videounlockpost.FollowersEmailID);

                        var unlockedcontents = await _context.NGCOREJWT_UnlockedContents.Where(x => x.IsActive == true && x.CelebrityLoginFKID == checkuser.UserLoginPKID).ToListAsync();

                        foreach (var saveulmfs in unlockedcontents)
                        {
                            NGCOREJWT_UnlockedContent_MF unlockedmfs = new NGCOREJWT_UnlockedContent_MF();
                            unlockedmfs.CelebrityLoginFKID = checkuser.UserLoginPKID;
                            unlockedmfs.FollowersLoginFKID = checkfollower.FollowersLoginPKID;
                            unlockedmfs.UnlockedContentFKID = saveulmfs.UnlockedContentPKID;
                            unlockedmfs.ContentPrice = saveulmfs.ContentPrice;
                            unlockedmfs.ContentPriceFKID = saveulmfs.ContentPriceFKID;
                            unlockedmfs.ContentType = saveulmfs.ContentType;
                            unlockedmfs.ContentCaption = saveulmfs.ContentCaption;
                            unlockedmfs.ContentDesc = saveulmfs.ContentDesc;
                            unlockedmfs.IconPath = saveulmfs.IconPath1;
                            unlockedmfs.ImagePath = saveulmfs.ImagePath1;
                            unlockedmfs.DupFileName = saveulmfs.FileName;
                            unlockedmfs.IsActive = true;
                            unlockedmfs.IsDeleted = false;
                            unlockedmfs.IsLocked = false;
                            unlockedmfs.CreatedBy = "System";
                            unlockedmfs.UpdatedBy = "System";
                            unlockedmfs.CreatedDate = saveulmfs.CreatedDate;
                            unlockedmfs.UpdatedDate = DateTime.Now;
                            unlockedmfs.GSTCharges = saveulmfs.UC_GSTCharges;
                            unlockedmfs.ServiceCharges = saveulmfs.UC_ServiceCharges;
                            unlockedmfs.TotalCharges = saveulmfs.UC_TotalCharges;

                            await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(unlockedmfs);
                            await _context.SaveChangesAsync();
                        }

                    }
                }

                var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == videounlockpost.FollowersName
              && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == videounlockpost.CelebrityName
              && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkuser.UserLoginPKID
               && f.IsActive == true && f.IsLocked == false && f.ContentCaption == videounlockpost.ContentCaption
               && f.ContentPriceFKID == videounlockpost.AlbumPostersPriceFKID && f.FollowersLoginFKID == findfollower.FollowersLoginPKID).ToListAsync();

                foreach (var uplc_mfs in updlockedcontent_mfs)
                {
                    uplc_mfs.IsLocked = true;
                    uplc_mfs.CreatedDate = DateTime.Now;
                    uplc_mfs.IconPath = "fa fa-unlock";
                    uplc_mfs.ImagePath = "assets/videothumbnail.jpg";
                    uplc_mfs.FileName = uplc_mfs.DupFileName;

                    // Increment the UnlockedContent count of video

                    var unlockedcount = await _context.NGCOREJWT_Store_Thumbnails.Where(i => i.IsActive == true && i.UnlockedContentFKID == uplc_mfs.UnlockedContentFKID
                                        && i.ContentCaptions == uplc_mfs.ContentCaption).FirstOrDefaultAsync();

                    if (unlockedcount != null)
                    {
                        if (unlockedcount.UnlockedContentCount == null)
                        {
                            unlockedcount.UnlockedContentCount = 0;
                        }
                        unlockedcount.UnlockedContentCount += 1;
                        unlockedcount.ContentType = uplc_mfs.ContentType;
                        unlockedcount.FileName = uplc_mfs.FileName;
                    }

                    // Payment Section

                    NGCOREJWT_PaymentSection paysection = new NGCOREJWT_PaymentSection();
                    paysection.Celebrity_FKID = findfollower.UserLoginFKID;
                    paysection.Followers_FKID = findfollower.FollowersLoginPKID;
                    paysection.ContentPrice_FKID = videounlockpost.AlbumPostersPriceFKID;
                    paysection.UnlockedContentMF_FKID = uplc_mfs.UnlockedContent_MF_PKID;

                    string shares = string.Empty;
                    //decimal pricededuct = 0;
                    //decimal finaldeduct = 0;
                    //shares = uplc_mfs.PostersPrice;
                    //pricededuct = Convert.ToDecimal(shares);
                    //finaldeduct = pricededuct * 20 / 100;
                    paysection.Share_Bank = 0;

                    decimal ours = 0;
                    decimal finalours_share = 0;
                    shares = posterprice.ContentPrice.Split("/-")[0];
                    ours = Convert.ToDecimal(shares);
                    finalours_share = ours * 20 / 100;
                    paysection.Share_Ours = finalours_share;

                    decimal celebrity = 0;
                    decimal finalcelebrity_share = 0;
                    //shares = uplc_mfs.PostersPrice;
                    celebrity = Convert.ToDecimal(shares);
                    finalcelebrity_share = celebrity * 80 / 100;
                    paysection.Share_Celebrities = finalcelebrity_share;

                    paysection.ContentPrice = uplc_mfs.ContentPrice;
                    paysection.IsActive = true;
                    paysection.IsDeleted = false;
                    paysection.CreatedBy = "System";
                    paysection.UpdatedBy = "System";
                    paysection.CreatedDate = DateTime.Now;
                    paysection.UpdatedDate = DateTime.Now;

                    await _context.NGCOREJWT_PaymentSections.AddAsync(paysection);

                    // Generate Invoices

                    NGCOREJWT_GenerateInvoice geninvoice = new NGCOREJWT_GenerateInvoice();
                    geninvoice.CelebrityFKID = checkuser.UserLoginPKID;
                    geninvoice.FollowerFKID = findfollower.FollowersLoginPKID;
                    geninvoice.ContentPriceFKID = videounlockpost.AlbumPostersPriceFKID;
                    geninvoice.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    geninvoice.FollowerEmailID = findfollower.EmailID;
                    geninvoice.InvoiceNo = "CS-VID-" + uplc_mfs.UnlockedContent_MF_PKID + "_" + Convert.ToDateTime(DateTime.Now).ToString("ddMMyyyyhhmmss");
                    geninvoice.InvDescription = "Streaming Service";
                    geninvoice.Amount = Convert.ToDecimal(shares);
                    geninvoice.GSTCharges = Convert.ToDecimal(uplc_mfs.GSTCharges);
                    geninvoice.ServiceCharges = Convert.ToDecimal(uplc_mfs.ServiceCharges);
                    geninvoice.TotalCharges = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    geninvoice.IsActive = true;
                    geninvoice.IsDeleted = false;
                    geninvoice.CreatedBy = "System";
                    geninvoice.CreatedDate = DateTime.Now;

                    await _context.NGCOREJWT_GenerateInvoices.AddAsync(geninvoice);

                    // save customer payment details

                    CustPaymentDetail custPayment = new CustPaymentDetail();
                    custPayment.CelebrityFKID = checkuser.UserLoginPKID;
                    custPayment.FollowerFKID = findfollower.FollowersLoginPKID;
                    custPayment.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    custPayment.Amount = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    custPayment.Currency = "INR";
                    custPayment.PaymentCapture = 1;
                    custPayment.RzpayOrderID = videounlockpost.RzpayOrderID;
                    custPayment.RzpayPaymentID = videounlockpost.RzpayPaymentID;
                    custPayment.RzpaySignature = videounlockpost.RzpaySignature;
                    custPayment.PaymentStatus = "Success";
                    custPayment.IsActive = true;
                    custPayment.IsDeleted = false;
                    custPayment.CreatedDate = DateTime.Now;

                    await _context.CustPaymentDetails.AddAsync(custPayment);

                    //   await GeneratePDF_EmailNotification(findfollower.EmailID, "CS-VID-" + uplc_mfs.UnlockedContent_MF_PKID, "",
                    //   "Streaming Service", Convert.ToDecimal(shares), Convert.ToDecimal(uplc_mfs.GSTCharges), Convert.ToDecimal(uplc_mfs.ServiceCharges),
                    //  Convert.ToDecimal(uplc_mfs.TotalCharges));
                }

                await _context.SaveChangesAsync();
                if (addfolowerscount == 0)
                {
                    await EmailNotification("Dear " + videounlockpost.CelebrityName + ", your follower named " + videounlockpost.FollowersName + " has unlocked a video", videounlockpost.ContentCaption, " Unlocked your " + videounlockpost.ContentCaption + " (Post @ " + videounlockpost.PriceInfo + ")", addfolowerscount, videounlockpost.CelebrityName);
                }
                else
                {
                    await EmailNotification("Dear " + videounlockpost.CelebrityName + ", you got a new follower named " + videounlockpost.FollowersName, videounlockpost.ContentCaption, " Unlocked your " + videounlockpost.ContentCaption + " (Post @ " + videounlockpost.PriceInfo + ")", addfolowerscount, videounlockpost.CelebrityName);
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockVideoPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        public async Task<Celebrity_FollowersName> GetCelebFollowersDetails_UnLock(string celebname, string followersemailid)
        {
            var firstdef = await (from usr in _context.NGCOREJWT_UserLogins
                                  join flog in _context.NGCOREJWT_FollowersLogins on usr.UserLoginPKID equals flog.UserLoginFKID
                                  where flog.IsActive == true && usr.IsActive == true && flog.IsApproved == true && flog.IsRegistered == true
                                  && usr.IsActive == true && usr.IsApproved == true && usr.IsRegistered == true
                                  && usr.PersonalityType == "Celebrity" && usr.FullName == celebname && flog.EmailID == followersemailid
                                  select new Celebrity_FollowersName()
                                  {
                                      CelebrityName = usr.FullName,
                                      FollowersName = flog.FollowersName,
                                      FollowersLoginPKID = flog.FollowersLoginPKID,
                                      CelebrityFKID = usr.UserLoginPKID,
                                      EmailID = flog.EmailID
                                  }).FirstOrDefaultAsync();
            return firstdef;
        }

        [Route("video_alreadyexists/{CelebrityName}/{FollowersEmailID}/{VideoCaption}")]
        [HttpGet("{CelebrityName}/{FollowersEmailID}/{VideoCaption}")]
        public async Task<int> VideoAlreadyExists(string CelebrityName, string FollowersEmailID, string VideoCaption)
        {
            int videocount = 0;
            var celebfollowerdet = await GetCelebFollowersDetails_UnLock(CelebrityName, FollowersEmailID);
            if (celebfollowerdet == null)
            {
                videocount = 0;
            }
            else
            {
                videocount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true &&
                u.ContentCaption == VideoCaption && u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();
            }

            return videocount;
        }

        // register the user and unlocked the video post
        [Route("video_unlockedpost")]
        [HttpPost]
        public async Task<JsonResult> VideoUnlockPost([FromForm] UnlockedPost_VideoFormdata videounlockpost)
        {
            string video_alertmessage = string.Empty;
            try
            {
                //var celebfollowerdet = await GetCelebFollowersDetails_UnLock(videounlockpost.CelebrityName, videounlockpost.FollowersEmailID);

                //int video_alreadyexist = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true && u.ContentCaption == videounlockpost.ContentCaption
                //&& u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();

                //if (video_alreadyexist == 0)
                //{
                var usercount = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == videounlockpost.CelebrityName
                           && x.IsActive == true && x.IsRegistered == true && x.PersonalityType == "Celebrity" && x.IsApproved == true).CountAsync();

                if (usercount == 1)
                {
                    await SaveFollowersData_UnlockVideoFormData(videounlockpost);
                }

                video_alertmessage = "Not Exists";
                //}
                //else
                //{
                //    video_alertmessage = "Already Exists";
                //}
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockVideoPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(video_alertmessage);
        }

        // To open the Video file

        [Route("openvideofile/{FullName}/{FileName}")]
        [HttpGet("{FullName}/{FileName}")]
        public async Task<FileStream> OpenVideoFile(string FullName, string FileName)
        {
            var path = (dynamic)null;
            try
            {
                var files = await (from user in _context.NGCOREJWT_UserLogins
                                   join vid in _context.NGCOREJWT_VideoCollections on user.UserLoginPKID equals vid.UserLoginFKID
                                   where user.IsActive == true && user.FullName == FullName && vid.FileName == FileName && vid.IsActive == true
                                   select new NGCOREJWT_VideoCollection()
                                   {
                                       FileName = vid.FileName.Trim('"'),
                                       CreatedDate = vid.CreatedDate
                                   }).FirstOrDefaultAsync();

                var filepath = Path.Combine("VideoCollections", FullName, Convert.ToDateTime(files.CreatedDate).ToString("dd-MM-yyyy"));

                path = Path.Combine(
                         _hostingEnvironment.ContentRootPath, filepath,
                         files.FileName);

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "VideoCollections_OpenVideo");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        // Get the video liked count - GetUC_VideoLikedCount

        [Route("getucvideo_likedcount/{CelebrityName}/{FollowersName}/{VideoCaptions}/{PosterPriceFKID}")]
        [HttpGet("{CelebrityName}/{FollowersName}/{VideoCaptions}/{PosterPriceFKID}")]
        public async Task<List<GetVideo_LikedCount>> GetUC_VideoLikedCount(string CelebrityName, string FollowersName,
                                                                          string VideoCaptions, int PosterPriceFKID)
        {
            var getlikedvideo = (dynamic)null;
            try
            {
                var chkceluser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.FollowersName == FollowersName
                && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.IsActive == true && j.IsApproved == true && j.IsRegistered == true
                && j.FullName == CelebrityName && j.PersonalityType == "Celebrity").Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                int likedcount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == VideoCaptions
                && j.ContentPriceFKID == PosterPriceFKID && j.IsLocked == true && j.IsLiked == true).CountAsync();

                getlikedvideo = await (from ucmfs in _context.NGCOREJWT_UnlockedContent_MFs
                                       where ucmfs.IsActive == true && ucmfs.IsLocked == true && ucmfs.ContentCaption == VideoCaptions
                                       && ucmfs.ContentPriceFKID == PosterPriceFKID && ucmfs.FollowersLoginFKID == chkceluser.FollowersLoginPKID
                                       && ucmfs.CelebrityLoginFKID == chkceluser.UserLoginFKID
                                       select new GetVideo_LikedCount()
                                       {
                                           IsLiked = ucmfs.IsLiked == null ? false : true,
                                           UC_LikedCount = likedcount
                                       }).ToListAsync();

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockedContent_Get_Video_LikedCount");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getlikedvideo;
        }

        // Update the video liked count - UpdateUC_VideoLikedCount

        [Route("updateucvideo_likedcount")]
        [HttpPut]
        public async Task<List<GetVideo_LikedCount>> UpdateUC_VideoLikedCount([FromForm] Update_UC_LikedVideoCount upducvidlikedcount)
        {
            var getlikedvideo = (dynamic)null;
            try
            {
                var chkceluser = await _context.NGCOREJWT_FollowersLogins.Where(i => i.IsActive == true && i.FollowersName == upducvidlikedcount.FollowersName
                && i.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(j => j.IsActive == true && j.IsApproved == true && j.IsRegistered == true
                && j.FullName == upducvidlikedcount.CelebrityName && j.PersonalityType == "Celebrity").Select(s => s.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                var upducmfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == upducvidlikedcount.VideoCaptions
                && j.ContentPriceFKID == upducvidlikedcount.PosterPriceFKID && j.IsLocked == true && j.CelebrityLoginFKID == chkceluser.UserLoginFKID
                && j.FollowersLoginFKID == chkceluser.FollowersLoginPKID && j.IsLiked == null).FirstOrDefaultAsync();

                upducmfs.IsLiked = true;
                upducmfs.UpdatedDate = DateTime.Now;

                var store = await _context.NGCOREJWT_Store_Thumbnails.Where(k => k.IsActive == true && k.ContentCaptions == upducvidlikedcount.VideoCaptions
                && k.ContentPriceFKID == upducvidlikedcount.PosterPriceFKID && k.CelebrityFKID == chkceluser.UserLoginFKID).FirstOrDefaultAsync();

                store.UpdatedDate = DateTime.Now;
                if (store.UC_LikedCount == null)
                {
                    store.UC_LikedCount = 0;
                }
                store.UC_LikedCount += 1;

                await _context.SaveChangesAsync();

                int v_likedcount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == upducvidlikedcount.VideoCaptions
                && j.ContentPriceFKID == upducvidlikedcount.PosterPriceFKID && j.IsLocked == true && j.IsLiked == true).CountAsync();

                getlikedvideo = await (from ucmfs in _context.NGCOREJWT_UnlockedContent_MFs
                                       where ucmfs.IsActive == true && ucmfs.IsLocked == true && ucmfs.ContentCaption == upducvidlikedcount.VideoCaptions
                                       && ucmfs.ContentPriceFKID == upducvidlikedcount.PosterPriceFKID && ucmfs.FollowersLoginFKID == chkceluser.FollowersLoginPKID
                                       && ucmfs.CelebrityLoginFKID == chkceluser.UserLoginFKID && ucmfs.IsLiked == true
                                       select new GetVideo_LikedCount()
                                       {
                                           IsLiked = ucmfs.IsLiked,
                                           UC_LikedCount = v_likedcount
                                       }).ToListAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockedContent_Update_Video_LikedCount");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getlikedvideo;
        }

        // Insert the video comments

        [Route("postvideocomments")]
        [HttpPost]
        public async Task<IActionResult> SuggestComments_Video([FromForm] VideoSuggestComments vsuggcomments)
        {
            // var videosuggcomments = (dynamic)null;
            try
            {
                var chkusers = await GetCelebFollowersDetails(vsuggcomments.CelebrityName, vsuggcomments.Followersname);

                var contentmfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == chkusers.CelebrityFKID
                && f.IsActive == true && f.IsLocked == true && f.ContentCaption == vsuggcomments.VideoCaption &&
                f.FollowersLoginFKID == chkusers.FollowersLoginPKID && f.FileName == vsuggcomments.VideoFileName).FirstOrDefaultAsync();

                if (contentmfs != null)
                {
                    NGCOREJWT_SuggestComment suggestcomment = new NGCOREJWT_SuggestComment();
                    suggestcomment.CelebrityFKID = contentmfs.CelebrityLoginFKID;
                    suggestcomment.FollowerFKID = contentmfs.FollowersLoginFKID;
                    suggestcomment.UnlockedContent_MF_FKID = contentmfs.UnlockedContentFKID;
                    suggestcomment.Comments = vsuggcomments.VideoComments;
                    suggestcomment.IsActive = true;
                    suggestcomment.IsDeleted = false;
                    suggestcomment.CreatedBy = "System";
                    suggestcomment.CreatedDate = DateTime.Now;

                    await _context.NGCOREJWT_SuggestComments.AddAsync(suggestcomment);
                    await _context.SaveChangesAsync();

                    //videosuggcomments = await (from suggcmts in _context.NGCOREJWT_SuggestComments
                    //                           where suggcmts.IsActive == true && suggcmts.UnlockedContent_MF_FKID == contentmfs.UnlockedContent_MF_PKID
                    //                           select new GetVideoSuggestComments()
                    //                           {
                    //                               vsubscribername = vsuggcomments.Followersname,
                    //                               vcomments = suggcmts.Comments,
                    //                               vcommentdate = Convert.ToDateTime(suggcmts.CreatedDate)
                    //                           }).FirstOrDefaultAsync();

                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Video_InsertUserComments");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Get the video comments - GetVideoComments

        [Route("getvideocomments/{CelebrityName}/{FollowersName}/{Vcaptions}/{Vfilename}")]
        [HttpGet("{CelebrityName}/{FollowersName}/{Vcaptions}/{Vfilename}")]
        public async Task<List<GetVideoSuggestComments>> GetVideoComments(string CelebrityName, string FollowersName,
                                                                          string Vcaptions, string Vfilename)
        {
            var getvcomments = (dynamic)null;
            try
            {
                var chkusers = await GetCelebFollowersDetails(CelebrityName, FollowersName);

                var getvideocmts = await _context.NGCOREJWT_UnlockedContent_MFs.Where(j => j.IsActive == true && j.ContentCaption == Vcaptions
                && j.FileName == Vfilename && j.IsLocked == true && j.CelebrityLoginFKID == chkusers.CelebrityFKID).FirstOrDefaultAsync();

                getvcomments = await (from scmts in _context.NGCOREJWT_SuggestComments
                                      join flog in _context.NGCOREJWT_FollowersLogins on scmts.FollowerFKID equals flog.FollowersLoginPKID
                                      join unlockmfs in _context.NGCOREJWT_UnlockedContents on scmts.UnlockedContent_MF_FKID equals unlockmfs.UnlockedContentPKID
                                      where scmts.UnlockedContent_MF_FKID == getvideocmts.UnlockedContentFKID
                                      select new GetVideoSuggestComments()
                                      {
                                          vsubscribername = flog.FollowersName,
                                          vcomments = scmts.Comments,
                                          vcommentdate = Convert.ToDateTime(scmts.CreatedDate)
                                      }).ToListAsync();

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Video_UserComments");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return getvcomments;
        }

        // Document Controller related methods


        // Upload the Documents

        [Route("insertdocuments")]
        [HttpPost]
        public async Task<ActionResult<NGCOREJWT_DocumentCollection>> UploadDocuments([FromForm] DocumentFormData docfdata)
        {
            try
            {

                if (docfdata != null && docfdata.DocFilename.Length > 0)
                {
                    // string foldername = "AudioCollections";
                    // string userfolder = docfdata.UserName;
                    string newpath = Path.Combine("DocumentCollections");
                    // foreach (var file in docfdata.FileName)
                    // {
                    var directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, docfdata.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));

                    if (!directoryinfo.Exists)
                    {
                        directoryinfo = Directory.CreateDirectory(Path.Combine(newpath, docfdata.UserName, Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy")));
                    }

                    var docfilename = Path.GetFileName(docfdata.DocFilename.FileName.Trim('"'));

                    var docthumbname = Path.GetFileName(docfdata.DocThumbnailname.FileName.Trim('"'));


                    if (System.IO.File.Exists(docfilename) || System.IO.File.Exists(docthumbname))
                    {
                        System.IO.File.Delete(docfilename);
                        System.IO.File.Delete(docthumbname);
                    }

                    var finalpath = Path.Combine(Directory.GetCurrentDirectory(), directoryinfo.FullName);

                    var pathToSave = Path.Combine(finalpath, docfilename);

                    var docthumbpath = Path.Combine(finalpath, docthumbname);

                    using (var stream = new FileStream(pathToSave, FileMode.Create))
                    {
                        await docfdata.DocFilename.CopyToAsync(stream);
                    }
                    using (var stream = new FileStream(docthumbpath, FileMode.Create))
                    {
                        await docfdata.DocThumbnailname.CopyToAsync(stream);
                    }

                    var getloginid = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == docfdata.UserName && i.IsActive == true &&
                    i.IsRegistered == true).FirstOrDefaultAsync();

                    if (getloginid != null)
                    {
                        NGCOREJWT_DocumentCollection docs = new NGCOREJWT_DocumentCollection();
                        docs.UserLoginFKID = getloginid.UserLoginPKID;
                        docs.ContentPriceFKID = docfdata.AlbumPosterPriceFKID;
                        docs.FileName = docfdata.DocFilename.FileName.Trim('"');
                        docs.ContentType = Path.GetExtension(docfdata.DocFilename.FileName.Trim('"'));
                        //MemoryStream docms = new MemoryStream();
                        //await docfdata.DocThumbnailname.CopyToAsync(docms);
                        //docs.DocumentData = docms.ToArray();
                        docs.DocumentCaption = docfdata.DocumentCaption.Trim('"');
                        docs.DocumentDesc = docfdata.DocumentDesc.Trim('"');
                        docs.Doct_GSTCharges = docfdata.D_GSTCharges;
                        docs.Doct_ServiceCharges = docfdata.D_ServiceCharges;
                        docs.Doct_TotalCharges = docfdata.D_TotalCharges;
                        //  docs.DocThumbnail = docfdata.DocThumbnailname.FileName.Trim('"');
                        //DirectoryInfo di = new DirectoryInfo(pathToSave);
                        //FileInfo[] fileinfo = di.GetFiles();
                        //videos.VideoSize = fileinfo.Length;

                        docs.IsActive = true;
                        docs.IsDeleted = false;
                        docs.IsLocked = false;
                        docs.CreatedBy = "System";
                        docs.UpdatedBy = "System";
                        docs.CreatedDate = DateTime.Now;
                        docs.UpdatedDate = DateTime.Now;

                        _context.NGCOREJWT_DocumentCollections.Add(docs);

                        NGCOREJWT_UnlockedContent unlockdocs = new NGCOREJWT_UnlockedContent();
                        unlockdocs.CelebrityLoginFKID = getloginid.UserLoginPKID;
                        unlockdocs.ContentPriceFKID = docfdata.AlbumPosterPriceFKID;
                        unlockdocs.ContentType = Path.GetExtension(docfdata.DocFilename.FileName.Trim('"'));
                        unlockdocs.ContentCaption = docfdata.DocumentCaption.Trim('"');
                        unlockdocs.FileName = docfdata.DocFilename.FileName.Trim('"');
                        unlockdocs.ContentDesc = docfdata.DocumentDesc.Trim('"');
                        unlockdocs.IconPath = "fa fa-lock";
                        unlockdocs.ImagePath = "assets/userpdfthumbnail.jpg";
                        unlockdocs.ContentPrice = docfdata.PostersPrice.Trim('"');
                        unlockdocs.IconPath1 = "fa fa-lock";
                        unlockdocs.ImagePath1 = "assets/userpdfthumbnail.jpg";
                        unlockdocs.IsActive = true;
                        unlockdocs.IsDeleted = false;
                        unlockdocs.IsLocked = false;
                        unlockdocs.CreatedBy = "System";
                        unlockdocs.UpdatedBy = "System";
                        unlockdocs.CreatedDate = DateTime.Now;
                        unlockdocs.UpdatedDate = DateTime.Now;
                        unlockdocs.UC_GSTCharges = docfdata.D_GSTCharges;
                        unlockdocs.UC_ServiceCharges = docfdata.D_ServiceCharges;
                        unlockdocs.UC_TotalCharges = docfdata.D_TotalCharges;
                        //MemoryStream docmss = new MemoryStream();
                        //await docfdata.DocThumbnailname.CopyToAsync(docmss);
                        //unlockdocs.UC_ThumbnailImage = docmss.ToArray();

                        _context.NGCOREJWT_UnlockedContents.Add(unlockdocs);

                        await _context.SaveChangesAsync();

                        var getfollowers = await _context.NGCOREJWT_FollowersLogins.Where(i => i.UserLoginFKID == getloginid.UserLoginPKID && i.IsActive == true
                        && i.IsRegistered == true && i.IsApproved == true).ToListAsync();

                        var contentid = await _context.NGCOREJWT_UnlockedContents.Where(i => i.ContentCaption == docfdata.DocumentCaption &&
                        i.FileName == docfdata.DocFilename.FileName.Trim('"') && i.ContentPrice == docfdata.PostersPrice &&
                        i.ContentType == Path.GetExtension(docfdata.DocFilename.FileName.Trim('"'))
                        && i.ContentPriceFKID == docfdata.AlbumPosterPriceFKID).FirstOrDefaultAsync();

                        if (getfollowers == null || getfollowers.Count == 0)
                        {
                            NGCOREJWT_Store_Thumbnail storedocthumb = new NGCOREJWT_Store_Thumbnail();
                            MemoryStream audthumb = new MemoryStream();
                            await docfdata.DocThumbnailname.CopyToAsync(audthumb);
                            storedocthumb.ThumbnailImage = audthumb.ToArray();
                            storedocthumb.ThumbnailPath = docfdata.DocThumbnailname.FileName.Trim('"');
                            storedocthumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                            storedocthumb.CelebrityFKID = getloginid.UserLoginPKID;
                            storedocthumb.ContentCaptions = docfdata.DocumentCaption;
                            storedocthumb.UnlockedContentCount = 0;
                            storedocthumb.ContentType = contentid.ContentType;
                            storedocthumb.FileName = docfdata.DocFilename.FileName.Trim('"');
                            storedocthumb.ContentPriceFKID = docfdata.AlbumPosterPriceFKID;
                            storedocthumb.IsActive = true;
                            storedocthumb.IsDeleted = false;
                            storedocthumb.CreatedBy = "System";
                            storedocthumb.CreatedDate = DateTime.Now;
                            storedocthumb.UpdatedBy = "System";
                            storedocthumb.UpdatedDate = DateTime.Now;

                            await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storedocthumb);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {

                            int doccount = 0;
                            foreach (var insertnewdocs in getfollowers)
                            {
                                NGCOREJWT_UnlockedContent_MF insertdocs = new NGCOREJWT_UnlockedContent_MF();
                                insertdocs.CelebrityLoginFKID = getloginid.UserLoginPKID;
                                insertdocs.FollowersLoginFKID = insertnewdocs.FollowersLoginPKID;
                                insertdocs.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                insertdocs.ContentPriceFKID = docfdata.AlbumPosterPriceFKID;
                                insertdocs.ContentPrice = docfdata.PostersPrice;
                                insertdocs.ContentType = contentid.ContentType;
                                insertdocs.ContentCaption = docfdata.DocumentCaption;
                                insertdocs.ContentDesc = docfdata.DocumentDesc;
                                insertdocs.IconPath = "fa fa-lock";
                                insertdocs.ImagePath = "assets/userpdfthumbnail.jpg";
                                insertdocs.FileName = null; ;
                                insertdocs.DupFileName = docfdata.DocFilename.FileName.Trim('"');
                                insertdocs.IsActive = true;
                                insertdocs.IsDeleted = false;
                                insertdocs.IsLocked = false;
                                insertdocs.CreatedBy = "System";
                                insertdocs.CreatedDate = DateTime.Now;
                                insertdocs.UpdatedBy = "System";
                                insertdocs.UpdatedDate = DateTime.Now;
                                insertdocs.GSTCharges = docfdata.D_GSTCharges;
                                insertdocs.ServiceCharges = docfdata.D_ServiceCharges;
                                insertdocs.TotalCharges = docfdata.D_TotalCharges;
                                //MemoryStream docthms = new MemoryStream();
                                //await docfdata.DocThumbnailname.CopyToAsync(docthms);
                                //insertdocs.UC_ThumbnailImage_MF = docthms.ToArray();

                                await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(insertdocs);
                                doccount += 1;
                                if (doccount == 1)
                                {
                                    NGCOREJWT_Store_Thumbnail storedocthumb = new NGCOREJWT_Store_Thumbnail();
                                    MemoryStream audthumb = new MemoryStream();
                                    await docfdata.DocThumbnailname.CopyToAsync(audthumb);
                                    storedocthumb.ThumbnailImage = audthumb.ToArray();
                                    storedocthumb.ThumbnailPath = docfdata.DocThumbnailname.FileName.Trim('"');
                                    storedocthumb.UnlockedContentFKID = contentid.UnlockedContentPKID;
                                    storedocthumb.CelebrityFKID = getloginid.UserLoginPKID;
                                    storedocthumb.ContentCaptions = docfdata.DocumentCaption;
                                    storedocthumb.UnlockedContentCount = 0;
                                    storedocthumb.ContentType = contentid.ContentType;
                                    storedocthumb.FileName = docfdata.DocFilename.FileName.Trim('"');
                                    storedocthumb.ContentPriceFKID = docfdata.AlbumPosterPriceFKID;
                                    storedocthumb.IsActive = true;
                                    storedocthumb.IsDeleted = false;
                                    storedocthumb.CreatedBy = "System";
                                    storedocthumb.CreatedDate = DateTime.Now;
                                    storedocthumb.UpdatedBy = "System";
                                    storedocthumb.UpdatedDate = DateTime.Now;

                                    await _context.NGCOREJWT_Store_Thumbnails.AddAsync(storedocthumb);
                                    await _context.SaveChangesAsync();
                                }

                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    await EmailNotification(docfdata.UserName, docfdata.DocumentCaption, "New Document Uploaded", 2, "");
                    //   }
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "Upload_Documents_ByCelebrity");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Load the Document Collections

        [Route("load_documentdet/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<List<Documentshowdet>> BindDocumentDetails(string FullName)
        {
            var docdet = await (from userlog in _context.NGCOREJWT_UserLogins
                                join docs in _context.NGCOREJWT_DocumentCollections on userlog.UserLoginPKID equals docs.UserLoginFKID
                                join price in _context.NGCOREJWT_ContentsPrices on docs.ContentPriceFKID equals price.ContentPricePKID
                                where docs.UserLoginFKID == userlog.UserLoginPKID && docs.IsActive == true && userlog.FullName == FullName
                                && price.IsActive == true && userlog.IsActive == true && userlog.IsRegistered == true &&
                                userlog.PersonalityType == "Celebrity" && userlog.IsApproved == true
                                where userlog.IsActive == true && userlog.FullName == FullName && docs.IsActive == true
                                orderby docs.CreatedDate descending
                                select new Documentshowdet()
                                {
                                    DocumentCollectionPKID = docs.DocCollectionPKID,
                                    DocumentCaption = docs.DocumentCaption,
                                    DocumentDesc = docs.DocumentDesc,
                                    FileName = docs.FileName,
                                    Createdate = Convert.ToDateTime(docs.CreatedDate),
                                    PriceInfo = docs.Doct_TotalCharges,
                                    IsLocked = docs.IsLocked == false ? "Locked" : "",
                                    AlbumPosterPricePKID = price.ContentPricePKID,
                                    DocThumbnail = docs.DocumentData == null ? "assets/userpdfthumbnail.jpg" : null
                                    //  AudioData = string.Format("data:video/;base64,{0}", Convert.ToBase64String(audios.AudioData))
                                }).ToListAsync();
            return docdet;
        }

        // Load the documents

        [Route("loadpdfdocuments/{FullName}")]
        [HttpGet("{FullName}")]
        public IActionResult LoadPdfDoc(string FullName)
        {
            var result = new List<string>();

            var docpath = Path.Combine("DocumentCollections", FullName);

            // var documents = Path.Combine(docpath, FullName);
            if (Directory.Exists(docpath))
            {
                var provider = _hostingEnvironment.ContentRootFileProvider;
                foreach (string fileName in Directory.GetFiles(docpath))
                {
                    var fileInfo = provider.GetFileInfo(fileName);
                    result.Add(fileInfo.Name.Trim('"'));
                }
            }
            return Ok(result);
        }

        // To open the PDF document

        [Route("openpdfdoc/{FullName}/{FileName}")]
        [HttpGet("{FullName}/{FileName}")]
        public async Task<FileStream> OpenPDFDocument(string FullName, string FileName)
        {
            var path = (dynamic)null;
            try
            {
                var files = await (from user in _context.NGCOREJWT_UserLogins
                                   join docs in _context.NGCOREJWT_DocumentCollections on user.UserLoginPKID equals docs.UserLoginFKID
                                   where user.IsActive == true && user.FullName == FullName && docs.FileName == FileName && docs.IsActive == true
                                   select new NGCOREJWT_DocumentCollection()
                                   {
                                       FileName = docs.FileName.Trim('"'),
                                       CreatedDate = docs.CreatedDate
                                   }).FirstOrDefaultAsync();

                var filepath = Path.Combine("DocumentCollections", FullName, Convert.ToDateTime(files.CreatedDate).ToString("dd-MM-yyyy"));

                path = Path.Combine(
                         _hostingEnvironment.ContentRootPath, filepath,
                         files.FileName);

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "DocumentCollections_OpenPDF");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        // document modal popup

        [Route("getdocument_popup/{albposterpricepkid}/{docollectionpkid}/{docscaption}")]
        [HttpGet("{albposterpricepkid}/{docollectionpkid}/{docscaption}")]
        public async Task<List<GetDocuments_Popup>> GetDocument_Popup(int albposterpricepkid, int docollectionpkid, string docscaption)
        {
            var docdetpopup = (dynamic)null;
            var getserviceid = await _context.NGCOREJWT_ContentsPrices.Where(i => i.ContentPricePKID == albposterpricepkid && i.IsActive == true).FirstOrDefaultAsync();

            if (getserviceid != null)
            {
                if (docollectionpkid == 0)
                {
                    docdetpopup = await (from docscoll in _context.NGCOREJWT_DocumentCollections
                                         join albumprice in _context.NGCOREJWT_ContentsPrices on docscoll.ContentPriceFKID equals albumprice.ContentPricePKID
                                         where docscoll.ContentPriceFKID == getserviceid.ContentPricePKID && docscoll.IsActive == true
                                         && docscoll.DocumentCaption == docscaption
                                         select new GetDocuments_Popup()
                                         {
                                             DocumentCaption = docscoll.DocumentCaption,
                                             PriceInfo = Convert.ToInt32(docscoll.Doct_TotalCharges) * 100 / 100 + "/-",
                                             DocumentCollPKID = docscoll.DocCollectionPKID,
                                             AlbumPosterPricePKID = albumprice.ContentPricePKID
                                         }).ToListAsync();
                }
                else
                {
                    docdetpopup = await (from docscoll in _context.NGCOREJWT_UnlockedContents
                                         join albumprice in _context.NGCOREJWT_ContentsPrices on docscoll.ContentPriceFKID equals albumprice.ContentPricePKID
                                         where docscoll.ContentPriceFKID == getserviceid.ContentPricePKID && docscoll.IsActive == true
                                         && docscoll.ContentCollectionFKID == docollectionpkid || docscoll.ContentCaption == docscaption
                                         select new GetDocuments_Popup()
                                         {
                                             DocumentCaption = docscoll.ContentCaption,
                                             PriceInfo = Convert.ToInt32(docscoll.UC_TotalCharges) * 100 / 100 + "/-",
                                             DocumentCollPKID = docscoll.ContentCollectionFKID,
                                             AlbumPosterPricePKID = albumprice.ContentPricePKID
                                         }).ToListAsync();
                }

            }


            return docdetpopup;
        }

        // Update the Document caption and price - UpdateDocumentCap_Price

        [Route("updatedoctcap_price")]
        [HttpPut]
        public async Task<ActionResult<Update_DoctCap_Price_Celeb>> UpdateDoctCap_Price([FromForm] Update_DoctCap_Price_Celeb update_docap_price)
        {
            try
            {
                string resultmsg = string.Empty;

                var celebrityid = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == update_docap_price.CelebrityName && x.PersonalityType == "Celebrity" &&
                                  x.IsActive == true && x.IsRegistered == true && x.IsApproved == true).FirstOrDefaultAsync();

                var upd_doc_coll = await _context.NGCOREJWT_DocumentCollections.Where(v => v.IsActive == true && v.FileName == update_docap_price.DoctFileName &&
                                v.UserLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == update_docap_price.DoctFileName &&
                                    v.CelebrityLoginFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                var upd_unlock_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == update_docap_price.DoctFileName &&
                             v.CelebrityLoginFKID == celebrityid.UserLoginPKID).ToListAsync();

                var upd_thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == update_docap_price.DoctFileName &&
                                 v.CelebrityFKID == celebrityid.UserLoginPKID).FirstOrDefaultAsync();

                if (upd_doc_coll != null && upd_unlock != null && upd_unlock_mfs != null && upd_thumbnails != null)
                {
                    upd_doc_coll.Doct_GSTCharges = update_docap_price.Doct_GSTCharges;
                    upd_doc_coll.Doct_ServiceCharges = update_docap_price.Doct_ServiceCharges;
                    upd_doc_coll.Doct_TotalCharges = update_docap_price.Doct_TotalCharges;
                    upd_doc_coll.ContentPriceFKID = update_docap_price.PosterPriceFKID;
                    upd_doc_coll.DocumentCaption = update_docap_price.Doc_Caption;
                    upd_doc_coll.UpdatedDate = DateTime.Now;

                    upd_unlock.UC_GSTCharges = update_docap_price.Doct_GSTCharges;
                    upd_unlock.UC_ServiceCharges = update_docap_price.Doct_ServiceCharges;
                    upd_unlock.UC_TotalCharges = update_docap_price.Doct_TotalCharges;
                    upd_unlock.ContentPriceFKID = update_docap_price.PosterPriceFKID;
                    upd_unlock.ContentPrice = update_docap_price.PriceInfo;
                    upd_unlock.ContentCaption = update_docap_price.Doc_Caption;
                    upd_unlock.UpdatedDate = DateTime.Now;

                    foreach (var unlock_mfs in upd_unlock_mfs)
                    {
                        unlock_mfs.ContentPriceFKID = update_docap_price.PosterPriceFKID;
                        unlock_mfs.ContentPrice = update_docap_price.PriceInfo;
                        unlock_mfs.ContentCaption = update_docap_price.Doc_Caption;
                        unlock_mfs.UpdatedDate = DateTime.Now;
                        unlock_mfs.GSTCharges = update_docap_price.Doct_GSTCharges;
                        unlock_mfs.ServiceCharges = update_docap_price.Doct_ServiceCharges;
                        unlock_mfs.TotalCharges = update_docap_price.Doct_TotalCharges;
                    }

                    upd_thumbnails.ContentPriceFKID = update_docap_price.PosterPriceFKID;
                    upd_thumbnails.ContentCaptions = update_docap_price.Doc_Caption;
                    upd_thumbnails.UpdatedDate = DateTime.Now;

                    resultmsg = null;
                }
                else
                {
                    resultmsg = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UpdateCelebrity_Document_Caption_Price");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // Sync the Document caption and price from the Document Collection table - EditDocCap_Price_Artist

        [Route("binddoctdetails_celeb/{celebrityname}/{editdocfile}")]
        [HttpGet("{celebrityname}/{editdocfile}")]
        public async Task<Edit_DocCap_Price_Celeb> GetCelebDoctDetails(string celebrityname, string editdocfile)
        {
            var audiodet = await (from doct in _context.NGCOREJWT_DocumentCollections
                                  join usr in _context.NGCOREJWT_UserLogins on doct.UserLoginFKID equals usr.UserLoginPKID
                                  join price in _context.NGCOREJWT_ContentsPrices on doct.ContentPriceFKID equals price.ContentPricePKID
                                  where price.IsActive == true && usr.FullName == celebrityname && usr.IsActive == true
                                  && doct.IsActive == true && doct.FileName == editdocfile
                                  select new Edit_DocCap_Price_Celeb()
                                  {
                                      Doc_Caption = doct.DocumentCaption,
                                      PosterPriceFKID = doct.ContentPriceFKID,
                                      PostersPrice = price.ContentPrice
                                  }).FirstOrDefaultAsync();
            return audiodet;
        }

        // Document Deletion by the Celebrity

        [Route("deletedocs/{celebrityname}/{doccaptions}/{docfilename}")]
        [HttpDelete("{celebrityname}/{doccaptions}/{docfilename}")]
        public async Task<JsonResult> DeleteDocuments(string celebrityname, string doccaptions, string docfilename)
        {
            string result = string.Empty;
            try
            {
                var celebid = await _context.NGCOREJWT_UserLogins.Where(i => i.IsActive == true && i.IsRegistered == true && i.IsApproved == true &&
                              i.PersonalityType == "Celebrity" && i.FullName == celebrityname).FirstOrDefaultAsync();

                var docutcall = await _context.NGCOREJWT_DocumentCollections.Where(v => v.IsActive == true && v.FileName == docfilename &&
                               v.DocumentCaption == doccaptions && v.UserLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlockcontent = await _context.NGCOREJWT_UnlockedContents.Where(v => v.IsActive == true && v.FileName == docfilename &&
                              v.ContentCaption == doccaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                var unlock = await _context.NGCOREJWT_UnlockedContent_MFs.Where(v => v.IsActive == true && v.DupFileName == docfilename &&
                              v.ContentCaption == doccaptions && v.CelebrityLoginFKID == celebid.UserLoginPKID).ToListAsync();

                var thumbnails = await _context.NGCOREJWT_Store_Thumbnails.Where(v => v.IsActive == true && v.FileName == docfilename &&
                             v.ContentCaptions == doccaptions && v.CelebrityFKID == celebid.UserLoginPKID).FirstOrDefaultAsync();

                if (docutcall != null && unlockcontent != null && unlock != null && thumbnails != null)
                {
                    docutcall.IsActive = false;
                    docutcall.UpdatedDate = DateTime.Now;
                    docutcall.IsDeleted = true;

                    unlockcontent.IsActive = false;
                    unlockcontent.UpdatedDate = DateTime.Now;
                    unlockcontent.IsDeleted = true;

                    foreach (var unlocks in unlock)
                    {
                        unlocks.IsActive = false;
                        unlocks.UpdatedDate = DateTime.Now;
                        unlocks.IsDeleted = true;
                    }

                    thumbnails.IsActive = false;
                    thumbnails.UpdatedDate = DateTime.Now;
                    thumbnails.IsDeleted = true;

                    result = null;
                }
                else
                {
                    result = "failure";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "DeleteCelebrity_Documents");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(result);
        }

        // Bind the user documents

        [Route("binduserdocuments/{CelebrityName}/{FollowersName}")]
        [HttpGet("{CelebrityName}/{FollowersName}")]
        public async Task<List<UserDocumentDetails>> BindUserDocuments(string CelebrityName, string FollowersName)
        {
            List<UserDocumentDetails> lockeddocs = (dynamic)null;

            var flogindocs = await GetCelebFollowersDetails(CelebrityName, FollowersName);

            lockeddocs = await (from unlock in _context.NGCOREJWT_UnlockedContent_MFs
                                join userlog in _context.NGCOREJWT_UserLogins on unlock.CelebrityLoginFKID equals userlog.UserLoginPKID
                                join fls in _context.NGCOREJWT_FollowersLogins on unlock.FollowersLoginFKID equals fls.FollowersLoginPKID
                                join price in _context.NGCOREJWT_ContentsPrices on unlock.ContentPriceFKID equals price.ContentPricePKID
                                join docstore in _context.NGCOREJWT_Store_Thumbnails on unlock.UnlockedContentFKID equals docstore.UnlockedContentFKID
                                where flogindocs.CelebrityName == CelebrityName && unlock.FollowersLoginFKID == flogindocs.FollowersLoginPKID
                                && unlock.IsActive == true && price.IsActive == true && unlock.ContentType == ".pdf"
                                && fls.FollowersName == flogindocs.FollowersName
                                group unlock by new
                                {
                                    unlock.ContentCaption,
                                    unlock.ContentDesc,
                                    unlock.FileName,
                                    unlock.IsLocked,
                                    price.ContentPricePKID,
                                    unlock.TotalCharges,
                                    unlock.IconPath,
                                    docstore.ThumbnailImage,
                                    unlock.CreatedDate
                                }
                                 into unlockdocs
                                orderby unlockdocs.Key.CreatedDate descending
                                select new UserDocumentDetails()
                                {
                                    DocumentCollectionPKID = 0,
                                    DocumentCaption = unlockdocs.Key.ContentCaption,
                                    DocumentDesc = unlockdocs.Key.ContentDesc,
                                    Createdate = Convert.ToDateTime(unlockdocs.Key.CreatedDate),
                                    FileName = unlockdocs.Key.FileName,
                                    PriceInfo = unlockdocs.Key.IsLocked == false ? Convert.ToInt32(unlockdocs.Key.TotalCharges) + "/-(Inclusive of all taxes)" : unlockdocs.Key.FileName,
                                    IsLocked = unlockdocs.Key.IsLocked == false ? "Locked" : "UnLocked",
                                    Hardcoded = unlockdocs.Key.IsLocked == false ? "UNLOCK POST @" : "",
                                    AlbumPosterPricePKID = unlockdocs.Key.ContentPricePKID,
                                    IconPath = unlockdocs.Key.IconPath,
                                    ImagePath = unlockdocs.Key.IsLocked == false ? "assets/userpdfthumbnail.jpg" : string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(unlockdocs.Key.ThumbnailImage))

                                }).ToListAsync();
            return lockeddocs;
        }

        // Get Video Unlocked Content Invoice Details

        [Route("doc_invdetails")]
        [HttpPost]
        public async Task<List<GeneratePDFInvoice_Video>> InvoiceDetails_Doc([FromForm] UnlockedPost_DocFormdata doctunlock)
        {
            var genpdfinvoice_doc = (dynamic)null;
            var checkcelebrity = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == doctunlock.CelebrityName && x.IsActive == true
                                 && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

            var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == doctunlock.FollowersName
            && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == doctunlock.CelebrityName
            && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

            int albumposterpkid = doctunlock.AlbumPostersPriceFKID;
            var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkcelebrity.UserLoginPKID
            && f.IsActive == true && f.IsLocked == true && f.ContentCaption == doctunlock.ContentCaption && f.ContentPriceFKID == albumposterpkid
            && f.FollowersLoginFKID == findfollower.FollowersLoginPKID).FirstOrDefaultAsync();

            if (updlockedcontent_mfs != null)
            {
                genpdfinvoice_doc = await (from gpinv in _context.NGCOREJWT_GenerateInvoices
                                           where gpinv.IsActive == true && gpinv.UnlockedContent_MF_FKID == updlockedcontent_mfs.UnlockedContent_MF_PKID
                                           select new GeneratePDFInvoice_Video()
                                           {
                                               FollowerEmailID = findfollower.EmailID,
                                               InvoiceNo = gpinv.InvoiceNo,
                                               InvoiceDate = Convert.ToDateTime(gpinv.CreatedDate),
                                               InvoiceDesc = "Streaming Service",
                                               Amount = gpinv.Amount,
                                               GSTCharges = gpinv.GSTCharges,
                                               ServiceCharges = gpinv.ServiceCharges,
                                               TotalCharges = gpinv.TotalCharges
                                           }).ToListAsync();

            }
            return genpdfinvoice_doc;
        }

        [Route("doct_alreadyexists/{CelebrityName}/{FollowersEmailID}/{DocCaption}")]
        [HttpGet("{CelebrityName}/{FollowersEmailID}/{DocCaption}")]
        public async Task<int> DocAlreadyExists(string CelebrityName, string FollowersEmailID, string DocCaption)
        {
            int doctcount = 0;
            var celebfollowerdet = await GetCelebFollowersDetails_UnLock(CelebrityName, FollowersEmailID);
            if (celebfollowerdet == null)
            {
                doctcount = 0;
            }
            else
            {
                doctcount = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true &&
                u.ContentCaption == DocCaption && u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();
            }

            return doctcount;
        }

        // unlock the document content from the document list - DocumentUnlockPost
        public async Task<ActionResult<UnlockedPost_DocFormdata>> SaveFollowersData_UnlockDocFormData(UnlockedPost_DocFormdata unlockpost)
        {
            try
            {
                var checkuser = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == unlockpost.CelebrityName && x.IsActive == true
                && x.PersonalityType == "Celebrity").FirstOrDefaultAsync();

                var followerscount = await _context.NGCOREJWT_FollowersLogins.Where(i => i.EmailID == unlockpost.FollowersEmailID &&
                i.IsActive == true && i.IsApproved == true && i.UserType == "Follower").CountAsync();

                var posterprice = await _context.NGCOREJWT_ContentsPrices.Where(i => i.IsActive == true &&
                i.ContentPricePKID == unlockpost.AlbumPostersPriceFKID).FirstOrDefaultAsync();

                if (followerscount == 0)
                {
                    var follower = new IdentityUser
                    {
                        Email = unlockpost.FollowersEmailID,
                        UserName = unlockpost.FollowersName,
                        // PhoneNumber = videounlockpost.FollowersMobileNumber,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(follower, "Squad$123");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(follower, "Follower");
                        addfolowerscount = 1;

                        var followerlogin = savefollowers.SaveFollowersLogin(unlockpost.FollowersName.Trim('"'), unlockpost.FollowersEmailID.Trim('"'),
                                            unlockpost.FollowersMobileNumber.Trim('"'), checkuser.UserLoginPKID);

                        await _context.NGCOREJWT_FollowersLogins.AddAsync(followerlogin);
                        await _context.SaveChangesAsync();

                        var checkfollower = await GetCelebFollowersDetails_UnLock(unlockpost.CelebrityName, unlockpost.FollowersEmailID);

                        var unlockedcontents = await _context.NGCOREJWT_UnlockedContents.Where(x => x.IsActive == true && x.CelebrityLoginFKID == checkuser.UserLoginPKID).ToListAsync();

                        foreach (var saveulmfs in unlockedcontents)
                        {
                            NGCOREJWT_UnlockedContent_MF unlockedmfs = new NGCOREJWT_UnlockedContent_MF();
                            unlockedmfs.CelebrityLoginFKID = checkuser.UserLoginPKID;
                            unlockedmfs.FollowersLoginFKID = checkfollower.FollowersLoginPKID;
                            unlockedmfs.UnlockedContentFKID = saveulmfs.UnlockedContentPKID;
                            unlockedmfs.ContentPrice = saveulmfs.ContentPrice;
                            unlockedmfs.ContentPriceFKID = saveulmfs.ContentPriceFKID;
                            unlockedmfs.ContentType = saveulmfs.ContentType;
                            unlockedmfs.ContentCaption = saveulmfs.ContentCaption;
                            unlockedmfs.ContentDesc = saveulmfs.ContentDesc;
                            unlockedmfs.IconPath = saveulmfs.IconPath1;
                            unlockedmfs.ImagePath = saveulmfs.ImagePath1;
                            unlockedmfs.DupFileName = saveulmfs.FileName;
                            unlockedmfs.IsActive = true;
                            unlockedmfs.IsDeleted = false;
                            unlockedmfs.IsLocked = false;
                            unlockedmfs.CreatedBy = "System";
                            unlockedmfs.UpdatedBy = "System";
                            unlockedmfs.CreatedDate = saveulmfs.CreatedDate;
                            unlockedmfs.UpdatedDate = DateTime.Now;
                            unlockedmfs.GSTCharges = saveulmfs.UC_GSTCharges;
                            unlockedmfs.ServiceCharges = saveulmfs.UC_ServiceCharges;
                            unlockedmfs.TotalCharges = saveulmfs.UC_TotalCharges;

                            await _context.NGCOREJWT_UnlockedContent_MFs.AddAsync(unlockedmfs);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var findfollower = await _context.NGCOREJWT_FollowersLogins.Where(f => f.FollowersName == unlockpost.FollowersName
                && f.IsActive == true && f.UserLoginFKID == _context.NGCOREJWT_UserLogins.Where(i => i.FullName == unlockpost.CelebrityName
                && i.IsActive == true && i.IsRegistered == true && i.IsApproved == true && i.PersonalityType == "Celebrity").Select(x => x.UserLoginPKID).FirstOrDefault()).FirstOrDefaultAsync();

                var updlockedcontent_mfs = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == checkuser.UserLoginPKID
               && f.IsActive == true && f.IsLocked == false && f.ContentCaption == unlockpost.ContentCaption
               && f.FollowersLoginFKID == findfollower.FollowersLoginPKID).ToListAsync();

                foreach (var uplc_mfs in updlockedcontent_mfs)
                {
                    uplc_mfs.IsLocked = true;
                    uplc_mfs.CreatedDate = DateTime.Now;
                    uplc_mfs.IconPath = "fa fa-unlock";
                    uplc_mfs.ImagePath = "assets/pdfthumbnail.jpg";
                    uplc_mfs.FileName = uplc_mfs.DupFileName;

                    // Increment the UnlockedContent count of document

                    var unlockedcount = await _context.NGCOREJWT_Store_Thumbnails.Where(i => i.IsActive == true && i.UnlockedContentFKID == uplc_mfs.UnlockedContentFKID
                                        && i.ContentCaptions == uplc_mfs.ContentCaption).FirstOrDefaultAsync();

                    if (unlockedcount != null)
                    {
                        if (unlockedcount.UnlockedContentCount == null)
                        {
                            unlockedcount.UnlockedContentCount = 0;
                        }
                        unlockedcount.UnlockedContentCount += 1;
                        unlockedcount.ContentType = uplc_mfs.ContentType;
                        unlockedcount.FileName = uplc_mfs.FileName;
                    }

                    // Payment Section

                    NGCOREJWT_PaymentSection paysection = new NGCOREJWT_PaymentSection();
                    paysection.Celebrity_FKID = findfollower.UserLoginFKID;
                    paysection.Followers_FKID = findfollower.FollowersLoginPKID;
                    paysection.ContentPrice_FKID = unlockpost.AlbumPostersPriceFKID;
                    paysection.UnlockedContentMF_FKID = uplc_mfs.UnlockedContent_MF_PKID;

                    string shares = string.Empty;
                    //decimal bank = 0;
                    //decimal finalbank_share = 0;
                    //shares = uplc_mfs.PostersPrice.Split("/-")[0];
                    //bank = Convert.ToDecimal(shares);
                    //finalbank_share = bank * 2 / 100;
                    paysection.Share_Bank = 0;

                    decimal ours = 0;
                    decimal finalours_share = 0;
                    shares = posterprice.ContentPrice.Split("/-")[0];
                    ours = Convert.ToDecimal(shares);
                    finalours_share = ours * 20 / 100;
                    paysection.Share_Ours = finalours_share;

                    decimal celebrity = 0;
                    decimal finalcelebrity_share = 0;
                    //shares = uplc_mfs.PostersPrice;
                    celebrity = Convert.ToDecimal(shares);
                    finalcelebrity_share = celebrity * 80 / 100;
                    paysection.Share_Celebrities = finalcelebrity_share;

                    paysection.ContentPrice = uplc_mfs.ContentPrice;
                    paysection.IsActive = true;
                    paysection.IsDeleted = false;
                    paysection.CreatedBy = "System";
                    paysection.UpdatedBy = "System";
                    paysection.CreatedDate = DateTime.Now;
                    paysection.UpdatedDate = DateTime.Now;

                    await _context.NGCOREJWT_PaymentSections.AddAsync(paysection);

                    // Generate Invoices

                    NGCOREJWT_GenerateInvoice geninvoice = new NGCOREJWT_GenerateInvoice();
                    geninvoice.CelebrityFKID = checkuser.UserLoginPKID;
                    geninvoice.FollowerFKID = findfollower.FollowersLoginPKID;
                    geninvoice.ContentPriceFKID = unlockpost.AlbumPostersPriceFKID;
                    geninvoice.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    geninvoice.FollowerEmailID = findfollower.EmailID;
                    geninvoice.InvoiceNo = "CS-DOC-" + uplc_mfs.UnlockedContent_MF_PKID + "_" + Convert.ToDateTime(DateTime.Now).ToString("ddMMyyyyhhmmss");
                    geninvoice.InvDescription = "Streaming Service";
                    geninvoice.Amount = Convert.ToDecimal(shares);
                    geninvoice.GSTCharges = Convert.ToDecimal(uplc_mfs.GSTCharges);
                    geninvoice.ServiceCharges = Convert.ToDecimal(uplc_mfs.ServiceCharges);
                    geninvoice.TotalCharges = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    geninvoice.IsActive = true;
                    geninvoice.IsDeleted = false;
                    geninvoice.CreatedBy = "System";
                    geninvoice.CreatedDate = DateTime.Now;

                    await _context.NGCOREJWT_GenerateInvoices.AddAsync(geninvoice);

                    // save customer payment details

                    CustPaymentDetail custPayment = new CustPaymentDetail();
                    custPayment.CelebrityFKID = checkuser.UserLoginPKID;
                    custPayment.FollowerFKID = findfollower.FollowersLoginPKID;
                    custPayment.UnlockedContent_MF_FKID = uplc_mfs.UnlockedContent_MF_PKID;
                    custPayment.Amount = Convert.ToDecimal(uplc_mfs.TotalCharges);
                    custPayment.Currency = "INR";
                    custPayment.PaymentCapture = 1;
                    custPayment.RzpayOrderID = unlockpost.RzpayOrderID;
                    custPayment.RzpayPaymentID = unlockpost.RzpayPaymentID;
                    custPayment.RzpaySignature = unlockpost.RzpaySignature;
                    custPayment.PaymentStatus = "Success";
                    custPayment.IsActive = true;
                    custPayment.IsDeleted = false;
                    custPayment.CreatedDate = DateTime.Now;

                    await _context.CustPaymentDetails.AddAsync(custPayment);
                }

                await _context.SaveChangesAsync();
                if (addfolowerscount == 0)
                {
                    await EmailNotification("Dear " + unlockpost.CelebrityName + ", your follower named " + unlockpost.FollowersName + " has unlocked a PDF Document ", unlockpost.ContentCaption, " Unlocked your " + unlockpost.ContentCaption + " (Post @ " + unlockpost.PriceInfo + ")", addfolowerscount, unlockpost.CelebrityName);
                }
                else
                {
                    await EmailNotification("Dear " + unlockpost.CelebrityName + ", you got a new follower named " + unlockpost.FollowersName, unlockpost.ContentCaption, " Unlocked your " + unlockpost.ContentCaption + " (Post @ " + unlockpost.PriceInfo + ")", addfolowerscount, unlockpost.CelebrityName);
                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockDocumentPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        // register the user and unlocked the document (.pdf) post
        [Route("doc_unlockedpost")]
        [HttpPost]
        public async Task<JsonResult> DocumentUnlockPost([FromForm] UnlockedPost_DocFormdata unlockpost)
        {
            string doc_alertmessage = string.Empty;
            try
            {

                //var celebfollowerdet = await GetCelebFollowersDetails_UnLock(unlockpost.CelebrityName, unlockpost.FollowersEmailID);

                //if (celebfollowerdet == null)
                //{
                //    // payment gateway
                //    doc_alertmessage = "Payment Gateway";
                //}
                //else
                //{
                //    int doc_alreadyexist = await _context.NGCOREJWT_UnlockedContent_MFs.Where(u => u.IsActive == true && u.IsLocked == true && u.ContentCaption == unlockpost.ContentCaption
                //    && u.CelebrityLoginFKID == celebfollowerdet.CelebrityFKID && u.FollowersLoginFKID == celebfollowerdet.FollowersLoginPKID).CountAsync();

                //    if (doc_alreadyexist == 0)
                //    {
                var usercount = await _context.NGCOREJWT_UserLogins.Where(x => x.FullName == unlockpost.CelebrityName
                       && x.IsActive == true && x.IsRegistered == true && x.PersonalityType == "Celebrity").CountAsync();

                if (usercount == 1)
                {
                    await SaveFollowersData_UnlockDocFormData(unlockpost);
                }
                doc_alertmessage = "Not Exists";
                //    }
                //    else
                //    {
                //        doc_alertmessage = "Already Exists";
                //    }
                //}
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "UnlockDocumentPost_ByFollower");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(doc_alertmessage);
        }

        // Unlocked Document - Followers Unlocked Document Count for Celebrity View

        [Route("groupbydoc_count/{FullName}")]
        [HttpGet("{FullName}")]
        public async Task<ActionResult<IEnumerable<UnlockDocCount>>> GroupbyDoc_Celebrity(string FullName)
        {
            var doc_count = await (from dstore in _context.NGCOREJWT_Store_Thumbnails
                                   join usrlogin in _context.NGCOREJWT_UserLogins on dstore.CelebrityFKID equals usrlogin.UserLoginPKID
                                   join price in _context.NGCOREJWT_ContentsPrices on dstore.ContentPriceFKID equals price.ContentPricePKID
                                   where dstore.IsActive == true && usrlogin.FullName == FullName && dstore.ContentType == ".pdf"
                                   && usrlogin.IsApproved == true && usrlogin.IsRegistered == true && usrlogin.PersonalityType == "Celebrity"
                                   orderby dstore.CreatedDate descending
                                   select new UnlockDocCount()
                                   {
                                       DocCount = dstore.UnlockedContentCount,
                                       DocCaption = dstore.ContentCaptions,
                                       FileName = dstore.FileName,
                                       DocThumbnail = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(dstore.ThumbnailImage)),
                                       DocPriceInfo = price.ContentPrice
                                   }).ToListAsync();

            return doc_count;
        }

        // Razor Payment Gateway

        [Route("userpayments")]
        [HttpPost]
        public async Task<JsonResult> InsertPayments([FromForm] InsertPayments payments)
        {

            OrderModel order = new OrderModel()
            {
                OrderAmount = payments.Amount,
                Currency = payments.Currency,
                Payment_Capture = 0,    // 0 - Manual capture, 1 - Auto capture
                Notes = new Dictionary<string, string>()
                {
                    { "note 1", "first note while creating order" }, { "note 2", "you can add max 15 notes" },
                    { "note for account 1", "this is a linked note for account 1" }, { "note 2 for second transfer", "it's another note for 2nd account" }
                }
            };
            var orderId = await CreateTransfersViaOrder(order);
            return new JsonResult(orderId);
        }

        private async Task<string> CreateTransfersViaOrder(OrderModel order)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_appSettings._Key, _appSettings._Secret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", order.OrderAmountInSubUnits);
                options.Add("currency", order.Currency);
                options.Add("payment_capture", order.Payment_Capture);
                options.Add("notes", order.Notes);

                Order orderResponse = client.Order.Create(options);
                var orderId = orderResponse.Attributes["id"].ToString();
                return orderId;
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "PaymentGateway_OrderCreation");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
                return null;
            }
        }

        [Route("afteruserpayment")]
        [HttpPost]
        public async Task<JsonResult> AfterPayment([FromForm] AfterPayment afterPayment)
        {
            var paymentStatus = afterPayment.RzStatus;
            try
            {
                if (paymentStatus == "Fail")
                    return new JsonResult(paymentStatus);

                var orderId = afterPayment.RzOrderID;
                var paymentId = afterPayment.RzPaymentID;
                var signature = afterPayment.RzSignature;

                var validSignature = CompareSignatures(orderId, paymentId, signature);
                if (validSignature)
                {
                    var paystatus = CapturePayment(paymentId);
                    return new JsonResult(paystatus);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "AfterPayment");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(paymentStatus);
        }

        [Route("paymentfailure")]
        [HttpPost]
        public async Task<IActionResult> PaymentFailures([FromForm] PaymentFailure paymentFailure)
        {
            var getdetails = (dynamic)null;
            var chkcelebrity = (dynamic)null;
            try
            {
                var chkcelebfoll = await GetCelebFollowersDetails_UnLock(paymentFailure.CelebrityName, paymentFailure.FollowersEmailID);
                if (chkcelebfoll != null)
                {
                    getdetails = await _context.NGCOREJWT_UnlockedContent_MFs.Where(f => f.CelebrityLoginFKID == chkcelebfoll.CelebrityFKID
                    && f.IsActive == true && f.IsLocked == false && f.ContentCaption == paymentFailure.ContentCaptions
                    && f.FollowersLoginFKID == chkcelebfoll.FollowersLoginPKID).FirstOrDefaultAsync();
                }
                else
                {
                    chkcelebrity = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == paymentFailure.CelebrityName && i.IsActive == true
                    && i.IsApproved == true && i.PersonalityType == "Celebrity").FirstOrDefaultAsync();
                }
                if (getdetails != null)
                {
                    CustPaymentDetail custPayment = new CustPaymentDetail();
                    custPayment.CelebrityFKID = getdetails.CelebrityLoginFKID;
                    custPayment.FollowerFKID = getdetails.FollowersLoginFKID;
                    custPayment.UnlockedContent_MF_FKID = getdetails.UnlockedContent_MF_PKID;
                    custPayment.Amount = Convert.ToDecimal(getdetails.TotalCharges);
                    custPayment.Currency = "INR";
                    custPayment.PaymentCapture = 0;
                    custPayment.RzpayOrderID = paymentFailure.RazOrderID;
                    custPayment.RzpayPaymentID = paymentFailure.RazPaymentID;
                    custPayment.RzpaySignature = "no signature";
                    custPayment.PaymentStatus = paymentFailure.RazStatus;
                    custPayment.IsActive = true;
                    custPayment.IsDeleted = false;
                    custPayment.CreatedDate = DateTime.Now;

                    await _context.CustPaymentDetails.AddAsync(custPayment);
                    await _context.SaveChangesAsync();

                    //string numbers = "9488063196,8344877692"; // in a comma seperated list
                    //string message = "Hi, Payment Failure to " + chkcelebfoll.FollowersName + " of INR " + Convert.ToDecimal(getdetails.TotalCharges);

                    //viasms.SendviaSMS(numbers, message);
                }

            }
            catch (Exception ex)
            {
                var exceptions = exceptionlog.SendExcepToDB(ex, "PaymentFailure");
                _context.NGCOREJWT_Exception_ErrorLogs.Add(exceptions);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        private bool CompareSignatures(string orderId, string paymentId, string razorPaySignature)
        {
            var text = orderId + "|" + paymentId;
            var secret = _appSettings._Secret;
            var generatedSignature = CalculateSHA256(text, secret);
            if (generatedSignature == razorPaySignature)
                return true;
            else
                return false;
        }

        private string CalculateSHA256(string text, string secret)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(text),
            baSalt = enc.GetBytes(secret);
            HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;
        }

        public string CapturePayment(string paymentId)
        {
            string result = string.Empty;
            RazorpayClient client = new RazorpayClient(_appSettings._Key, _appSettings._Secret);
            Payment payment = client.Payment.Fetch(paymentId);
            var amount = payment.Attributes["amount"];
            var currency = payment.Attributes["currency"];

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amount);
            options.Add("currency", currency);
            Payment paymentCaptured = payment.Capture(options);
            result = "Success";
            return result;
        }

        // Find the celebrity

        [Route("findcelebrity/{celebname}")]
        [HttpGet("{celebname}")]
        public async Task<IActionResult> FindtheCelebrity(string celebname)
        {
            string response = string.Empty;
            var fdceleb = await _context.NGCOREJWT_UserLogins.Where(i => i.FullName == celebname && i.IsActive == true && i.IsApproved == true
            && i.IsRegistered == true && i.PersonalityType == "Celebrity").FirstOrDefaultAsync();
            if (fdceleb != null)
            {
                response = "Success";
            }
            else
            {
                response = "Fail";
            }
            return new JsonResult(response);
        }
    }
}
