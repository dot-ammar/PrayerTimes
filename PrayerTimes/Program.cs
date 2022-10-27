using Flurl.Http;
using Newtonsoft.Json;
using System.Text;
using Prayertimes_Adhan;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.Media.Streaming.Adaptive;

//variables
int changeDate = 1;
int b = 0;
string prayerToday;
dynamic DynamicData;
string prayerToday2;
dynamic DynamicData2;
string myJsonString = File.ReadAllText("Config.json");
var myJsonObject = JsonConvert.DeserializeObject<prayerConfig>(myJsonString);



//Url peramter builder
List<string> optionalPerameters = new() { myJsonObject.Method, myJsonObject.Shafaq, myJsonObject.Tune, myJsonObject.School, myJsonObject.MidnightMode, myJsonObject.LatitudeAdjustmentMethod, myJsonObject.Adjustment };
List<string> optionalPerametersFilter = new() { myJsonObject.Method, myJsonObject.Shafaq, myJsonObject.Tune, myJsonObject.School, myJsonObject.MidnightMode, myJsonObject.LatitudeAdjustmentMethod, myJsonObject.Adjustment };
List<string> optionalPerametersNames = new() { "method", "shafaq", "tune", "school", "midnightMode", "latitudeAdjustmentMethod", "adjustment" };


string alpha = "http://api.aladhan.com/v1/timings?";
var builder = new StringBuilder();
int count = 0;

foreach (var c in alpha)
{

    builder.Append(c);
    if ((++count % 34) == 0)
    {

        builder.Append($"latitude={myJsonObject.Latitude}");
        builder.Append($"&longitude={myJsonObject.Longitude}");

        foreach (string a in optionalPerametersFilter)
        {

            if (a == null)
            {
                optionalPerameters.Remove(a);
                optionalPerametersNames.Remove(nameof(a));
            }

        }
        
        foreach (string z in optionalPerameters)
        {
            builder.Append($"&{optionalPerametersNames.ElementAt(b)}={z}");
            b++;
        }

    }
}

alpha = builder.ToString();

string alphaTomorrow = alpha.Replace("timings?", $"timings/{((DateTimeOffset)DateTime.Now.AddDays(changeDate)).ToUnixTimeSeconds()}?");

//Debug.WriteLine(alpha);
//Debug.WriteLine(alphaTomorrow);

//dynamic data for today
try
{
    prayerToday = await alpha
                .GetStringAsync();
    DynamicData = JsonConvert.DeserializeObject(prayerToday);
}

catch
{
    //Console.WriteLine("ERROR: Please check perameteres in Config.json.");
    //Console.ReadLine();
    return;
}

//dynamic data for tomorrow
try
{
    prayerToday2 = await alphaTomorrow
                .GetStringAsync();
    DynamicData2 = JsonConvert.DeserializeObject(prayerToday2);
}

catch
{
    //Console.WriteLine("ERROR: Please check perameteres in Config.json.");
    //Console.ReadLine();
    return;
}


/*Console.WriteLine($"Muslim Prayer/Salah Times for {DynamicData.data.date.readable}\n" +
    $"----------------------------------------\n" +
    $"Fajr: {DynamicData.data.timings.Fajr}\n" +
    $"Sunrise: {DynamicData.data.timings.Sunrise}\n" +
    $"Dhuhr: {DynamicData.data.timings.Dhuhr}\n" +
    $"Asr: {DynamicData.data.timings.Asr}\n" +
    $"Maghrib: {DynamicData.data.timings.Maghrib}\n" +
    $"Isha: {DynamicData.data.timings.Isha}\n");
*/

//current time
string currenttime = DateTime.Now.ToString("HH:mm", System.Globalization.DateTimeFormatInfo.InvariantInfo);

//parse the hh:mm to return the time of day, in minutes
int timeGet(string a)
{
    int nowTimeFormat = (int.Parse(a[0].ToString()) * 10 * 60)
        + (int.Parse(a[1].ToString()) * 60)
        + (int.Parse(a[3].ToString()) * 10)
        + int.Parse(a[4].ToString());
    return nowTimeFormat;
}


