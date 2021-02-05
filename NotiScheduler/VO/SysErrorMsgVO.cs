using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.VO {
    class SysErrorMsgVO {

        public string className { get; set; }
        public string errormsg { get; set; }

        public SysErrorMsgVO(string errormsg) {

            this.className = "";
            this.errormsg = errormsg;
        }

        public SysErrorMsgVO(string className, string errormsg) {

            this.className = className;
            this.errormsg = errormsg;
        } 
    }
}
