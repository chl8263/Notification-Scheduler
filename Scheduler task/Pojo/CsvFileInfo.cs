using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler_task.Pojo {
    class FileInfo {
        public const string nil = "nil";

        public FileInfo() {
        }

        public FileInfo(String fileName, String filePath, int dataCount) {
            this.fileName = fileName;
            this.filePath = filePath;
            this.dataCount = dataCount;
        }

        public String fileName { get; set; }

        public String filePath { get; set; }

        public int dataCount { get; set; }
    }
}
