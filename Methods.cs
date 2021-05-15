using System;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeliveryApplication.Tools
{
    public class Methods
    {
        public async static Task SignOut(Page p)
        {
            /* sign user out */
            try
            {
                p.IsEnabled = false;

                // clear user data from app propeprties
                Application.Current.Properties.Remove(Constants.USERNAME);
                Application.Current.Properties.Remove(Constants.PASSWORD);
                Application.Current.Properties.Remove(Constants.ID);

                await Application.Current.SavePropertiesAsync();

                // set root page as the login page
                Application.Current.MainPage = new NavigationPage(new LoginPage());

            } catch (Exception e) { }
        }

        public static void AfterChecking(ActivityIndicator a, Layout p)
        {
            /* stop activity indicator */
            p.IsEnabled = true;
            a.IsVisible = false;
        }

        public static void BeforeChecking(ActivityIndicator a, Layout p)
        {
            /* run activity indicator before calling apis */
            p.IsEnabled = false;
            a.IsVisible = true;
        }

        public async static void DisplayExceptionAlert(Page p)
        {
            /* display exception alert */
            await p.DisplayAlert( GetString("error"), GetString("errorOccurred"), GetString("ok") );
        }

        public async static Task DisplayAlert(Page p, string title, string message, string button)
        {
            await p.DisplayAlert(Methods.GetString(title), Methods.GetString(message), Methods.GetString(button));
        }

        public static string RandomString(int length, bool numeric = false)
        {
            /* generate a random string */
            Random random = new Random();
            string chars = numeric ? "0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdeafghigklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsEmpty(string x)
        {
            /* check if a string is empty or not */
            return string.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x);
        }

        public static string GetString(string key, string locale = null)
        {
            /* get translated message 
                ATTENTION: THIS METHOD WILL BE USED IF THE APP IS MULTILINGUAL. SO COMMENT THIS METHOD IF THIS IS NOT THE CASE.
                
                HOW IT WORKS: CREATE A CLASS FOR EACH LANGUAGE (MessagesEN and MEssagesAR) 
                              AND PLACE INSIDE IT A DICTIONARY THAT CONTAINS SAME KEYS
             */
            if(!IsEmpty(locale))
            {
                if (locale.Equals("en"))
                    return MessagesEN.MESSAGES[key];

                return MessagesAR.MESSAGES[key];
            }

            locale = GetAppProperty(Constants.LOCALE);
            return locale.Equals("en") || locale.Length == 0 ? MessagesEN.MESSAGES[key] 
                                                             : MessagesAR.MESSAGES[key];
        }

        public static string ReplaceEmail(string str, string symbol)
        {
            /* hide middle chars of email for security */

            string email = str.Split('@')[0], host = str.Split('@')[1];
            int show = str.Length >= 4 ? email.Length / 4 : 1; // quarter of the str length

            string start = email.Substring(0, show), // first {hide} chars
                   middle = email.Substring(show, email.Length- show * 2), // middle chars, will be replaced by {symbol}
                   end = email.Substring(email.Length - show); // last {hide} chars

            string newMiddle = "";

            for(int i=0; i<middle.Length; i++)
            {
                newMiddle += symbol;
            }

            return start + newMiddle + end + "@" + host;
        }

        public static string GetAppProperty(string key)
        {
            /* return the app property of {key} */
            if (Application.Current.Properties.ContainsKey(key))
                return Application.Current.Properties[key].ToString();

            return "";
        }        
        
        public static async Task SetAppProperty(string key, string value)
        {
            /* set the app property of {key} */
            Application.Current.Properties[key] = value;
            await Application.Current.SavePropertiesAsync();
        }

        public static void SetFlowDirection(Page p)
        {
            /* set the flow direction of the page */

            if(GetAppProperty(Constants.LOCALE).Equals("ar"))
                p.FlowDirection = FlowDirection.RightToLeft;
            else
                p.FlowDirection = FlowDirection.LeftToRight;

        }

        public static void SetBackgroundColor(VisualElement element, string color)
        {
            /* sets the background color of the {element} */
            element.BackgroundColor = Color.FromHex(color);
        }

        public static string GetLocale()
        {
            /* set locale of app upon first login else return current locale */
            if (!Application.Current.Properties.ContainsKey(Constants.LOCALE))
            {   
                string locale = GetDeviceLocale();
                Application.Current.Properties[Constants.LOCALE] = locale;
                return locale;
            }

            return Application.Current.Properties[Constants.LOCALE].ToString();
        }

        public static string GetDeviceLocale()
        {
            /* get device locale */
            string locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            // return "en" if locale of device not in supported languages
            if (!Constants.LANGS.Contains(locale))
                return "en";

            return locale;
        }

        public static string GetDate(int days = -7)
        {
            /* 
             * subtract or add {days} from current day 
             * by default get date of last week
             */
            return DateTime.Today.AddDays(days).ToString(Constants.SERVER_DATE_FORMAT);
        }

        public static async Task RefreshPageAsync(Page pageToRefresh, Page p)
        {
            /* 
             * refresh a page 
             * @param pageToRefresh: page to be refreshed => new PageToRefresh();
             * @param p: current page => this
             */
            p.Navigation.InsertPageBefore(pageToRefresh, p);
            await p.Navigation.PopAsync();
        }

        public static bool IsNumeric(string str)
        {
            /* check if string contains only digits */
            return Regex.IsMatch(str, @"^\d+$");
        }

        public static bool SendEmail(string email, string code)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(Constants.SMTP_SERVER); // smtp.gmail.com if sending from gmail

                mail.From = new MailAddress(Constants.EMAIL_FROM);
                mail.To.Add(email);
                mail.Subject = Methods.GetString("resetPass");
                mail.Body = Methods.GetString("emailBody") + code;

                SmtpServer.Port = Constants.PORT; // 587
                SmtpServer.Host = Constants.HOST;
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Constants.EMAIL_FROM, Constants.EMAIL_PASSWORD);

                SmtpServer.SendAsync(mail, null);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static List<T> ObservableCollToList<T>(ObservableCollection<T> oc)
        {
            List<T> lst = new List<T>();

            foreach(var item in oc)
            {
                lst.Add(item);
            }

            return lst;
        }

        public static ObservableCollection<T> ListToObservableColl<T>(List<T> lst)
        {
            ObservableCollection<T> oc = new ObservableCollection<T>();

            foreach (var item in lst)
            {
                oc.Add(item);
            }

            return oc;
        }

        public static void ClearForm(Layout child)
        {
            /*
                ATTENTION: need to check if (this.content as Layout) is != null before calling this method
             */
            foreach (var item in child.Children)
            {
                var layout = item as Layout;
                if (layout != null)
                    ClearForm(layout);
                else
                    ClearEntry(item);
            }

            void ClearEntry(Element entryElement)
            {
                var entry = entryElement as Entry;
                if (entry != null)
                    entry.Text = string.Empty;
            }
        }

        public static int Pagination<T>(List<T> lst, ObservableCollection<T> oc, int pagination, int fromRange)
        {
            /*
             * THIS METHOD WILL BE USED TO SHOW ITEMS IN A COLLECTION VIEW
                @param lst: lst of objects you want to fetch the items from
                @param oc: objects where the items will be added to to show them
                @param pagination: number of items to show in each iteration
                @param fromRange: last index fetched
             */
            if (lst.Count == 0)
                return 0;

            int count = lst.Count - fromRange > pagination ? pagination : lst.Count - fromRange;
            foreach (var order in lst.GetRange(fromRange, count))
            {
                oc.Add(order);
                fromRange++;
            }

            return fromRange;
        }
        
                public static void ListenForInternetConnection(Page p)
        {
            /*
                Dsplays alert if internet connection was lost
             */
            try
            {
                Connectivity.ConnectivityChanged += (s, e) =>
                {
                    if (!(Connectivity.NetworkAccess == NetworkAccess.Internet)) // no internet available
                        DisplayExceptionAlert(p);
                };
            }
            catch (Exception e) {}
        }

        public async static Task OpenEmailApp(string subject = "", string body = "", string emailTo = "")
        {
            /*
                OPEN EMAIL APP FROM THE DEVICE
             */
            await Email.ComposeAsync(subject, body, emailTo);
        }

        public static async Task NothingToShow<T>(Label lbl, List<T> lst)
        {
            /* this method will be used to animate a label when showing a list but its itemssource is emtpy */

            if (lst.Count == 0)
            {
                lbl.Opacity = 0;
                lbl.IsVisible = true;
                await lbl.FadeTo(1, 2500);
            }
            else
                lbl.IsVisible = false;
        }

        public static void CollectionViewItemSpacing(CollectionView collView, int spacing)
        {
            /* add spacing vertically between items of the collection view */
            collView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = spacing
            };
        }
        
        public static List<int> GetGridRowColumn(BindableObject obj)
        {
            /* 
             * it is used mostly when an item inside a grid is clicked
             * @param obj: the object to get its position, maybe frame, button, label. But it should be inside the grid
             */
            int row = Grid.GetRow(obj);
            int column = Grid.GetColumn(obj);

            return new List<int> { row, column };
        }

        public static string ListToString(List<string> lst)
        {
            /* 
             * convert a list of type string to string
             * @param lst: list of strings to be converted
             * @return all items inside {lst} separated by ","
             */
            string result = "";

            foreach (var item in lst)
                if(!IsEmpty(item))
                    result += ("," + item );

            return RemoveFirstCharacter(result);
        }

        public static string RemoveFirstCharacter(string str)
        {
            return str.Length > 1 ? str.Substring(1) : str;
        }

        public static string RemoveLastCharacter(string str)
        {
            return str.Length > 1 ? str.Remove(str.Length - 1, 1) : str;
        }
    }
}
