using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using QvConnectorInterface;

using MessageBox = System.Windows.Forms.MessageBox;

namespace QvConnector
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private Dictionary<string, IQvConnector> ConnectorMap = null;
        public const string defaultInput = "None";

        public Login(Dictionary<string, IQvConnector> l)
        {
            InitializeComponent();

            this.ConnectorMap = l;
            
            this.PopulateDriverListBox();
            this.PopulateAuthListBox();

        }

        private void PopulateDriverListBox()
        {
            this.driverListBox.Items.Clear();

            if (this.ConnectorMap == null || this.ConnectorMap.Count == 0)
                throw new Exception("No connection plugin registered ...");

            if (this.ConnectorMap != null)
                foreach (string qvc in this.ConnectorMap.Keys)
                    this.driverListBox.Items.Add(qvc);

            this.driverListBox.SelectedIndex = 0;
        }

        private IQvConnector getDriver()
        {
            if (this.ConnectorMap != null && this.GetDriver() != null)
                return this.ConnectorMap[this.GetDriver()];

            throw new Exception("No connection plugin registered ...");
        }

        private void PopulateAuthListBox()
        {
            this.authListBox.Items.Clear();
            this.authListBox.Items.Add(Login.defaultInput);

            foreach (string gaam in this.getDriver().getAvailableAuthMethods())
                this.authListBox.Items.Add(gaam);

            this.authListBox.SelectedIndex = 0;
            this.ClearCredentials();
        }

        private void ClearCredentials()
        {
            if (this.authListBox.SelectedIndex == 0)
            {
                this.userTextBox.IsEnabled = false;
                this.passwordBox.IsEnabled = false;
            }
            else
            {
                this.userTextBox.IsEnabled = true;
                this.passwordBox.IsEnabled = true;
            }

            this.userTextBox.Text = "";
            this.passwordBox.Password = "";
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            string result = (this.getDriver().Test(this.GetServer(), this.GetAuth(), this.GetUsername(), this.GetPassword(), this.GetParam())) ? "Connection OK!" : "Connection Failed ...";
            MessageBox.Show(result, "Test Result");
        }

        public string GetServer()
        {
            return serverTextBox.Text;
        }

        public string GetDriver()
        {
            if (driverListBox.SelectedItem == null) return null;
            return driverListBox.SelectedItem.ToString();
        }

        public string GetAuth()
        {
            if (authListBox.SelectedItem == null) return null;
            return authListBox.SelectedItem.ToString();
        }

        public string GetParam()
        {
            if (paramTextBox.Text != null && paramTextBox.Text.Trim().Length == 0) return Login.defaultInput;
            return paramTextBox.Text;
        }

        public string GetUsername()
        {
            return userTextBox.Text;
        }

        public string GetPassword()
        {
            return passwordBox.Password;
        }

        private void okBbutton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void driverListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PopulateAuthListBox();
        }

        private void authListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ClearCredentials();
        }
    }
}
