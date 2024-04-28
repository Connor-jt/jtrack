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
using System.Windows.Markup;
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
        private MainWindow main;
        private jdata.jobject data;
        private bool initialized = false;
        public JobListing(jdata.jobject data_obj, MainWindow callback)
        {
            InitializeComponent();
            status_box.ItemsSource = jdata.job_states;
            status_box.SelectedIndex = (int)data_obj.status;

            title_box.Text = data_obj.title;
            employer_box.Text = data_obj.employer;
            data = data_obj;
            main = callback;
            initialized = true;
        }

        private void StatusChanged(object sender, SelectionChangedEventArgs e){
            if (!initialized) return;
            data.status = (jdata.fixed_job_states)status_box.SelectedIndex;
            main.ListingUpdated();
        }
    }
}
