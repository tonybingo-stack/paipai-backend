using System.Collections;

namespace SignalRHubs.Lib
{
    public class GlobalModule
    {
        public static Hashtable NumberOfActiveUser = new Hashtable();
        public static Hashtable NumberOfUsers = new Hashtable();
        public static Hashtable NumberOfPosts = new Hashtable();
    }
}
