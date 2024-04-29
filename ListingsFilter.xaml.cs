using System;
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
using jstructs;

namespace jtrack{
    /// <summary>
    /// Interaction logic for ListingsFilter.xaml
    /// </summary>
    public partial class ListingsFilter : UserControl{
        MainWindow main;
        jdata.fixed_job_states filter;
        public int matching_count = 0;
        public ListingsFilter(MainWindow main, jdata.fixed_job_states filter, bool is_filter_disabled){
            InitializeComponent();
            this.main = main;
            this.filter = filter;
            filter_title_box.Text = jdata.job_states[(byte)filter];
            filter_off = is_filter_disabled;
            RefreshFilterVisuallyActive();
        }
        public void UpdateCount()
         => count_text.Text = matching_count.ToString();

        bool filter_off = false;
        private void Button_Click(object sender, RoutedEventArgs e){
            filter_off = !filter_off;
            main.UpdateFilters(filter, filter_off);
            RefreshFilterVisuallyActive();
        }
        private void RefreshFilterVisuallyActive(){
            if (filter_off) filter_title_box.Foreground = Brushes.Gray;
            else filter_title_box.Foreground = Brushes.White;
        }
    }
}
