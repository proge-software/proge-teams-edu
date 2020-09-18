using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Abstraction
{
    public interface ISdsSyncParameters
    {
        public string SyncProfileId { get; set; }

        public string SyncProfileUploadUrl { get; set; }

        public string[] LocalCsvFilesPaths { get; set; }
    }
}
