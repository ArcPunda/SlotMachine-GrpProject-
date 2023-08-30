using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace SlotMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RoyalCasinoDBDataContext dbcon = new RoyalCasinoDBDataContext(Properties.Settings.Default.Group_2___CasinoConnectionString);
        DispatcherTimer dt = new DispatcherTimer();
        DispatcherTimer FCR = new DispatcherTimer();
        DispatcherTimer SCR = new DispatcherTimer();
        DispatcherTimer TCR = new DispatcherTimer();
        DispatcherTimer IFCR = new DispatcherTimer();
        DispatcherTimer ISCR = new DispatcherTimer();
        DispatcherTimer ITCR = new DispatcherTimer();
        DispatcherTimer idle = new DispatcherTimer();
        string log = "";
        int speed = 100;
        int reset = 0;
        int Increment = 0;
        int SIncrement = 0;
        int TIncrement = 0;
        int converter = 0;
        int timer = 0;
        int machineID = 6;
        int gameID = 6;
        decimal customerW = 0;
        decimal OriginalBalance = 0;
        decimal MB = 0;
        bool checker = false;
        decimal Customer = 0;
        decimal total = 0;
        public static Random rnd = new Random();
        string[] FC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();
        string[] SC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();
        string[] TC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();

        private int nextcustomerID = 0;

        public MainWindow()
        {
            InitializeComponent();

            UserName.Opacity = 100;
            Password.Opacity = 100;
            TD.Opacity = 0;
            TD1.Opacity = 0;
            BD.Opacity = 0;
            BD1.Opacity = 0;
            TR.Opacity = 0;
            TR1.Opacity = 0;
            MR.Opacity = 0;
            MR1.Opacity = 0;
            BR.Opacity = 0;
            BR1.Opacity = 0;
            Paimon.Opacity = 0;
            Balance.Opacity = 0;
            AvailableBalance.Opacity = 0;
            Rules.Opacity = 0;
            Top_Diagonal.IsHitTestVisible = false;
            TopRow.IsHitTestVisible=false;
            Middle_Row.IsHitTestVisible = false;
            Bottom_Row.IsHitTestVisible = false;
            Bottom_Diagonal.IsHitTestVisible = false;
            Undo.IsHitTestVisible = false;
            this.Start.Foreground = System.Windows.Media.Brushes.Gray;
            BET.Text = "LOCKED";
            BET.IsEnabled = false;
            BET.IsHitTestVisible = false;
            BET.TextAlignment = TextAlignment.Center;
            LogOut.IsHitTestVisible = false;
            LogOut.IsEnabled = false;
            StartGame.IsEnabled = false;
            StartGame.IsHitTestVisible = false;
            Restart.Opacity = 0;

            var bal = (from a in dbcon.table_Machines where a.Machine_ID == machineID select a).FirstOrDefault();
            MB = bal.Machine_CurrentBalance;
            
            if(MB <= 10000)
            {
                MessageBox.Show("Machine is running low on funds, please notify the admin \n " +
                                "Game Will Close Itself until funds has been refilled");
                DateTime timeLogin = DateTime.Now;
                dbcon.uspCreateGameLog(timeLogin,1,1,1,3,"Machine is running low on funds", 0, MB);
                this.Close();
            }

            string selectedFileName = FC[0];
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            FC1.Source = bitmap;

            selectedFileName = FC[1];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            FC2.Source = bitmap;

            selectedFileName = FC[2];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            FC3.Source = bitmap;
            /////

            selectedFileName = SC[0];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            SC1.Source = bitmap;

            selectedFileName = SC[1];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            SC2.Source = bitmap;

            selectedFileName = SC[2];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            SC3.Source = bitmap;

            /////

            selectedFileName = TC[0];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            TC1.Source = bitmap;

            selectedFileName = TC[1];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            TC2.Source = bitmap;

            selectedFileName = TC[2];
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName);
            bitmap.EndInit();

            TC3.Source = bitmap;

            FCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            FCR.Tick += FCR_Tick;
            SCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            SCR.Tick += SCR_Tick;
            TCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            SCR.Tick += TCR_Tick; 
            idle.Interval = new TimeSpan(0, 0, 0, 1);
            idle.Tick += idle_Tick;

            IFCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            IFCR.Tick += IFCR_Tick;
            ISCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            ISCR.Tick += ISCR_Tick;
            ITCR.Interval = new TimeSpan(0, 0, 0, 0, 1);
            ISCR.Tick += ITCR_Tick;

            IFCR.Start();
            ISCR.Start();
            ITCR.Start();
        }
        private void Dt_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)FC3.Margin.Left;
            Move1[1] = (int)FC3.Margin.Top;
            Move1[2] = (int)FC3.Margin.Right;
            Move1[3] = (int)FC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)FC2.Margin.Left;
            Move2[1] = (int)FC2.Margin.Top;
            Move2[2] = (int)FC2.Margin.Right;
            Move2[3] = (int)FC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)FC1.Margin.Left;
            Move3[1] = (int)FC1.Margin.Top;
            Move3[2] = (int)FC1.Margin.Right;
            Move3[3] = (int)FC1.Margin.Bottom;

            ///
            /// 
            int[] SMove1 = { 0, 0, 0, 0 };
            SMove1[0] = (int)SC3.Margin.Left;
            SMove1[1] = (int)SC3.Margin.Top;
            SMove1[2] = (int)SC3.Margin.Right;
            SMove1[3] = (int)SC3.Margin.Bottom;

            int[] SMove2 = { 0, 0, 0, 0 };
            SMove2[0] = (int)SC2.Margin.Left;
            SMove2[1] = (int)SC2.Margin.Top;
            SMove2[2] = (int)SC2.Margin.Right;
            SMove2[3] = (int)SC2.Margin.Bottom;

            int[] SMove3 = { 0, 0, 0, 0 };
            SMove3[0] = (int)SC1.Margin.Left;
            SMove3[1] = (int)SC1.Margin.Top;
            SMove3[2] = (int)SC1.Margin.Right;
            SMove3[3] = (int)SC1.Margin.Bottom;
            ////
            ////
            int[] TMove1 = { 0, 0, 0, 0 };
            TMove1[0] = (int)TC3.Margin.Left;
            TMove1[1] = (int)TC3.Margin.Top;
            TMove1[2] = (int)TC3.Margin.Right;
            TMove1[3] = (int)TC3.Margin.Bottom;

            int[] TMove2 = { 0, 0, 0, 0 };
            TMove2[0] = (int)TC2.Margin.Left;
            TMove2[1] = (int)TC2.Margin.Top;
            TMove2[2] = (int)TC2.Margin.Right;
            TMove2[3] = (int)TC2.Margin.Bottom;

            int[] TMove3 = { 0, 0, 0, 0 };
            TMove3[0] = (int)TC1.Margin.Left;
            TMove3[1] = (int)TC1.Margin.Top;
            TMove3[2] = (int)TC1.Margin.Right;
            TMove3[3] = (int)TC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 180 && Move1[1] > 726 && Move1[2] == 0 && Move1[3] == 0 ||
                    Move2[0] == 180 && Move2[1] > 726 && Move2[2] == 0 && Move2[3] == 0 ||
                    Move3[0] == 180 && Move3[1] > 726 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == FC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC3.Source = bitmap;
                    FC3.Margin = new Thickness(Move1[0] = 180, Move1[1] = 138, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC2.Source = bitmap;
                    FC2.Margin = new Thickness(Move2[0] = 180, Move2[1] = 138, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC1.Source = bitmap;
                    FC1.Margin = new Thickness(Move3[0] = 180, Move3[1] = 138, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                FC3.Margin = new Thickness(Move1[0], Move1[1] += 6, Move1[2], Move1[3]);
                FC2.Margin = new Thickness(Move2[0], Move2[1] += 6, Move2[2], Move2[3]);
                FC1.Margin = new Thickness(Move3[0], Move3[1] += 6, Move3[2], Move3[3]);
            }
            #endregion

            #region Second Column
            if (SMove1[0] == 482 && SMove1[1] > 726 && SMove1[2] == 0 && SMove1[3] == 0 ||
                        SMove2[0] == 482 && SMove2[1] > 726 && SMove2[2] == 0 && SMove2[3] == 0 ||
                        SMove3[0] == 482 && SMove3[1] > 726 && SMove3[2] == 0 && SMove3[3] == 0)
            {
                if (SIncrement == FC.Length)
                {
                    SIncrement = 0;
                }
                if (SMove1[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC3.Source = bitmap;
                    SC3.Margin = new Thickness(SMove1[0] = 482, SMove1[1] = 138, SMove1[2] = 0, SMove1[3] = 0);
                    SIncrement++;
                }
                else if (SMove2[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC2.Source = bitmap;
                    SC2.Margin = new Thickness(SMove2[0] = 482, SMove2[1] = 138, SMove2[2] = 0, SMove2[3] = 0);
                    SIncrement++;
                }
                else if (SMove3[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC1.Source = bitmap;
                    SC1.Margin = new Thickness(SMove3[0] = 482, SMove3[1] = 138, SMove3[2] = 0, SMove3[3] = 0);
                    SIncrement++;
                }
            }
            else
            {
                SC3.Margin = new Thickness(SMove1[0], SMove1[1] += 6, SMove1[2], SMove1[3]);
                SC2.Margin = new Thickness(SMove2[0], SMove2[1] += 6, SMove2[2], SMove2[3]);
                SC1.Margin = new Thickness(SMove3[0], SMove3[1] += 6, SMove3[2], SMove3[3]);
            }
            #endregion

            #region Third Column
            if (TMove1[0] == 794 && TMove1[1] > 726 && TMove1[2] == 0 && TMove1[3] == 0 ||
                            TMove2[0] == 794 && TMove2[1] > 726 && TMove2[2] == 0 && TMove2[3] == 0 ||
                            TMove3[0] == 794 && TMove3[1] > 726 && TMove3[2] == 0 && TMove3[3] == 0)
            {
                if (TIncrement == TC.Length)
                {
                    TIncrement = 0;
                }
                if (TMove1[1] > 650)
                {
                    string selectedFileName = TC[TIncrement];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC3.Source = bitmap;
                    TC3.Margin = new Thickness(TMove1[0] = 794, TMove1[1] = 138, TMove1[2] = 0, TMove1[3] = 0);
                    TIncrement++;
                }
                else if (TMove2[1] > 650)
                {
                    string selectedFileName = TC[TIncrement];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC2.Source = bitmap;
                    TC2.Margin = new Thickness(TMove2[0] = 794, TMove2[1] = 138, TMove2[2] = 0, TMove2[3] = 0);
                    TIncrement++;
                }
                else if (TMove3[1] > 650)
                {
                    string selectedFileName = TC[TIncrement];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC1.Source = bitmap;
                    TC1.Margin = new Thickness(TMove3[0] = 794, TMove3[1] = 138, TMove3[2] = 0, TMove3[3] = 0);
                    TIncrement++;
                }
            }
            else
            {
                TC3.Margin = new Thickness(TMove1[0], TMove1[1] += 6, TMove1[2], TMove1[3]);
                TC2.Margin = new Thickness(TMove2[0], TMove2[1] += 6, TMove2[2], TMove2[3]);
                TC1.Margin = new Thickness(TMove3[0], TMove3[1] += 6, TMove3[2], TMove3[3]);
            } 
            #endregion
        }
        private void bet_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Betting.Foreground = System.Windows.Media.Brushes.Yellow;
        }

        private void bet_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Betting.Foreground = System.Windows.Media.Brushes.White;
        }

        private void bet_Click(object sender, RoutedEventArgs e)
        {
            var allCustomers = (from b in dbcon.table_Customers
                                where b.Customer_Username == UserName.Text
                                select b).ToList();

            var customerLogin = (from a in dbcon.table_Customers
                                 where a.Customer_Username == UserName.Text
                                 select a).FirstOrDefault();

            string[] customers = new string[nextcustomerID + 1];
            string password = "";

            int y = 0;

            foreach (var customer in allCustomers)
            {
                customers[y] = customer.Customer_Username.ToString();
                y++;
            }

            if (customers.Contains(UserName.Text))
            {
                MessageBox.Show("Username Found");
                password = customerLogin.Customer_Password.ToString();

                if (Password.Password == password)
                {
                    var customerIn = (from a in dbcon.table_Customers
                                      where a.Customer_Username == UserName.Text
                                      select a).FirstOrDefault();

                    int customerID = customerIn.Customer_ID;

                    checkCustomerIfLoggedIn(customerID);
                    if (checker == true)
                    {
                        checker = false;
                    }
                    else
                    {
                        UserName.Opacity = 0;
                        Password.Opacity = 0;
                        this.Start.Foreground = System.Windows.Media.Brushes.White;
                        StartGame.IsHitTestVisible = true;
                        StartGame.IsEnabled = true;
                        UserName.IsHitTestVisible = false;
                        Password.IsHitTestVisible = false;
                        Betting.IsHitTestVisible = false;
                        LogOut.IsEnabled = true;
                        LogOut.IsHitTestVisible = true;
                        bet.Opacity = 0;
                        bet.IsEnabled = false;
                        bet.IsHitTestVisible = false;
                        BET.Text = "BET HERE";
                        BET.IsEnabled = true;
                        BET.IsHitTestVisible = true;
                        Betting.Opacity = 0;
                        LOut.Opacity = 100;
                        Rules.Opacity = 100;
                        TD.Opacity = 100;
                        TD1.Opacity = 100;
                        BD.Opacity = 100;
                        BD1.Opacity = 100;
                        TR.Opacity = 100;
                        TR1.Opacity = 100;
                        MR.Opacity = 100;
                        MR1.Opacity = 100;
                        BR.Opacity = 100;
                        BR1.Opacity = 100;
                        Balance.Opacity = 100;
                        Paimon.Opacity = 100;
                        AvailableBalance.Opacity = 100;
                        Undo.IsHitTestVisible = true;
                        Restart.Opacity = 100;
                        Top_Diagonal.IsHitTestVisible = true;
                        TopRow.IsHitTestVisible = true;
                        Middle_Row.IsHitTestVisible = true;
                        Bottom_Row.IsHitTestVisible = true;
                        Bottom_Diagonal.IsHitTestVisible = true;
                        Customer = customerIn.Customer_CurrentBalance;
                        AvailableBalance.Content = Customer.ToString();
                        OriginalBalance = Convert.ToDecimal(AvailableBalance.Content);
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect Password");
                }
            }
            else
            {
                DateTime timeLogin = DateTime.Now;
                dbcon.uspCreateGameLog(timeLogin, 1, machineID, gameID, 4, "Customer is not found", 0, 0);
                MessageBox.Show("Username Not Found");
            }
        }

        private void checkCustomerIfLoggedIn(int customerID)
        {
            var current = dbcon.vwFunqAllMachines().ToList();
            var customerIn = (from a in dbcon.table_Customers
                              where a.Customer_Username == UserName.Text
                              select a).FirstOrDefault();
            int[] idsLoggedIn = new int[6];
            int x = 0;

            foreach (var id in current)
            {
                idsLoggedIn[x++] = id.Customer_ID;
            }

            if (idsLoggedIn.Contains(customerID))
            {
                DateTime timeLogin = DateTime.Now;
                customerID = customerIn.Customer_ID;
                string customername = customerIn.Customer_Username;
                MessageBox.Show("User is logged in already in different machine");
                dbcon.uspCreateGameLog(timeLogin, customerID, machineID, gameID, 5, customername + " is already logged in a different machine \t", 0, 0);
                checker = true;
            }
            else
            {
                DateTime timeLogin = DateTime.Now;
                customerID = customerIn.Customer_ID;
                string customername = customerIn.Customer_Username;
                dbcon.uspCreateGameLog(timeLogin, customerID, machineID, gameID, 1, customername + " has logged in \t", 0, 0);
                dbcon.uspUpdateMachineCustomer(machineID, customerID);
            }
        }



        private void UserName_MouseEnter(object sender, MouseEventArgs e)
        {
            UserName.Text = "";
        }

        private void UserName_MouseLeave(object sender, MouseEventArgs e)
        {
            if (UserName.Text == "")
            {
                UserName.Text = "Username";
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            var customerIn = (from a in dbcon.table_Customers
                              where a.Customer_Username == UserName.Text
                              select a).FirstOrDefault();

            int customerID = customerIn.Customer_ID;
            string customername = customerIn.Customer_Username;
            DateTime timeLogin = DateTime.Now;
            dbcon.uspCreateGameLog(timeLogin, customerID, machineID, gameID, 1, customername + " has logged out \t", 0, 0);
            dbcon.uspUpdateMachineCustomer(machineID, 1);

            UserName.Opacity = 100;
            Password.Opacity = 100;
            UserName.IsHitTestVisible = true;
            Password.IsHitTestVisible = true;
            UserName.Text = "Username";
            Password.Password = "";
            TD.Opacity = 0;
            TD1.Opacity = 0;
            BD.Opacity = 0;
            BD1.Opacity = 0;
            TR.Opacity = 0;
            TR1.Opacity = 0;
            MR.Opacity = 0;
            MR1.Opacity = 0;
            BR.Opacity = 0;
            BR1.Opacity = 0;
            Paimon.Opacity = 0;
            Balance.Opacity = 0;
            AvailableBalance.Opacity = 0;
            AvailableBalance.Content = 0;
            TR1.Content = 0;
            MR1.Content = 0;
            BR1.Content = 0;
            TD1.Content = 0;
            BD1.Content = 0;
            Rules.Opacity = 0;
            LOut.Opacity = 0;
            Betting.Opacity = 100;
            Undo.IsHitTestVisible = false;
            Restart.Opacity = 0;
            this.Start.Foreground = System.Windows.Media.Brushes.Gray;
            BET.Text = "LOCKED";
            bet.IsEnabled = true;
            bet.IsHitTestVisible = true;
            BET.IsEnabled = false;
            BET.IsHitTestVisible = false;
            BET.TextAlignment = TextAlignment.Center;
            LogOut.IsHitTestVisible = false;
            LogOut.IsEnabled = false;
            StartGame.IsEnabled = false;
            StartGame.IsHitTestVisible = false;
            LogOut.IsHitTestVisible = false;
            LogOut.IsEnabled = false;

            MainWindow Rerun = new MainWindow();
            Rerun.Show();
            this.Close();
        }

        #region VisualText

        private void LogOut_MouseEnter(object sender, MouseEventArgs e)
        {
            this.LOut.Foreground = System.Windows.Media.Brushes.Yellow;
        }

        private void LogOut_MouseLeave(object sender, MouseEventArgs e)
        {
            this.LOut.Foreground = System.Windows.Media.Brushes.White;
        }

        private void StartGame_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Start.Foreground = System.Windows.Media.Brushes.Yellow;
        }

        private void StartGame_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Start.Foreground = System.Windows.Media.Brushes.White;
        } 
        #endregion
        private void BET_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string a = BET.Text.ToString();
                if (a == "BET HERE" || a == "LOCKED")
                {
                    //does nothing
                }
                else
                {
                    converter = Int32.Parse(a);
                }
            }
            catch (Exception ex)
            {
                BET.Text = "BET HERE";
            }
        }

        private void UserName_LostFocus(object sender, RoutedEventArgs e)
        {
           if(UserName.Text == "")
            {
                UserName.Text = "Username";
            }
            else
            {
                ///Does Nothing
            }
        }

        private void Top_Diagonal_Click(object sender, RoutedEventArgs e)
        {
            if (BET.Text == "BET HERE")
            {
                MessageBox.Show("No Value Was Placed");
            }
            else
            {
                decimal a = 0;
                decimal b = 0;
                decimal c = 0;
                decimal balance = 0;
                a = Convert.ToDecimal(TD1.Content.ToString());
                b = Convert.ToDecimal(BET.Text);
                balance = Convert.ToDecimal(AvailableBalance.Content.ToString());
                if (b > balance)
                {
                    MessageBox.Show("Insuffecient Balance");
                }
                else
                {
                    c = a + b;
                    TD1.Content = c.ToString();
                }
            }
        }

        private void Bottom_Diagonal_Click(object sender, RoutedEventArgs e)
        {
            if (BET.Text == "BET HERE")
            {
                MessageBox.Show("No Value Was Placed");
            }
            else
            {
                decimal a = 0;
                decimal b = 0;
                decimal c = 0;
                decimal balance = 0;
                a = Convert.ToDecimal(BD1.Content.ToString());
                b = Convert.ToDecimal(BET.Text);
                balance = Convert.ToDecimal(AvailableBalance.Content.ToString());
                if (b > balance)
                {
                    MessageBox.Show("Insuffecient Balance");
                }
                else
                {
                    c = a + b;
                    BD1.Content = c.ToString();
                }
            }
        }

        private void Bottom_Row_Click(object sender, RoutedEventArgs e)
        {
            if (BET.Text == "BET HERE")
            {
                MessageBox.Show("No Value Was Placed");
            }
            else
            {
                decimal a = 0;
                decimal b = 0;
                decimal c = 0;
                decimal balance = 0;
                a = Convert.ToDecimal(BR1.Content.ToString());
                b = Convert.ToDecimal(BET.Text);
                balance = Convert.ToDecimal(AvailableBalance.Content.ToString());
                if (b > balance)
                {
                    MessageBox.Show("Insuffecient Balance");
                }
                else
                {
                    c = a + b;
                    BR1.Content = c.ToString();
                }
            }
        }

        private void Middle_Row_Click(object sender, RoutedEventArgs e)
        {
            if (BET.Text == "BET HERE")
            {
                MessageBox.Show("No Value Was Placed");
            }
            else
            {
                decimal a = 0;
                decimal b = 0;
                decimal c = 0;
                decimal balance = 0;
                a = Convert.ToDecimal(MR1.Content.ToString());
                b = Convert.ToDecimal(BET.Text);
                balance = Convert.ToDecimal(AvailableBalance.Content.ToString());

                if (b > balance)
                {
                    MessageBox.Show("Insuffecient Balance");
                }
                else
                {
                    c = a + b;
                    MR1.Content = c.ToString();
                }
            }
        }

        private void TopRow_Click(object sender, RoutedEventArgs e)
        {
            if (BET.Text == "BET HERE")
            {
                MessageBox.Show("No Value Was Placed");
            }
            else
            {
                decimal a = 0;
                decimal b = 0;
                decimal c = 0;
                decimal balance = 0;
                a = Convert.ToDecimal(TR1.Content.ToString());
                b = Convert.ToDecimal(BET.Text);
                balance = Convert.ToDecimal(AvailableBalance.Content.ToString());
                if (b > balance)
                {
                    MessageBox.Show("Insuffecient Balance");
                }
                else
                {
                    c = a + b;
                    TR1.Content = c.ToString();
                }
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            decimal TR = Convert.ToDecimal(TR1.Content.ToString());
            decimal MR = Convert.ToDecimal(MR1.Content.ToString());
            decimal BR = Convert.ToDecimal(BR1.Content.ToString());
            decimal TD = Convert.ToDecimal(TD1.Content.ToString());
            decimal BD = Convert.ToDecimal(BD1.Content.ToString());
            decimal betting = Convert.ToDecimal(AvailableBalance.Content);
            total = TR + MR + BR + TD + BD;
            
            if(total > betting)
            {
                MessageBox.Show("The total amount of bets are above the available balance of: " + betting);
            }
            else
            {
                var customerIn = (from a in dbcon.table_Customers
                                  where a.Customer_Username == UserName.Text
                                  select a).FirstOrDefault();

                int customerID = customerIn.Customer_ID;
                decimal MachineBalance = MB + total;
                dbcon.uspUpdateCustomerCurrentBalance(customerID, total);
                dbcon.uspUpdateMachineBalance(machineID, MachineBalance);

                idle.Stop();
                timer = 0;
                IFCR.Stop();
                ISCR.Stop();
                ITCR.Stop();
                speed = 100;
                FCR.Start();
                speed = 100;
                SCR.Start();
                speed = 100;
                TCR.Start();
                speed = 100;
                idle.Start();

                StartGame.IsHitTestVisible = false;
                StartGame.IsHitTestVisible = true; 

                reset++;
                if (reset >= 5)
                {
                    FC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();
                    SC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();
                    TC = Slots.slottings.OrderBy(x => rnd.Next()).ToArray();
                }
            }
        }
        private void idle_Tick(object sender, EventArgs e)
        {
            if (timer >= 5)
            {
                IFCR.Start();
                ISCR.Start();
                ITCR.Start();
                timer = 0;
            }
            timer++;
        }
        private void FCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)FC3.Margin.Left;
            Move1[1] = (int)FC3.Margin.Top;
            Move1[2] = (int)FC3.Margin.Right;
            Move1[3] = (int)FC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)FC2.Margin.Left;
            Move2[1] = (int)FC2.Margin.Top;
            Move2[2] = (int)FC2.Margin.Right;
            Move2[3] = (int)FC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)FC1.Margin.Left;
            Move3[1] = (int)FC1.Margin.Top;
            Move3[2] = (int)FC1.Margin.Right;
            Move3[3] = (int)FC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 180 && Move1[1] > 650 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 180 && Move2[1] > 650 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 180 && Move3[1] > 650 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == FC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC3.Source = bitmap;
                    FC3.Margin = new Thickness(Move1[0] = 180, Move1[1] = 150, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC2.Source = bitmap;
                    FC2.Margin = new Thickness(Move2[0] = 180, Move2[1] = 150, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC1.Source = bitmap;
                    FC1.Margin = new Thickness(Move3[0] = 180, Move3[1] = 150, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                if (speed == 0)
                {
                    FC3.Margin = new Thickness(Move1[0] = 180, Move1[1] = 634, Move1[2], Move1[3]);
                    FC2.Margin = new Thickness(Move2[0] = 180, Move2[1] = 442, Move2[2], Move2[3]);
                    FC1.Margin = new Thickness(Move3[0] = 180, Move3[1] = 230, Move3[2], Move3[3]);
                    FCR.Stop();
                }
                else
                {
                    FC3.Margin = new Thickness(Move1[0], Move1[1] += 100, Move1[2], Move1[3]);
                    FC2.Margin = new Thickness(Move2[0], Move2[1] += 100, Move2[2], Move2[3]);
                    FC1.Margin = new Thickness(Move3[0], Move3[1] += 100, Move3[2], Move3[3]);
                    speed--;
                }
            }
            #endregion
        }
        private void SCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)SC3.Margin.Left;
            Move1[1] = (int)SC3.Margin.Top;
            Move1[2] = (int)SC3.Margin.Right;
            Move1[3] = (int)SC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)SC2.Margin.Left;
            Move2[1] = (int)SC2.Margin.Top;
            Move2[2] = (int)SC2.Margin.Right;
            Move2[3] = (int)SC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)SC1.Margin.Left;
            Move3[1] = (int)SC1.Margin.Top;
            Move3[2] = (int)SC1.Margin.Right;
            Move3[3] = (int)SC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 482 && Move1[1] > 650 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 482 && Move2[1] > 650 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 482 && Move3[1] > 650 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == SC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC3.Source = bitmap;
                    SC3.Margin = new Thickness(Move1[0] = 482, Move1[1] = 150, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC2.Source = bitmap;
                    SC2.Margin = new Thickness(Move2[0] = 482, Move2[1] = 150, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC1.Source = bitmap;
                    SC1.Margin = new Thickness(Move3[0] = 482, Move3[1] = 150, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                if (speed == 0)
                {
                    SC3.Margin = new Thickness(Move1[0] = 482, Move1[1] = 634, Move1[2], Move1[3]);
                    SC2.Margin = new Thickness(Move2[0] = 482, Move2[1] = 442, Move2[2], Move2[3]);
                    SC1.Margin = new Thickness(Move3[0] = 482, Move3[1] = 230, Move3[2], Move3[3]);
                    SCR.Stop();
                }
                else
                {
                    SC3.Margin = new Thickness(Move1[0], Move1[1] += 100, Move1[2], Move1[3]);
                    SC2.Margin = new Thickness(Move2[0], Move2[1] += 100, Move2[2], Move2[3]);
                    SC1.Margin = new Thickness(Move3[0], Move3[1] += 100, Move3[2], Move3[3]);
                    speed--;
                }
            }
            #endregion
        }

        private void TCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)TC3.Margin.Left;
            Move1[1] = (int)TC3.Margin.Top;
            Move1[2] = (int)TC3.Margin.Right;
            Move1[3] = (int)TC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)TC2.Margin.Left;
            Move2[1] = (int)TC2.Margin.Top;
            Move2[2] = (int)TC2.Margin.Right;
            Move2[3] = (int)TC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)TC1.Margin.Left;
            Move3[1] = (int)TC1.Margin.Top;
            Move3[2] = (int)TC1.Margin.Right;
            Move3[3] = (int)TC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 794 && Move1[1] > 650 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 794 && Move2[1] > 650 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 794 && Move3[1] > 650 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == TC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC3.Source = bitmap;
                    TC3.Margin = new Thickness(Move1[0] = 794, Move1[1] = 150, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC2.Source = bitmap;
                    TC2.Margin = new Thickness(Move2[0] = 794, Move2[1] = 150, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC1.Source = bitmap;
                    TC1.Margin = new Thickness(Move3[0] = 794, Move3[1] = 150, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                if (speed == 0)
                {
                    var customerIn = (from A in dbcon.table_Customers
                                      where A.Customer_Username == UserName.Text
                                      select A).FirstOrDefault();
                    DateTime timeLogin = DateTime.Now;
                    int customerID = customerIn.Customer_ID;

                    TC3.Margin = new Thickness(Move1[0] = 794, Move1[1] = 634, Move1[2], Move1[3]);
                    TC2.Margin = new Thickness(Move2[0] = 794, Move2[1] = 442, Move2[2], Move2[3]);
                    TC1.Margin = new Thickness(Move3[0] = 794, Move3[1] = 230, Move3[2], Move3[3]);

                    if (FC1.Source.ToString() == SC1.Source.ToString() && FC1.Source.ToString() == TC1.Source.ToString() && SC1.Source.ToString() == TC1.Source.ToString())
                    {
                        decimal a = Convert.ToDecimal(TR1.Content);
                        
                        if (a > 0)
                        {
                            MessageBox.Show("WINNER!");
                            decimal b = a * 2;
                            decimal c = Convert.ToDecimal(AvailableBalance.Content);
                            decimal d = c + b;

                            AvailableBalance.Content = d;
                            OriginalBalance = d;

                            customerW += b;
                            dbcon.uspCreateGameLog(timeLogin, customerID, 1, 1, 1, "Customer Won", customerW, MB);
                            dbcon.uspUpdateMachineCurrentWinnings(machineID,customerW);
                        }
                        else
                        {
                            MessageBox.Show("MATCH");
                        }
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);
                        AvailableBalance.Content = minus - MR - BR - BD - TD;

                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    else if (FC2.Source.ToString() == SC2.Source.ToString() && FC2.Source.ToString() == TC2.Source.ToString() && SC2.Source.ToString() == TC2.Source.ToString())
                    {
                        decimal a = Convert.ToDecimal(MR1.Content);
                        if (a > 0)
                        {
                            MessageBox.Show("WINNER!");
                            decimal b = a * 2;
                            decimal c = Convert.ToDecimal(AvailableBalance.Content);
                            decimal d = c + b;

                            AvailableBalance.Content = d;
                            OriginalBalance = d;

                            customerW += b;
                            dbcon.uspCreateGameLog(timeLogin, customerID, 1, 1, 1, "Customer Won", customerW, MB);
                            dbcon.uspUpdateMachineCurrentWinnings(machineID, customerW);
                        }
                        else
                        {
                            MessageBox.Show("MATCH");
                        }
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);
                        AvailableBalance.Content = minus - TR - BR - BD - TD;
                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    else if (FC3.Source.ToString() == SC3.Source.ToString() && FC3.Source.ToString() == TC3.Source.ToString() && SC3.Source.ToString() == TC3.Source.ToString())
                    {
                        decimal a = Convert.ToDecimal(BR1.Content);
                        if (a > 0)
                        {
                            MessageBox.Show("WINNER!");
                            decimal b = a * 2;
                            decimal c = Convert.ToDecimal(AvailableBalance.Content);
                            decimal d = c + b;

                            AvailableBalance.Content = d;
                            OriginalBalance = d;

                            customerW += b;
                            dbcon.uspCreateGameLog(timeLogin, customerID, 1, 1, 1, "Customer Won", customerW, MB);
                            dbcon.uspUpdateMachineCurrentWinnings(machineID, customerW);
                        }
                        else
                        {
                            MessageBox.Show("MATCH");
                        }
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);
                        AvailableBalance.Content = minus - TR - MR - BD - TD;
                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    else if (FC3.Source.ToString() == SC2.Source.ToString() && FC3.Source.ToString() == TC1.Source.ToString() && SC2.Source.ToString() == TC1.Source.ToString())
                    {
                        decimal a = Convert.ToDecimal(TD1.Content);
                        if (a > 0)
                        {
                            MessageBox.Show("WINNER!");
                            decimal b = a * 5;
                            decimal c = Convert.ToDecimal(AvailableBalance.Content);
                            decimal d = c + b;

                            AvailableBalance.Content = d;
                            OriginalBalance = d;

                            customerW += b;
                            dbcon.uspCreateGameLog(timeLogin, customerID, 1, 1, 1, "Customer Won", customerW, MB);
                            dbcon.uspUpdateMachineCurrentWinnings(machineID, customerW);
                        }
                        else
                        {
                            MessageBox.Show("MATCH");
                        }
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);

                        AvailableBalance.Content = minus - TR - MR - BR - BD;
                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    else if (FC1.Source.ToString() == SC2.Source.ToString() && FC1.Source.ToString() == TC3.Source.ToString() && SC2.Source.ToString() == TC3.Source.ToString())
                    {
                        decimal a = Convert.ToDecimal(BD1.Content);
                        if (a > 0)
                        {
                            MessageBox.Show("WINNER!");
                            decimal b = a * 5;
                            decimal c = Convert.ToDecimal(AvailableBalance.Content);
                            decimal d = c + b;

                            AvailableBalance.Content = d;
                            OriginalBalance = d;

                            customerW += b;
                            dbcon.uspCreateGameLog(timeLogin, customerID, 1, 1, 1, "Customer Won", customerW, MB);
                            dbcon.uspUpdateMachineCurrentWinnings(machineID, customerW);
                        }
                        else
                        {
                            MessageBox.Show("MATCH");
                        }
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);

                        AvailableBalance.Content = minus - TR - MR - BR - TD;
                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    else
                    {
                        decimal TR = Convert.ToDecimal(TR1.Content.ToString());
                        decimal MR = Convert.ToDecimal(MR1.Content.ToString());
                        decimal BR = Convert.ToDecimal(BR1.Content.ToString());
                        decimal TD = Convert.ToDecimal(TD1.Content.ToString());
                        decimal BD = Convert.ToDecimal(BD1.Content.ToString());

                        decimal minus = Convert.ToDecimal(AvailableBalance.Content);

                        AvailableBalance.Content = minus - TR - MR - BR - TD - BD;
                        TR1.Content = 0;
                        MR1.Content = 0;
                        BR1.Content = 0;
                        BD1.Content = 0;
                        TD1.Content = 0;
                    }
                    TCR.Stop();
                }
                else
                {
                    TC3.Margin = new Thickness(Move1[0], Move1[1] += 100, Move1[2], Move1[3]);
                    TC2.Margin = new Thickness(Move2[0], Move2[1] += 100, Move2[2], Move2[3]);
                    TC1.Margin = new Thickness(Move3[0], Move3[1] += 100, Move3[2], Move3[3]);
                    speed--;
                }
            }
            #endregion
        }




        private void IFCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)FC3.Margin.Left;
            Move1[1] = (int)FC3.Margin.Top;
            Move1[2] = (int)FC3.Margin.Right;
            Move1[3] = (int)FC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)FC2.Margin.Left;
            Move2[1] = (int)FC2.Margin.Top;
            Move2[2] = (int)FC2.Margin.Right;
            Move2[3] = (int)FC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)FC1.Margin.Left;
            Move3[1] = (int)FC1.Margin.Top;
            Move3[2] = (int)FC1.Margin.Right;
            Move3[3] = (int)FC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 180 && Move1[1] > 718 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 180 && Move2[1] > 718 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 180 && Move3[1] > 718 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == FC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC3.Source = bitmap;
                    FC3.Margin = new Thickness(Move1[0] = 180, Move1[1] = 145, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC2.Source = bitmap;
                    FC2.Margin = new Thickness(Move2[0] = 180, Move2[1] = 145, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = FC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    FC1.Source = bitmap;
                    FC1.Margin = new Thickness(Move3[0] = 180, Move3[1] = 145, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                    FC3.Margin = new Thickness(Move1[0], Move1[1] += 10, Move1[2], Move1[3]);
                    FC2.Margin = new Thickness(Move2[0], Move2[1] += 10, Move2[2], Move2[3]);
                    FC1.Margin = new Thickness(Move3[0], Move3[1] += 10, Move3[2], Move3[3]);
            }
            #endregion
        }
        private void ISCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)SC3.Margin.Left;
            Move1[1] = (int)SC3.Margin.Top;
            Move1[2] = (int)SC3.Margin.Right;
            Move1[3] = (int)SC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)SC2.Margin.Left;
            Move2[1] = (int)SC2.Margin.Top;
            Move2[2] = (int)SC2.Margin.Right;
            Move2[3] = (int)SC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)SC1.Margin.Left;
            Move3[1] = (int)SC1.Margin.Top;
            Move3[2] = (int)SC1.Margin.Right;
            Move3[3] = (int)SC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 482 && Move1[1] > 718 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 482 && Move2[1] > 718 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 482 && Move3[1] > 718 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == SC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC3.Source = bitmap;
                    SC3.Margin = new Thickness(Move1[0] = 482, Move1[1] = 145, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC2.Source = bitmap;
                    SC2.Margin = new Thickness(Move2[0] = 482, Move2[1] = 145, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = SC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    SC1.Source = bitmap;
                    SC1.Margin = new Thickness(Move3[0] = 482, Move3[1] = 145, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                    SC3.Margin = new Thickness(Move1[0], Move1[1] += 10, Move1[2], Move1[3]);
                    SC2.Margin = new Thickness(Move2[0], Move2[1] += 10, Move2[2], Move2[3]);
                    SC1.Margin = new Thickness(Move3[0], Move3[1] += 10, Move3[2], Move3[3]);
            }
            #endregion
        }

        private void ITCR_Tick(object sender, EventArgs e)
        {
            int[] Move1 = { 0, 0, 0, 0 };
            Move1[0] = (int)TC3.Margin.Left;
            Move1[1] = (int)TC3.Margin.Top;
            Move1[2] = (int)TC3.Margin.Right;
            Move1[3] = (int)TC3.Margin.Bottom;

            int[] Move2 = { 0, 0, 0, 0 };
            Move2[0] = (int)TC2.Margin.Left;
            Move2[1] = (int)TC2.Margin.Top;
            Move2[2] = (int)TC2.Margin.Right;
            Move2[3] = (int)TC2.Margin.Bottom;

            int[] Move3 = { 0, 0, 0, 0 };
            Move3[0] = (int)TC1.Margin.Left;
            Move3[1] = (int)TC1.Margin.Top;
            Move3[2] = (int)TC1.Margin.Right;
            Move3[3] = (int)TC1.Margin.Bottom;
            ///
            #region First Column
            if (Move1[0] == 794 && Move1[1] > 718 && Move1[2] == 0 && Move1[3] == 0 ||
                        Move2[0] == 794 && Move2[1] > 718 && Move2[2] == 0 && Move2[3] == 0 ||
                        Move3[0] == 794 && Move3[1] > 718 && Move3[2] == 0 && Move3[3] == 0)
            {
                if (Increment == TC.Length)
                {
                    Increment = 0;
                }
                if (Move1[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC3.Source = bitmap;
                    TC3.Margin = new Thickness(Move1[0] = 794, Move1[1] = 145, Move1[2] = 0, Move1[3] = 0);
                    Increment++;
                }
                else if (Move2[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC2.Source = bitmap;
                    TC2.Margin = new Thickness(Move2[0] = 794, Move2[1] = 145, Move2[2] = 0, Move2[3] = 0);
                    Increment++;
                }
                else if (Move3[1] > 650)
                {
                    string selectedFileName = TC[Increment];
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();

                    TC1.Source = bitmap;
                    TC1.Margin = new Thickness(Move3[0] = 794, Move3[1] = 145, Move3[2] = 0, Move3[3] = 0);
                    Increment++;
                }
            }
            else
            {
                    TC3.Margin = new Thickness(Move1[0], Move1[1] += 10, Move1[2], Move1[3]);
                    TC2.Margin = new Thickness(Move2[0], Move2[1] += 10, Move2[2], Move2[3]);
                    TC1.Margin = new Thickness(Move3[0], Move3[1] += 10, Move3[2], Move3[3]);
            }
            #endregion
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            TR1.Content = 0;
            MR1.Content = 0;
            BR1.Content = 0;
            TD1.Content = 0;
            BD1.Content = 0;
        }

        private void Julette_Closed(object sender, EventArgs e)
        {
            if(UserName.Text != "Username")
            {
                dbcon.uspUpdateMachineCustomer(machineID, 1);
            }
        }
    }
}