//calculates the time from current time to next prayer, in minutes
int TimeToPrayer(string a, int b = 0)
{
    int timeGetPrayer = timeGet(a);
    int timeGetCurrent = timeGet(currenttime);

    bool continueq = true;

    if (b > 0)
    {

        timeGetPrayer = 1440 - timeGetCurrent + timeGetPrayer;
        continueq = false;
    }

    else if (timeGetPrayer <= 720 && timeGetCurrent >= 720)
    {
        if (b == 0)
        {
            timeGetPrayer = (1440 - timeGetCurrent) - timeGetPrayer;
        }
    }


    else if (continueq == true)
    {
        timeGetPrayer -= timeGetCurrent;
    }

    return timeGetPrayer;
}

//determine what the next prayer is
List<int> prayerDayMinTime = new() { timeGet(DynamicData.data.timings.Fajr), timeGet(DynamicData.data.timings.Dhuhr), timeGet(DynamicData.data.timings.Asr), timeGet(DynamicData.data.timings.Maghrib), timeGet(DynamicData.data.timings.Isha), timeGet(DynamicData2.data.timings.Fajr) + 1440 };
List<dynamic> prayerStandardTime = new() { DynamicData.data.timings.Fajr, DynamicData.data.timings.Dhuhr, DynamicData.data.timings.Asr, DynamicData.data.timings.Maghrib, DynamicData.data.timings.Isha, DynamicData2.data.timings.Fajr };
List<string> prayerName = new() { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha", "Fajr2" };

int largestSF = timeGet(currenttime);

foreach (int i in prayerDayMinTime)
{
    if (i >= largestSF)
    {
        largestSF = i;
        break;
    }

}

string largestSFname = prayerName[prayerDayMinTime.IndexOf(largestSF)];

//convert time of day in minutes to time of day in "hh' hours and 'mm' minutes'"
int getNextmin = 0;

foreach (string i in prayerName)
{
    if (largestSFname == "Fajr")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Fajr);
    }
    else if (largestSFname == "Dhuhr")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Dhuhr);

    }
    else if (largestSFname == "Asr")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Asr);

    }
    else if (largestSFname == "Maghrib")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Maghrib);

    }
    else if (largestSFname == "Isha")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Isha);

    }
    else if (largestSFname == "Fajr2")
    {
        getNextmin = TimeToPrayer(DynamicData.data.timings.Fajr, changeDate);

    }
}

string nextPrayerTime = Convert.ToString(prayerStandardTime[prayerName.IndexOf(largestSFname)]);

if (largestSFname == "Fajr2")
{
    largestSFname = "Fajr";
}

TimeSpan span = TimeSpan.FromMinutes(getNextmin);
string label = span.ToString("hh' hours and 'mm' minutes'");

ToastNotificationManagerCompat.History.Clear();


new ToastContentBuilder()
    .AddText($"Upcoming Prayer | {largestSFname} | {convertToTwelve(nextPrayerTime)}")
    .AddText($" {label} remaining...")
    .Show();


//Console.WriteLine($"{label} to {largestSFname}.");


//methods

// converts 24 hour time string into 12 hour time string 24:59 -> 12:59 AM (no leading 0s in output)
string convertToTwelve(string twentyFour)
{
    string twelve;
    string amPm;
    int hour = Int32.Parse($"{twentyFour[0]}{twentyFour[1]}");
    int minute = Int32.Parse($"{twentyFour[3]}{twentyFour[4]}");
    if (hour > 12)
    {
        hour -= 12;
        twelve = hour.ToString() + ":" + minute;
        amPm = " pm";
    }
    else
    {
        twelve = hour.ToString() + ":" + minute;
        amPm = " am";
    }


    if (minute.ToString().Length == 1)
    {
        twelve = string.Concat(twelve.AsSpan(0, twelve.IndexOf(":") + 1), "0", twelve.AsSpan(twelve.Length - 1));
    }

    twelve += amPm;
    return twelve;
}