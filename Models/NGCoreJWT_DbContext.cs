using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class NGCoreJWT_DbContext : IdentityDbContext<IdentityUser>
    {
        public NGCoreJWT_DbContext(DbContextOptions<NGCoreJWT_DbContext> options) : base(options)
        {
        }

        // Creating Roles for our application

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                    new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                    new { Id = "2", Name = "Celebrity", NormalizedName = "CELEBRITY" },
                    new { Id = "3", Name = "Follower", NormalizedName = "FOLLOWER" }
                );
        }

        public virtual DbSet<NGCOREJWT_UserLogin> NGCOREJWT_UserLogins { get; set; }
        public virtual DbSet<NGCOREJWT_UserProfileDetail> NGCOREJWT_UserProfileDetails { get; set; }
        public virtual DbSet<NGCOREJWT_UserService> NGCOREJWT_UserServices { get; set; }
        public virtual DbSet<NGCOREJWT_PosterTitle> NGCOREJWT_PosterTitles { get; set; }
        public virtual DbSet<NGCOREJWT_ContentsPrice> NGCOREJWT_ContentsPrices { get; set; }
        public virtual DbSet<NGCOREJWT_VideoCollection> NGCOREJWT_VideoCollections { get; set; }
        public virtual DbSet<NGCOREJWT_AudioCollection> NGCOREJWT_AudioCollections { get; set; }
        public virtual DbSet<NGCOREJWT_DocumentCollection> NGCOREJWT_DocumentCollections { get; set; }
        public virtual DbSet<NGCOREJWT_FollowersLogin> NGCOREJWT_FollowersLogins { get; set; }
        public virtual DbSet<NGCOREJWT_UnlockedContent> NGCOREJWT_UnlockedContents { get; set; }
        public virtual DbSet<NGCOREJWT_UnlockedContent_MF> NGCOREJWT_UnlockedContent_MFs { get; set; }
        public virtual DbSet<NGCOREJWT_PersonalEmailID> NGCOREJWT_PersonalEmailIDs { get; set; }
        public virtual DbSet<NGCOREJWT_PaymentSection> NGCOREJWT_PaymentSections { get; set; }
        public virtual DbSet<NGCOREJWT_PayoutBilling> NGCOREJWT_PayoutBillings { get; set; }
        public virtual DbSet<NGCOREJWT_AccountsType> NGCOREJWT_AccountsTypes { get; set; }
        public virtual DbSet<NGCOREJWT_Store_Thumbnail> NGCOREJWT_Store_Thumbnails { get; set; }
        public virtual DbSet<NGCOREJWT_Email_Notification> NGCOREJWT_Email_Notifications { get; set; }
        public virtual DbSet<NGCOREJWT_Exception_ErrorLog> NGCOREJWT_Exception_ErrorLogs { get; set; }
        public virtual DbSet<NGCOREJWT_GenerateInvoice> NGCOREJWT_GenerateInvoices { get; set; }
        public virtual DbSet<NGCOREJWT_SuggestComment> NGCOREJWT_SuggestComments { get; set; }
        public virtual DbSet<NGCOREJWT_Log> NGCOREJWT_Logs { get; set; }
        public virtual DbSet<CustPaymentDetail> CustPaymentDetails { get; set; }
        public virtual DbSet<StoreOTP> StoreOTPs { get; set; }


    }
}
