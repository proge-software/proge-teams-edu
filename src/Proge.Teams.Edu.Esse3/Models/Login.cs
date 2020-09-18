using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Esse3.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Kind
    {
        public string value { get; set; }
    }

    public class Credentials
    {
        public object jwtKeyId { get; set; }
        public Kind kind { get; set; }
        public object profile { get; set; }
        public string user { get; set; }
    }

    public class User
    {
        public object codFis { get; set; }
        public object docenteId { get; set; }
        public object firstName { get; set; }
        public string grpDes { get; set; }
        public int grpId { get; set; }
        public int id { get; set; }
        public object idAb { get; set; }
        public object lastName { get; set; }
        public object persId { get; set; }
        public object soggEstId { get; set; }
        public int tipoFirmaFaId { get; set; }
        public int tipoFirmaId { get; set; }
        public List<object> trattiCarriera { get; set; }
        public string userId { get; set; }
    }

    public class Login
    {
        public string authToken { get; set; }
        public Credentials credentials { get; set; }
        public bool expPwd { get; set; }
        public string internalAuthToken { get; set; }
        public List<object> profili { get; set; }
        public User user { get; set; }
    }


}
