using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CreatorsSquad.Helpers
{
    public class AppSettings
    {
        // Properties for JWT Token Signature
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpireTime { get; set; }
        public string Secret { get; set; }

        // Sendgrind
        //public string SendGridKey { get; set; }
        //public string SendGridUser { get; set; }

        // Storing the contents in Amazon S3

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }

        // Razorpay Credentials

        public string _Key { get; set; }
        public string _Secret { get; set; }


    }
}
