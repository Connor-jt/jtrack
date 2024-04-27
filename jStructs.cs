using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace jstructs{
    public static class jdata{
        public static readonly string[] job_states = { "UNAPPLIED", "PENDING", "EXPIRED", "ABORTED", "REJECTED" };
        public enum fixed_job_states{
            unapplied = 0,
            pending = 1,
            expired = 2,
            aborted = 3,
            rejected = 4,
        }

        public class jobject{
            public string title;
            public string employer;
            public string link;
            public string status;
        }
}}
