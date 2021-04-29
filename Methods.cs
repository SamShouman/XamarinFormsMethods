﻿using System;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Mail;

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

        public static string ReplaceString(string str, string symbol)
        {
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
        
        public static void SetAppProperty(string key, string value)
        {
            /* set the app property of {key} */
            Application.Current.Properties[key] = value;
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
    }
}
