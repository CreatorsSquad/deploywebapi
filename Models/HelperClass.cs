using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Models
{
    public class InsertUserDetails
    {
        public string FullName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public string ProfileBio { get; set; }
        public int? FollowersCount { get; set; }
        public string YoutubeLink { get; set; }

    }

    public class InsertFollowersInfo
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
    }

    public class UpdateCP_formdata
    {
        public IFormFile FileName { get; set; }
        public string Profilebio { get; set; }
        public string Username { get; set; }
        public string CreatedBy { get; set; }
    }

    public class InsertImageVideo
    {
        public IFormFile FileName { get; set; }
        public string FullName { get; set; }

    }

    public class InsertServicePost
    {
        public string FullName { get; set; }
        public string NewService { get; set; }
        public string Price { get; set; }
        public int PosterTitleFKID { get; set; }
        public int PosterPriceFKID { get; set; }
        public int UserServicePKID { get; set; }

    }

    public class InsertArtistCollection
    {
        public string FullName { get; set; }
        public IFormFile FileName { get; set; }
        public string MembershipTitle { get; set; }
        public string TitleDesc { get; set; }
        public int PosterPriceFKID { get; set; }
        public int UserLoginFKID { get; set; }
        public int ArtistCollectionPKID { get; set; }
        public string PostersPrice { get; set; }
        public string ImgFileName { get; set; }

    }

    public class AlbumDetails
    {
        public IFormFile[] FileName { get; set; }
        public int PosterPriceFKID { get; set; }
        public string AlbumCaption { get; set; }
        public string AlbumDesc { get; set; }
        public string UserName { get; set; }
        public string PostersPrice { get; set; }

    }

    public class AlbumDet
    {
        public int AlbumCollectionPKID { get; set; }
        public int AlbumCount { get; set; }
        public string AlbumCaption { get; set; }
        public string AlbumDesc { get; set; }
        public string Imagedata { get; set; }
    }

    public class VideoDetails
    {
        public int VideoCollectionPKID { get; set; }
        public int VideoCount { get; set; }
        public string VideoCaption { get; set; }
        public string VideoDesc { get; set; }
        public string FileName { get; set; }
        public DateTime Createdate { get; set; }
        public decimal? PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int AlbumPosterPricePKID { get; set; }
        public string VideoThumbnail { get; set; }
    }

    public class ImageData
    {
        public string Imagesdata { get; set; }
        public string ContentCaption { get; set; }

    }

    public class VideoFormData
    {
        public IFormFile VideoFileName { get; set; }
        public IFormFile Thumbnail { get; set; }
        public int AlbumPosterPriceFKID { get; set; }
        public string VideoCaption { get; set; }
        public string VideoDesc { get; set; }
        public string UserName { get; set; }
        public string PostersPrice { get; set; }
        public decimal? V_GSTCharges { get; set; }
        public decimal? V_ServiceCharges { get; set; }
        public decimal? V_TotalCharges { get; set; }

    }

    public class AudioFormData
    {
        public IFormFile AudioFileName { get; set; }
        public IFormFile AudioThumbnail { get; set; }
        public int AlbumPosterPriceFKID { get; set; }
        public string AudioCaption { get; set; }
        public string AudioDesc { get; set; }
        public string UserName { get; set; }
        public string PostersPrice { get; set; }
        public decimal? A_GSTCharges { get; set; }
        public decimal? A_ServiceCharges { get; set; }
        public decimal? A_TotalCharges { get; set; }
    }

    public class AudioCollDetails
    {
        public string AudioCaption { get; set; }
        public string AudioDesc { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
    }

    public class DocumentFormData
    {
        public IFormFile DocFilename { get; set; }
        public IFormFile DocThumbnailname { get; set; }
        public int AlbumPosterPriceFKID { get; set; }
        public string DocumentCaption { get; set; }
        public string DocumentDesc { get; set; }
        public string UserName { get; set; }
        public string PostersPrice { get; set; }
        public decimal? D_GSTCharges { get; set; }
        public decimal? D_ServiceCharges { get; set; }
        public decimal? D_TotalCharges { get; set; }
    }

    public class DocumentCollDetails
    {
        public string DocumentCaption { get; set; }
        public string DocumentDesc { get; set; }
        public List<string> FilePath { get; set; }
    }

    public class Documentshowdet
    {
        public string DocumentCaption { get; set; }
        public string DocumentDesc { get; set; }
        public string FileName { get; set; }
        public DateTime Createdate { get; set; }
        public decimal? PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int AlbumPosterPricePKID { get; set; }
        public int DocumentCollectionPKID { get; set; }
        public string DocThumbnail { get; set; }

    }

    public class Audioshowdet
    {
        public string AudioCaption { get; set; }
        public string AudioDesc { get; set; }
        public string FileName { get; set; }
        public DateTime Createdate { get; set; }
        public decimal? PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int AlbumPosterPricePKID { get; set; }
        public int AudioCollectionPKID { get; set; }
        public string AudioThumbnail { get; set; }

    }

    public class Albumshowdet
    {
        public int AlbumCount { get; set; }
        public string AlbumCaption { get; set; }
        public string AlbumDesc { get; set; }
        public string FileName { get; set; }
        public DateTime Createdate { get; set; }
        public string PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int AlbumPosterPricePKID { get; set; }
        public int AlbumCollectionPKID { get; set; }

    }

    public class GetDocuments_Popup
    {
        public string DocumentCaption { get; set; }
        public string PriceInfo { get; set; }
        public int? DocumentCollPKID { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
    }
    public class GetAudioDetails_Popup
    {
        public string AudioCaption { get; set; }
        public string PriceInfo { get; set; }
        public int? AudioCollectionPKID { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
    }
    public class GetVideoDetails_Popup
    {
        public string VideoCaption { get; set; }
        public string PriceInfo { get; set; }
        public int? VideoCollectionPKID { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
    }

    public class GetAlbumDetails_Popup
    {
        public string AlbumCaption { get; set; }
        public string PriceInfo { get; set; }
        public int AlbumCollectionPKID { get; set; }
        public int PosterPricePKID { get; set; }
    }

    public class UnlockedPost_DocFormdata
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string FollowersEmailID { get; set; }
        public string FollowersMobileNumber { get; set; }
        public int AlbumPostersPriceFKID { get; set; }
        public int ContentCollectionFKID { get; set; }
        public string ContentCaption { get; set; }
        public string PriceInfo { get; set; }
        public string RzpayOrderID { get; set; }
        public string RzpayPaymentID { get; set; }
        public string RzpaySignature { get; set; }
    }

    public class UserDocumentDetails
    {
        public string DocumentCaption { get; set; }
        public string DocumentDesc { get; set; }
        public DateTime Createdate { get; set; }
        public string PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
        public int? DocumentCollectionPKID { get; set; }
        public string FileName { get; set; }
        public string IconPath { get; set; }
        public string ImagePath { get; set; }
        public string Hardcoded { get; set; }
    }

    public class UnlockedPost_VideoFormdata
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string FollowersEmailID { get; set; }
        public string FollowersMobileNumber { get; set; }
        public int AlbumPostersPriceFKID { get; set; }
        public int ContentCollectionFKID { get; set; }
        public string ContentCaption { get; set; }
        public string PriceInfo { get; set; }
        public string RzpayOrderID { get; set; }
        public string RzpayPaymentID { get; set; }
        public string RzpaySignature { get; set; }
    }

    public class UserVideoDetails
    {
        public string VideoCaption { get; set; }
        public string VideoDesc { get; set; }
        public DateTime Createdate { get; set; }
        public string PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
        public int? VideoCollectionPKID { get; set; }
        public string FileName { get; set; }
        public string IconPath { get; set; }
        public string ImagePath { get; set; }
        public string Hardcoded { get; set; }
    }
    public class UnlockedPost_AudioFormdata
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string FollowersEmailID { get; set; }
        public string FollowersMobileNumber { get; set; }
        public int AlbumPostersPriceFKID { get; set; }
        public int ContentCollectionFKID { get; set; }
        public string ContentCaption { get; set; }
        public string PriceInfo { get; set; }
        public string RzpayOrderID { get; set; }
        public string RzpayPaymentID { get; set; }
        public string RzpaySignature { get; set; }
    }

    public class UserAudioDetails
    {
        public string AudioCaption { get; set; }
        public string AudioDesc { get; set; }
        public DateTime Createdate { get; set; }
        public string PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
        public int? AudioCollectionPKID { get; set; }
        public string FileName { get; set; }
        public string IconPath { get; set; }
        public string ImagePath { get; set; }
        public string Hardcoded { get; set; }
    }

    public class UnlockedPostPhoto_Formdata
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string FollowersEmailID { get; set; }
        public string FollowersMobileNumber { get; set; }
        public int AlbumPostersPriceFKID { get; set; }
        public int ContentCollectionFKID { get; set; }
        public string ContentCaption { get; set; }
        public string PriceInfo { get; set; }
    }

    public class UserPhotoAlbumDetails
    {
        public string AlbumCaption { get; set; }
        public string AlbumDesc { get; set; }
        public DateTime Createdate { get; set; }
        public string PriceInfo { get; set; }
        public string IsLocked { get; set; }
        public int? AlbumPosterPricePKID { get; set; }
        public int? AlbumCollectionPKID { get; set; }
        public string FileName { get; set; }
        public string IconPath { get; set; }
        public string ImagePath { get; set; }
        public string Hardcoded { get; set; }
    }

    public class DashboardStat
    {
        public DateTime? LastPost { get; set; }
        public int? ViewersCount { get; set; }
        public decimal? CelebrityShare { get; set; }
        public int? FollowersCount { get; set; }
    }

    public class SubscriberCheck
    {
        public string IsSubscribed { get; set; }
        public bool? TruerFalse { get; set; }

    }

    public class ImageModel
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }

    public class PayoutBillingFormData
    {
        public string PanCardNo { get; set; }
        public string AadhaarCardNo { get; set; }
        public string BillingName { get; set; }
        public string UpiAddress { get; set; }
        public string IfscCode { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public int? AccountsTypePKID { get; set; }
        public string CelebrityName { get; set; }
    }

    public class FillPayoutBillings
    {
        public string PanCardNo { get; set; }
        public string AadhaarCardNo { get; set; }
        public string BillingName { get; set; }
        public string UpiAddress { get; set; }
        public string IfscCode { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public int? AccountsTypePKID { get; set; }
    }

    public class UpdatePayoutBillingDet
    {
        public string UpiAddress { get; set; }
        public string IfscCode { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public int? AccountsTypePKID { get; set; }
        public string CelebrityName { get; set; }
    }

    public class UnlockedContentCount
    {
        public int? UnlockedVideoCount { get; set; }
        public int? UnlockedAudioCount { get; set; }
        public int? UnlockedDocCount { get; set; }
    }

    public class UnlockVideoCount
    {
        public string VideoCaption { get; set; }
        public int? VideoCount { get; set; }
        public int? VideoCollectionPKID { get; set; }
        public string FileName { get; set; }
        public string VideoThumbnail { get; set; }
        public string VideoPriceInfo { get; set; }
        public int? VLikedCount { get; set; }
    }

    public class UnlockAudioCount
    {
        public string AudioCaption { get; set; }
        public int? AudioCount { get; set; }
        public string FileName { get; set; }
        public string AudioThumbnail { get; set; }
        public string AudioPriceInfo { get; set; }
        public int? ALikedCount { get; set; }

    }

    public class UnlockDocCount
    {
        public string DocCaption { get; set; }
        public int? DocCount { get; set; }
        public string FileName { get; set; }
        public string DocThumbnail { get; set; }
        public string DocPriceInfo { get; set; }

    }

    public class Celebrity_FollowersName
    {
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
        public string EmailID { get; set; }
        public int? FollowersLoginPKID { get; set; }
        public int? CelebrityFKID { get; set; }
    }

    public class Follower_Notification_Details
    {
        public string ThumbnailImage { get; set; }
        public string ContentCaptions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ContentType { get; set; }
        public string VidAudFileName { get; set; }
        public bool? ContentIsLocked { get; set; }
    }

    public class Celebrity_Notification_Details
    {
        public string ThumbnailImage { get; set; }
        public string ContentCaptions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ContentType { get; set; }
        public string VidAudFileName { get; set; }

    }

    public class Edit_VideoCap_Price_Celeb
    {
        public string VideoCaption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PostersPrice { get; set; }
    }

    public class Edit_AudioCap_Price_Celeb
    {
        public string AudioCaption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PostersPrice { get; set; }
    }

    public class Edit_DocCap_Price_Celeb
    {
        public string Doc_Caption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PostersPrice { get; set; }
    }

    public class Update_VideoCap_Price_Celeb
    {
        public string CelebrityName { get; set; }
        public string VideoCaption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PriceInfo { get; set; }
        public string VideoFileName { get; set; }
        public decimal? Video_GSTCharges { get; set; }
        public decimal? Video_ServiceCharges { get; set; }
        public decimal? Video_TotalCharges { get; set; }

    }

    public class Update_AudioCap_Price_Celeb
    {
        public string CelebrityName { get; set; }
        public string AudioCaption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PriceInfo { get; set; }
        public string AudioFileName { get; set; }
        public decimal? Audio_GSTCharges { get; set; }
        public decimal? Audio_ServiceCharges { get; set; }
        public decimal? Audio_TotalCharges { get; set; }
    }

    public class Update_DoctCap_Price_Celeb
    {
        public string CelebrityName { get; set; }
        public string Doc_Caption { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string PriceInfo { get; set; }
        public string DoctFileName { get; set; }
        public decimal? Doct_GSTCharges { get; set; }
        public decimal? Doct_ServiceCharges { get; set; }
        public decimal? Doct_TotalCharges { get; set; }
    }

    public class GetCelebrity_Follower_Details
    {
        public int? CelebrityFKID { get; set; }
        public int? FollowersFKID { get; set; }
        public bool? IsSubscribed { get; set; }

    }

    public class GeneratePDFInvoice_Video
    {
        public string FollowerEmailID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDesc { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? GSTCharges { get; set; }
        public decimal? ServiceCharges { get; set; }
        public decimal? TotalCharges { get; set; }
    }

    public class SendPDF_Email
    {
        public IFormFile FileName_Type { get; set; }
        public string CelebrityName { get; set; }
        public string FollowerEmailID { get; set; }
    }

    public class Update_UC_LikedVideoCount
    {
        public string VideoCaptions { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
    }

    public class GetVideo_LikedCount
    {
        public int? UC_LikedCount { get; set; }
        public bool? IsLiked { get; set; }
    }

    public class Update_UC_LikedAudioCount
    {
        public string AudioCaptions { get; set; }
        public int? PosterPriceFKID { get; set; }
        public string CelebrityName { get; set; }
        public string FollowersName { get; set; }
    }

    public class GetAudio_LikedCount
    {
        public int? UC_ALikedCount { get; set; }
        public bool? AIsLiked { get; set; }
    }

    public class ArtistViewModel
    {
        public string Username { get; set; }
        public int OTP { get; set; }
    }

    public class FollowerViewModel
    {
        public string FollowersName { get; set; }
        public string FollowersEmailID { get; set; }
        public int OTP { get; set; }
    }

    public class Follower_GenerateOTP
    {
        public string FollowersEmailID { get; set; }
        public string Username { get; set; }
    }

    public class Celebrity_GenerateOTP
    {
        public string CelebrityName { get; set; }
        public int OTP { get; set; }
    }

    public class VideoSuggestComments
    {
        public string CelebrityName { get; set; }
        public string Followersname { get; set; }
        public string VideoComments { get; set; }
        public string VideoFileName { get; set; }
        public string VideoCaption { get; set; }
    }

    public class GetVideoSuggestComments
    {
        public string vsubscribername { get; set; }
        public string vcomments { get; set; }
        public DateTime vcommentdate { get; set; }
    }

    public class AudioSuggestComments
    {
        public string CelebrityName { get; set; }
        public string Followersname { get; set; }
        public string AudioComments { get; set; }
        public string AudioFileName { get; set; }
        public string AudioCaption { get; set; }
    }

    public class GetAudioSuggestComments
    {
        public string asubscribername { get; set; }
        public string acomments { get; set; }
        public DateTime acommentdate { get; set; }
    }

    public class GetProfilePicture
    {
        public string FileName { get; set; }
        public string CelebrityName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class InsertPayments
    {
        public string CelebrityName { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }

    public class AfterPayment
    {
        public string RzOrderID { get; set; }
        public string RzPaymentID { get; set; }
        public string RzSignature { get; set; }
        public string RzStatus { get; set; }

    }

    public class PaymentFailure
    {
        public string CelebrityName { get; set; }
        public string FollowersEmailID { get; set; }
        public string ContentCaptions { get; set; }
        public string RazOrderID { get; set; }
        public string RazPaymentID { get; set; }
        public string RazStatus { get; set; }

    }



}
