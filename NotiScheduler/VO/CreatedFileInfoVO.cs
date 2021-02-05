using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NotiScheduler.VO {
    class CreatedFileInfoVO {

        public const string nil = "nil";

        public CreatedFileInfoVO() {
        }

        public CreatedFileInfoVO(String fileName, String filePath, int dataCount) {
            this.fileName = fileName;
            this.filePath = filePath;
            this.dataCount = dataCount;
        }

        public String fileName { get; set; }

        public String filePath { get; set; }

        public int dataCount { get; set; }
    }
}
