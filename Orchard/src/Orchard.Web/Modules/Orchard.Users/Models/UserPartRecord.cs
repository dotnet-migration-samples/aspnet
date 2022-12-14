using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

namespace Orchard.Users.Models {
    public class UserPartRecord : ContentPartRecord {
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual string NormalizedUserName { get; set; }

        public virtual string Password { get; set; }
        public virtual MembershipPasswordFormat PasswordFormat { get; set; }
        public virtual string HashAlgorithm { get; set; }
        public virtual string PasswordSalt { get; set; }

        public virtual UserStatus RegistrationStatus { get; set; }
        public virtual UserStatus EmailStatus { get; set; }
        public virtual string EmailChallengeToken { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? LastLoginUtc { get; set; }
        public virtual DateTime? LastLogoutUtc { get; set; }
        public virtual DateTime? LastPasswordChangeUtc { get; set; }
        public virtual bool ForcePasswordChange { get; set; }
    }
}