namespace SignalRHubs.Lib
{
    public class Utils
    {
        public static string Base64Encode(string str1, string str2)
        {
            if (string.Compare(str1, str2) == -1)
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str2 + str1);
                return System.Convert.ToBase64String(plainTextBytes);

            }
            else
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str1 + str2);
                return System.Convert.ToBase64String(plainTextBytes);
            }
        }
    }
}
