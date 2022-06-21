using CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Services;

namespace CandidateTesting.GabrielKobayashiBarboza.ConvertLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var convert = new ConvertToNewLog();
            convert.ConvertLog();
        }
    }
}
