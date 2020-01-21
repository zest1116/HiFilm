using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class LoginUser : AzureADObject
    {

        public string DisplayName { get; set; }

        public string MailAddress { get; set; }

    }
}