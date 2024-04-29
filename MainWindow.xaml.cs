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
            listings = jSerializer.deserialize();
            reload_listings();
        }
        private void reload_listings() {
            List<JobListing> UI_list = new();
            for (int i = 0; i < listings.Count; i++)
                // NOTE: we should filter out ones that dont match active filters??
                UI_list.Add(new JobListing(listings[i], this));
            listings_panel.ItemsSource = UI_list;
        }
        private void Grid_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Escape) keypress_for_abort();
        }
        void PostError(string error) => error_text.Text = error;
        // ----------------------------------------------------------------------------------------------------

        // ///////////////////////////////// // --------------------------------------------------------------
        // MANUAL LISTING ITEM INTERACTIONS //
        // /////////////////////////////// //
        private void listings_panel_SelectionChanged(object sender, SelectionChangedEventArgs e){
            if (listings_panel.SelectedIndex == -1)
                 listing_menu.Visibility = Visibility.Collapsed;
            else listing_menu.Visibility = Visibility.Visible;
        }
        public void ListingOrderUp(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            if (listings_panel.SelectedIndex == 0) return; // cant shift up if we're already at 0
            ListingSwap(listings_panel.SelectedIndex-1);
        }
        public void ListingOrderDown(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return; }
            if (listings_panel.SelectedIndex >= listings.Count-1) return; // cant shift down if we're at the bottom
            ListingSwap(listings_panel.SelectedIndex+1);
        }
        private void ListingSwap(int dst){
            // pop it then add it at the new index
            var item = listings[listings_panel.SelectedIndex];
            listings.RemoveAt(listings_panel.SelectedIndex);
            listings.Insert(dst, item);
            // save changes to disk
            jSerializer.serialize(listings);
            reload_listings();
            listings_panel.SelectedIndex = dst;
        }
        public void ListingLink(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            try{Process.Start(new ProcessStartInfo { FileName = listings[listings_panel.SelectedIndex].link, UseShellExecute = true });
            } catch(Exception ex){PostError(ex.Message);}
        }
        public void ListingLinkCopy(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1) { PostError("No selected listing!!"); return; }
            Clipboard.SetText(listings[listings_panel.SelectedIndex].link);
        }
        public void ListingRemove(object sender, RoutedEventArgs e){
            if (listings_panel.SelectedIndex == -1){ PostError("No selected listing!!"); return;}
            // confirm that we actually want to delete the item!!
            if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes){
                listings.RemoveAt(listings_panel.SelectedIndex);
                jSerializer.serialize(listings); // save changes to disk
                reload_listings();
            }
        }
        public void ListingUpdated(){
            jSerializer.serialize(listings); // save changes to disk
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
            jSerializer.serialize(listings); // save changes to disk
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
    }
}
