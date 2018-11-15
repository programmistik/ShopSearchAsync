using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ShopSearchAsync
{
   
    public partial class MainWindow : Window
    {

        DataTable table;

        public MainWindow()
        {
            InitializeComponent();
            
            OpenConnAsync();
        }

        private async void OpenConnAsync()
        {
            DataSet results = await GetDataSetAsync(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString, "select * from product", null);

            table = results.Tables[0];
            productDataGrid.DataContext = table;
        }

        public Task<DataSet> GetDataSetAsync(string sConnectionString, string sSQL, params SqlParameter[] parameters)
        {
            return Task.Run(() =>
            {
                using (var newConnection = new SqlConnection(sConnectionString))
                using (var mySQLAdapter = new SqlDataAdapter(sSQL, newConnection))
                {
                    mySQLAdapter.SelectCommand.CommandType = CommandType.Text;
                    if (parameters != null) mySQLAdapter.SelectCommand.Parameters.AddRange(parameters);

                    DataSet myDataSet = new DataSet();
                    mySQLAdapter.Fill(myDataSet);
                    return myDataSet;
                }
            });
        }
        

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            tbProductName.Text = "";
            tbPrice.Text = "";
            PriceCB.SelectedIndex = 0;
            chbDiscount.IsChecked = null;
            productDataGrid.DataContext = table;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
            tbProductName.Text = "";
            tbPrice.Text = "";
            PriceCB.SelectedIndex = 0;
            chbDiscount.IsChecked = null;
        }

        private void Search()
        {
            var tab = table.AsEnumerable().Where(p => p.Field<string>(1).ToUpper().Contains(tbProductName.Text.ToUpper()));


            if (!String.IsNullOrEmpty(tbPrice.Text))
            {
                if (PriceCB.SelectedIndex == 0 && tbPrice.Text.ToLower() == "null")
                    tab = tab.Where(pr => pr.Field<decimal?>(3) == null);
                else
                {
                    CultureInfo culture = new CultureInfo("en-US");
                    if (decimal.TryParse(tbPrice.Text, NumberStyles.AllowDecimalPoint, culture, out decimal result))
                    {
                        if (PriceCB.SelectedIndex == 0)
                            tab = tab.Where(pr => pr.Field<decimal?>(3) == result);
                        else if (PriceCB.SelectedIndex == 1)
                            tab = tab.Where(pr => pr.Field<decimal?>(3) > result);
                        else if (PriceCB.SelectedIndex == 2)
                            tab = tab.Where(pr => pr.Field<decimal?>(3) < result);
                        else if (PriceCB.SelectedIndex == 3)
                            tab = tab.Where(pr => pr.Field<decimal?>(3) >= result);
                        else if (PriceCB.SelectedIndex == 4)
                            tab = tab.Where(pr => pr.Field<decimal?>(3) <= result);
                    }
                    else
                        MessageBox.Show("Incorrect unit price!");
                }

            }

            if (chbDiscount.IsChecked != null)
            {
                if (chbDiscount.IsChecked == true)
                    tab = tab.Where(p => p.Field<bool>(5) == true);
                else
                    tab = tab.Where(p => p.Field<bool>(5) == false);
            }
            productDataGrid.DataContext = tab.AsDataView();
        }

        private void tbPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^-?[0-9][0-9,\.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PriceCB.SelectedIndex = 0;
            chbDiscount.IsChecked = null;
        }

        private void tbProductName_KeyUp(object sender, KeyEventArgs e)
        {
            Search();
        }
    }
}

