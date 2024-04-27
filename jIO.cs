using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jstructs;

namespace jIO{
    public class jSerializer{
        public static string save_path = "D:\\BACKUP\\JobApples\\listings.file"; // ??? this should be automatic?? or idk
        public static void serialize(List<jdata.jobject> current_listings)
        {
            create_backup();
        }
        public static List<jdata.jobject> deserialize(){
            // fallback for no file
            if (!File.Exists(save_path)) return new();

            var fs = new FileStream(save_path, FileMode.Open);
            var len = (int)fs.Length;

            // read the 4byte string count indicator
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            uint objects_count = BitConverter.ToUInt32(bytes);
            // loop read strings, 1 byte string length at the start



            fs.Close();
        }
        public static void create_backup()
        {

        }
    }
}
