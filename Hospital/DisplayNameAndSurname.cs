namespace Hospital
{
        //Class to keep track of what user is logged in (see How To Display User Name in Asp.net MVC 2019, 2019)
        public class DisplayNameAndSurname
        {
            //Static variable to store the user name (see How To Display User Name in Asp.net MVC 2019, 2019)
            public static string passUserName;

            //Static variable to store the user name (see How To Display User Name in Asp.net MVC 2019, 2019)
            public static string passUserSurname;

            //Method to set the user name
            public static void getUserName(string username)
            {
                //Assigning the provided username to the passUsername variable (see How To Display User Name in Asp.net MVC 2019, 2019)
                passUserName = username;
            }

            //Method to set the user surname
            public static void getUserSurname(string usersurname)
            {
                //Assigning the provided username to the passUserSurname variable (see How To Display User Name in Asp.net MVC 2019, 2019)
                passUserSurname = usersurname;
            }
        }
    }