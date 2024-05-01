using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using jIO;
using jstructs;
using jURL;

namespace jtrack
{
    // UI for job listing: name, company, application status | buttons: reorder??, 
    // UI for accepting urls
    // switch case depending on url
    // differe+nt instructions for finding title, desc, & other details
    // inspect and manage listings
    // 
    public partial class MainWindow : Window{

        // //////////////////////////////// // ----------------------------------------------------------------
        // MAIN LOADING & HELPER FUNCTIONS //
        // ////////////////////////////// //
        List<jdata.jobject> listings;
        public MainWindow(){
            InitializeComponent();
            // populate job manual status dropdown
            jman_status.ItemsSource = jdata.job_states;
            // TODO: we need to populate the listings filters

            // deserialize listings from saved file if any
            listings = jSerializer.deserialize(ref filter_map);
            LoadFilterUI();
            reload_listings();
        }
        private void reload_listings() {
            // store our currently selected member
            jdata.jobject? selected_job = null;
            if (listings_panel.SelectedIndex != -1)
                selected_job = ((List<JobListing>)listings_panel.ItemsSource)[listings_panel.SelectedIndex].data;

            // reset filter stats
            foreach (var v in filter_UIs) v.matching_count = 0;

            List<JobListing> UI_list = new();
            for (int i = 0; i < listings.Count; i++){
                byte curr_state = (byte)listings[i].status;
                filter_UIs[curr_state].matching_count++;
                if ((filter_map & (1u << curr_state)) == 0) // if '1', then we exclude on these filters??
                    UI_list.Add(new JobListing(listings[i], this));
            }
            listings_panel.ItemsSource = UI_list;

            // post new filter stats
            foreach (var v in filter_UIs) v.UpdateCount();
            job_count_box.Text = listings.Count.ToString();
            // reselect the previously selected entry if its still in the UI
            if (selected_job != null){
                for (int i = 0; i < UI_list.Count; i++){
                    if (UI_list[i].data == selected_job){
                        listings_panel.SelectedIndex = i;
                        break;
            }}}
        }
        private void Grid_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Escape) keypress_for_abort();
        }
        void PostError(string error) => error_text.Text = error;
        // ----------------------------------------------------------------------------------------------------

        // //////////////////////// // -----------------------------------------------------------------
        // FILTER GENERATION STUFF //
        // ////////////////////// //
        private uint filter_map = 0;
        private List<ListingsFilter> filter_UIs = new();
        public void UpdateFilters(jdata.fixed_job_states filter, bool state){
            if (state) filter_map |= (1u << (byte)filter);
            else filter_map &= ~(1u << (byte)filter);
            jSerializer.serialize(listings, filter_map);
            reload_listings();
        }
        private void LoadFilterUI(){
            for (int i = 0; i < jdata.job_states.Length; i++){
                bool is_filter_disabled = (filter_map & (1u << i) ) != 0;
                ListingsFilter curr_filter = new(this, (jdata.fixed_job_states)i, is_filter_disabled);
                filters_panel.Children.Add(curr_filter);
                filter_UIs.Add(curr_filter);
            }
        }
        // ------------------------------------------------------------------------------------------

        // ///////////////////////////////// // --------------------------------------------------------------
        // MANUAL LISTING ITEM INTERACTIONS //
        // /////////////////////////////// //
        private int GetSelectedItemIndex(int index){
            if (index == -1) throw new Exception("forgot to check index == -1");

            jdata.jobject? selected_job = null;
            selected_job = ((List<JobListing>)listings_panel.ItemsSource)[index].data;
            // reselect the previously selected entry if its still in the UI
            if (selected_job != null)
                for (int i = 0; i < listings.Count; i++)
                    if (listings[i] == selected_job)
                        return i;
            throw new Exception("failed to find item!!");
        }
        private void listings_panel_SelectionChanged(object sender, SelectionChangedEventArgs e){
            if (listings_panel.SelectedIndex == -1)
                 listing_menu.Visibility = Visibility.Collapsed;
            else listing_menu.Visibility = Visibility.Visible;
        }
        public void ListingOrderUp(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            ListingSwap(listings_panel.SelectedIndex-1);
        }
        public void ListingOrderDown(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return; }
            ListingSwap(listings_panel.SelectedIndex+1);
        }
        private void ListingSwap(int dst){
            var UI_elems = (List<JobListing>)listings_panel.ItemsSource;
            if (dst < 0) return; // cant shift up if we're already at 0
            if (dst >= UI_elems.Count) return; // cant shift down if we're at the bottom
            int selected_listing_index = GetSelectedItemIndex(listings_panel.SelectedIndex);
            int new_listing_index = GetSelectedItemIndex(dst);
            // pop it then add it at the new index
            var item = listings[selected_listing_index];
            listings.RemoveAt(selected_listing_index);
            listings.Insert(new_listing_index, item);
            // save changes to disk
            jSerializer.serialize(listings, filter_map);
            reload_listings(); // this should automatically reselect whatever one we just moved
        }
        public void ListingLink(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            try{Process.Start(new ProcessStartInfo { FileName = listings[GetSelectedItemIndex(listings_panel.SelectedIndex)].link, UseShellExecute = true });
            } catch(Exception ex){PostError(ex.Message);}
        }
        public void ListingLinkCopy(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1) { PostError("No selected listing!!"); return; }
            Clipboard.SetText(listings[GetSelectedItemIndex(listings_panel.SelectedIndex)].link);
        }
        public void ListingRemove(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            // confirm that we actually want to delete the item!!
            if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes){
                listings.RemoveAt(GetSelectedItemIndex(listings_panel.SelectedIndex));
                jSerializer.serialize(listings, filter_map); // save changes to disk
                reload_listings();
            }
        }
        public void ListingUpdated(){
            jSerializer.serialize(listings, filter_map); // save changes to disk
            reload_listings();
        }
        // --------------------------------------------------------------------------------------------------------

        // ////////////////////////////////// // --------------------------------------------------------------------------
        // STUFF FOR ADDING NEW JOB LISTINGS //
        // //////////////////////////////// //
        bool new_listing_open = false;
        bool processing_url = false;
        string? curr_listing_link = null;
        private void Button_NewListing(object sender, RoutedEventArgs e){
            if (new_listing_open){
                PostError("New listing UI already open!!");
                return;}
            // open up listing UI
            new_listing_panel.Visibility = Visibility.Visible;
            new_listing_link_panel.Visibility = Visibility.Visible;
            jauto_link.Focus();
            // clear any data inside the boxes
            jauto_link.Text = "";
            jman_title.Text = "";
            jman_employer.Text = "";
            jman_status.SelectedIndex = 0;
            curr_listing_link = null;
            new_listing_open = true;
            processing_url = false;
        }

        private async void Button_AutoSubmit(object sender, RoutedEventArgs e){
            if (processing_url){
                PostError("Already processing url request!!");
                return;}

            processing_url = true;
            curr_listing_link = jauto_link.Text;
            // check for errors in link
            if (string.IsNullOrWhiteSpace(curr_listing_link)){
                PostError("Empty link!!");
                CloseNewListingUI();
                return;}

            // then process url
            new_listing_link_panel.Visibility = Visibility.Collapsed;
            new_listing_waiting_symbol.Visibility = Visibility.Visible;
            var job_data_wrapper = await URLprocessor.Process(curr_listing_link);
            if (job_data_wrapper == null){ // check to see if processor failed!!
                PostError("Failed to process url!!");
                CloseNewListingUI();
                return;}
            URLprocessor.URL_result job_data = (URLprocessor.URL_result)job_data_wrapper;

            // finished loading url, process found data
            if (job_data.found_all_data) {
                PostNewListing(job_data.company, job_data.title, curr_listing_link, jdata.fixed_job_states.unapplied);
                CloseNewListingUI();
                return;}

            // pass in any data that we did manage to pull ('else if' because if both were valid we'd have returned true)
            if (!string.IsNullOrWhiteSpace(job_data.title)) jman_title.Text = job_data.title;
            else if (!string.IsNullOrWhiteSpace(job_data.company)) jman_employer.Text = job_data.company;

            // open manual details UI
            new_listing_waiting_symbol.Visibility = Visibility.Collapsed;
            new_listing_details_panel.Visibility = Visibility.Visible;

            jman_title.Focus();
        }
        private void Button_ManualSubmit(object sender, RoutedEventArgs e){
            if (string.IsNullOrWhiteSpace(jman_title.Text) || string.IsNullOrWhiteSpace(jman_employer.Text) || jman_status.SelectedIndex == -1){
                PostError("Empty/null listing data, try again!!");
                return;}
            // otherwise go ahead and submit the data and close out the listing
            PostNewListing(jman_employer.Text, jman_title.Text, curr_listing_link, (jdata.fixed_job_states)jman_status.SelectedIndex);
            CloseNewListingUI();
        }
        private void PostNewListing(string company, string title, string link, jdata.fixed_job_states status){
            jdata.jobject data_obj = new();
            data_obj.employer = company;
            data_obj.title = title;
            data_obj.link = link;
            data_obj.status = status;
            listings.Add(data_obj);
            jSerializer.serialize(listings, filter_map); // save changes to disk
            reload_listings();
        }
        private void CloseNewListingUI(){
            if (!new_listing_open){
                PostError("Failed to close new listing UI because it was not open!!");
                return;}
            new_listing_panel.Visibility = Visibility.Collapsed;
            new_listing_link_panel.Visibility = Visibility.Collapsed;
            new_listing_waiting_symbol.Visibility = Visibility.Collapsed;
            new_listing_details_panel.Visibility = Visibility.Collapsed;
            main_thing.Focus();
            new_listing_open = false;
            processing_url = false;
        }
        private void keypress_for_abort(){
            if (!new_listing_open) return;
            CloseNewListingUI();
        }


        // ----------------------------------------------------------------------------------------------------------------------

        // JUNK TO MAKE NAVIGATION EASIER
        private void jauto_link_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter){
                e.Handled = true;
                Button_AutoSubmit(null, null);
        }}
        private void jman_title_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter) {
                e.Handled = true;
                jman_employer.Focus();
        }}
        private void jman_employer_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter) {
                e.Handled = true;
                Button_ManualSubmit(null, null);
        }}



        // god bless this code
        #region WindowBar_funcs 
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e) { SystemCommands.MinimizeWindow(this); }
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) { SystemCommands.CloseWindow(this); }

        #endregion
    }
}
