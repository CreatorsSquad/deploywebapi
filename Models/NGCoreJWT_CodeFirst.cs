using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    // UserLogin
    public class NGCOREJWT_UserLogin
    {
        [Key]
        public int UserLoginPKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FullName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string EmailID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ProfileBio { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string MobileNumber { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PersonalityType { get; set; }
        public int? OTP { get; set; }
        public int? ViewersVisitCount { get; set; }
        public int? FollowersCount { get; set; }

        [Column("IsCheckedOTP", TypeName = "TINYINT")]
        public bool? IsCheckedOTP { get; set; }

        [Column("IsRegistered", TypeName = "TINYINT")]
        public bool? IsRegistered { get; set; }

        [Column("IsApproved", TypeName = "TINYINT")]
        public bool? IsApproved { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // UserProfileDetails
    public class NGCOREJWT_UserProfileDetail
    {
        [Key]
        public int UserProfilePKID { get; set; }
        public int? UserLoginFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // UserService
    public class NGCOREJWT_UserService
    {
        [Key]
        public int UserServicePKID { get; set; }
        public int? UserLoginFKID { get; set; }
        public int? PosterTitleFKID { get; set; }
        public int? PosterPriceFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string NewService { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string Price { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // PosterTitle
    public class NGCOREJWT_PosterTitle
    {
        [Key]
        public int PosterTitlePKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PostersTitle { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // ContentPrice
    public class NGCOREJWT_ContentsPrice
    {
        [Key]
        public int ContentPricePKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentPrice { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "varchar(4000)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "varchar(4000)")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // My Video Collections
    public class NGCOREJWT_VideoCollection
    {
        [Key]
        public int VideoCollectionPKID { get; set; }
        public int? UserLoginFKID { get; set; }
        public int? ContentPriceFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string VideoCaption { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string VideoDesc { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column("IsLocked", TypeName = "TINYINT")]
        public bool? IsLocked { get; set; }

        [Column(TypeName = "LONGBLOB")]
        public byte[] VideoData { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Video_GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Video_ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Video_TotalCharges { get; set; }
    }

    // My Audio Collections
    public class NGCOREJWT_AudioCollection
    {
        [Key]
        public int AudioCollectionPKID { get; set; }
        public int? UserLoginFKID { get; set; }
        public int? ContentPriceFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AudioCaption { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AudioDesc { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column("IsLocked", TypeName = "TINYINT")]
        public bool? IsLocked { get; set; }

        [Column(TypeName = "LONGBLOB")]
        public byte[] AudioData { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Audio_GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Audio_ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Audio_TotalCharges { get; set; }
    }

    // My Document Collections
    public class NGCOREJWT_DocumentCollection
    {
        [Key]
        public int DocCollectionPKID { get; set; }
        public int? UserLoginFKID { get; set; }
        public int? ContentPriceFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string DocumentCaption { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string DocumentDesc { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column("IsLocked", TypeName = "TINYINT")]
        public bool? IsLocked { get; set; }

        [Column(TypeName = "LONGBLOB")]
        public byte[] DocumentData { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "varchar(4000)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "varchar(4000)")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Doct_GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Doct_ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Doct_TotalCharges { get; set; }
    }

    // FollowersLogin
    public class NGCOREJWT_FollowersLogin
    {
        [Key]
        public int FollowersLoginPKID { get; set; }
        public int? UserLoginFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FollowersName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string EmailID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string MobileNumber { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UserType { get; set; }
        public int? OTP { get; set; }

        [Column("IsCheckedOTP", TypeName = "TINYINT")]
        public bool? IsCheckedOTP { get; set; }

        [Column("IsRegistered", TypeName = "TINYINT")]
        public bool? IsRegistered { get; set; }

        [Column("IsApproved", TypeName = "TINYINT")]
        public bool? IsApproved { get; set; }

        [Column("IsSubscribed", TypeName = "TINYINT")]
        public bool? IsSubscribed { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // UnlockedContent
    public class NGCOREJWT_UnlockedContent
    {
        [Key]
        public int UnlockedContentPKID { get; set; }
        public int? CelebrityLoginFKID { get; set; }
        public int? ContentPriceFKID { get; set; }
        public int? ContentCollectionFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentCaption { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentDesc { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentPrice { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string IconPath { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ImagePath { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string IconPath1 { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ImagePath1 { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsLocked", TypeName = "TINYINT")]
        public bool? IsLocked { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UC_GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UC_ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? UC_TotalCharges { get; set; }
    }

    // Unlocked Contents - Multiple Followers
    public class NGCOREJWT_UnlockedContent_MF
    {
        [Key]
        public int UnlockedContent_MF_PKID { get; set; }
        public int? CelebrityLoginFKID { get; set; }
        public int? FollowersLoginFKID { get; set; }
        public int? UnlockedContentFKID { get; set; }
        public int? ContentPriceFKID { get; set; }

        [Column("IsLiked", TypeName = "TINYINT")]
        public bool? IsLiked { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentPrice { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentCaption { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentDesc { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string IconPath { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ImagePath { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string DupFileName { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsLocked", TypeName = "TINYINT")]
        public bool? IsLocked { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalCharges { get; set; }
    }

    // Personal EmailID's
    public class NGCOREJWT_PersonalEmailID
    {
        [Key]
        public int PersonalEmailID_PKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PEmailID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PMobileno { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // PaymentSection
    public class NGCOREJWT_PaymentSection
    {
        [Key]
        public int PaymentSection_PKID { get; set; }
        public int? Celebrity_FKID { get; set; }
        public int? Followers_FKID { get; set; }
        public int? ContentPrice_FKID { get; set; }
        public int? UnlockedContentMF_FKID { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Share_Bank { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Share_Ours { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Share_Celebrities { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentPrice { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // Payout Billing
    public class NGCOREJWT_PayoutBilling
    {
        [Key]
        public int PayoutBillingPKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? AccountTypeFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PanCardNo { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AadhaarCardNo { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string BillingName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpiAddress { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string IfscCode { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AccountNo { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AccountName { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // Account Type
    public class NGCOREJWT_AccountsType
    {
        [Key]
        public int AccountsTypePKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AccountType { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    // Store the Thumbnail Image
    public class NGCOREJWT_Store_Thumbnail
    {
        [Key]
        public int ThumbnailImagePKID { get; set; }
        public int? UnlockedContentFKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? ContentPriceFKID { get; set; }
        public int? UnlockedContentCount { get; set; }
        public int? UC_LikedCount { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FileName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ThumbnailPath { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ContentCaptions { get; set; }

        [Column(TypeName = "LONGBLOB")]
        public byte[] ThumbnailImage { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }
    }

    //  EmailNotification
    public class NGCOREJWT_Email_Notification
    {
        [Key]
        public int EmailPKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? FollowerFKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string SenderEmail { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string Recepients { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string Subject { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string MessageBody { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

    //  ErrorLog
    public class NGCOREJWT_Exception_ErrorLog
    {
        [Key]
        public int ErrorLogPKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ExceptionMessage { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ExceptionType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ExceptionSource { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string ExceptionURL { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

    //  Generate Invoice
    public class NGCOREJWT_GenerateInvoice
    {
        [Key]
        public int InvoicePKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? FollowerFKID { get; set; }
        public int? ContentPriceFKID { get; set; }
        public int? UnlockedContent_MF_FKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string FollowerEmailID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string InvoiceNo { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string InvDescription { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Amount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GSTCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ServiceCharges { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalCharges { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

    //  Suggest Comments
    public class NGCOREJWT_SuggestComment
    {
        [Key]
        public int CommentsPKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? FollowerFKID { get; set; }
        public int? UnlockedContent_MF_FKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string Comments { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

    //  UserInformation Logs 
    public class NGCOREJWT_Log
    {
        [Key]
        public int LogsPKID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UserName { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UserType { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UserEmailID { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LoginDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LogoutDate { get; set; }
    }

    //  Payment Details
    public class CustPaymentDetail
    {
        [Key]
        public int PaymentDetailPKID { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? FollowerFKID { get; set; }
        public int? UnlockedContent_MF_FKID { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Amount { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string Currency { get; set; }
        public int? PaymentCapture { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string RzpayOrderID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string RzpayPaymentID { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string RzpaySignature { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string PaymentStatus { get; set; }

        [Column("IsActive", TypeName = "TINYINT")]
        public bool? IsActive { get; set; }

        [Column("IsDeleted", TypeName = "TINYINT")]
        public bool? IsDeleted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

    //  Store OTP
    public class StoreOTP
    {
        [Key]
        public int StoreOTP_PKID { get; set; }
        public int? OneTimePassword { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string UserType { get; set; }
        public int? CelebrityFKID { get; set; }
        public int? FollowerFKID { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
    }

}
