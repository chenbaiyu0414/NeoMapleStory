namespace NeoMapleStory.Core.Database
{
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool Gender { get; set; }
        public bool IsGm { get; set; }

        /// <summary>
        /// 点券
        /// </summary>
        public int NexonPoint { get; set; }

        /// <summary>
        /// 抵用券
        /// </summary>
        public int MaplePoint { get; set; }

        //public DateTime BirthDate { get; set; } = DateTime.Now.Date;

        //[EmailAddress]
        //[MaxLength(320)]
        //public string Email { get; set; }

        //public DateTime RegisterDate { get; set; } = DateTime.Now;

        //public bool IsLogged { get; set; } = false;

        //public DateTime? LastLoginTime { get; set; } = null;

        //[MaxLength(15)]
        //public string LastLoginIp { get; set; }

        //[MaxLength(17)]
        //public string LastLoginMac { get; set; }

        //public bool PermanentBan { get; set; }

        //public DateTime? TempBan { get; set; }

        //[Column(TypeName = "varchar")]
        //public string BanReason { get; set; }
    }
}
