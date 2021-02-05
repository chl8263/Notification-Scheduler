using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.Helper {
    class UrlHelper {

        private string SLASH = "/";

        private string COLON = ":";

        private string DOT = ".";

        private string SCHEME_SUFFIX = "://";

        private string scheme = "http";

        private string host = "locahost";

        private string port = "";

        private string path = "";

        private string extension = "";

        public UrlHelper Scheme(string scheme) {
            this.scheme = scheme;
            return this;
        }

        public UrlHelper Host(string host) {
            this.host = host;
            return this;
        }

        public UrlHelper Port(string port) {
            this.port = port;
            return this;
        }

        public UrlHelper Path(string path) {
            this.path = this.path + SLASH + path;
            return this;
        }

        public UrlHelper Extension(string extension) {
            this.extension = extension;
            return this;
        }

        public string Build() {
            var result = scheme + SCHEME_SUFFIX + host + COLON + port + path + DOT + extension;
            return result;
        }
    }
}
