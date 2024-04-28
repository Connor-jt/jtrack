using jstructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace jtrack
{
    /// <summary>
    /// Interaction logic for JobListing.xaml
    /// </summary>
    public partial class JobListing : UserControl
    {
        private string access_link;
        public JobListing(string title, string employer, string link, jdata.fixed_job_states status)
        {
            InitializeComponent();
            status_box.ItemsSource = jdata.job_states;
            status_box.SelectedIndex = (int)status;

            title_box.Text = title;
            employer_box.Text = employer;
            access_link = link;
        }
    }
}
