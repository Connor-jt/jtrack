using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jURL{
    static class URLprocessor{
        public struct URL_result {
            public bool found_all_data;
            public string? title;
            public string? company;
        }
        public static async Task<URL_result> Process(string link){
            URL_result result = new URL_result();
            result.title = "test title";
            //result.company = "test company";


            // return true if all data was found
            result.found_all_data = !(string.IsNullOrWhiteSpace(result.title) | string.IsNullOrWhiteSpace(result.company));
            return result;
        }
    }
}
