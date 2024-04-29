using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using jstructs;
using static jstructs.jdata;

namespace jIO{
    public class jSerializer{
        private static uint file_signature = 0x7070616A;
        public static string save_path = "D:\\BACKUP\\JobApples\\listings.file"; // ??? this should be automatic?? or idk
        public static string backup_path = "D:\\BACKUP\\JobApples\\listings.file1";
        public static void create_backup(){
            if (!File.Exists(save_path)) return; // dont back up if no current save file
            if (File.Exists(backup_path)) File.Delete(backup_path); // clear old backup if it exists
            File.Copy(save_path, backup_path);
        }
        public static void serialize(List<jdata.jobject> current_listings, uint filters){
            create_backup();
            using (var fs = new FileStream(save_path, FileMode.Create, FileAccess.Write)){
                // encode file signature
                byte[] bytes = BitConverter.GetBytes(file_signature);
                fs.Write(bytes, 0, 4);
                // encode filters
                bytes = BitConverter.GetBytes(filters);
                fs.Write(bytes, 0, 4);
                // encode object count
                bytes = BitConverter.GetBytes(current_listings.Count);
                fs.Write(bytes, 0, 4);
                
                // encode all objects
                for (int i = 0; i < current_listings.Count; i++){
                    var current = current_listings[i];
                    serialize_string(fs, current.title);
                    serialize_string(fs, current.employer);
                    serialize_string(fs, current.link);
                    fs.WriteByte((byte)current.status);
                }
            }

        }
        private static void serialize_string(FileStream fs, string value){
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(value);
            // clamp size at 255 bytes!!
            int length = buffer.Length;
            if (length > 255) length = 255;
            // then serialize
            fs.WriteByte((byte)length);
            fs.Write(buffer, 0, length);
            return;
        }
        public static List<jdata.jobject> deserialize(ref uint filters){
            // fallback for no file
            if (!File.Exists(save_path)) return new();

            using (var fs = new FileStream(save_path, FileMode.Open)){
                // check encoded file signature
                byte[] bytes = new byte[4];
                fs.Read(bytes, 0, 4);
                if (BitConverter.ToUInt32(bytes) != file_signature)
                    throw new Exception("Bad file signature on serialized data file!!");

                // decode the filters
                fs.Read(bytes, 0, 4);
                filters = BitConverter.ToUInt32(bytes);
                // read the 4byte string count indicator
                fs.Read(bytes, 0, 4);
                uint objects_count = BitConverter.ToUInt32(bytes);

                // loop read j listing strings
                List<jdata.jobject> output = new();
                for (int i = 0; i < objects_count; i++){
                    jdata.jobject curr_listing = new();
                    curr_listing.title = deserialize_string(fs);
                    curr_listing.employer = deserialize_string(fs);
                    curr_listing.link = deserialize_string(fs);
                    curr_listing.status = (fixed_job_states)fs.ReadByte();
                    output.Add(curr_listing);
                }
                return output;
            }
        }
        private static string deserialize_string(FileStream fs){
            int string_length = fs.ReadByte();
            if (string_length == 0) return "";

            byte[] string_buffer = new byte[string_length];
            fs.Read(string_buffer, 0, string_length);
            return System.Text.Encoding.UTF8.GetString(string_buffer);
        }
    }
}
