using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace jstructs{
    public static class jdata{
        public static string[] job_states = { "UNAPPLIED", "PENDING", "EXPIRED", "ABORTED", "REJECTED" };
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
namespace jIO{
    public class serializer{
        public string save_path; // ??? this should be automatic?? or idk
        public void serialize()
        {

        }
        public void deserialize()
        {

        }
        public void create_backup()
        {

        }
    }
}

namespace jtrack
{
    // UI for job listing: name, company, application status | buttons: reorder??, 
    // UI for accepting urls
    // switch case depending on url
    // differe+nt instructions for finding title, desc, & other details
    // inspect and manage listings
    // 








    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
