using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
    public class WorkCalendarHelper
    {
        private const string BaseUrl = "https://isdayoff.ru/api/getdata";
        private readonly HttpClient _httpClient;

        private const string _badRequest = "101";
        private const string _notFound = "101";
        private const char _nonWorkingDay = '1';
        private const char _workingDay = '0';

        public WorkCalendarHelper()
        {
            _httpClient = new HttpClient();
        }

        private string GetResponseContent(HttpResponseMessage responseMessage)
        {
            string stringResponse = responseMessage.Content.ReadAsStringAsync().Result;

            if (stringResponse == _badRequest)
            {
                throw new InvalidOperationException("Incorrect format of data.");
            }

            if (stringResponse == _notFound)
            {
                throw new InvalidOperationException("Data was not found.");
            }

            return new string(stringResponse.Select(c => c == _nonWorkingDay ? _nonWorkingDay : _workingDay).ToArray());
        }

        private HttpResponseMessage SendGetRequest(string url, IDictionary<string, object> parameters)
        {
            var urlParams = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            url += urlParams.Length > 0 ? $"?{urlParams}" : string.Empty;

            return _httpClient.GetAsync(url).Result;
        }

        private string GetResponseResult(IDictionary<string, object> parameters)
        {
            HttpResponseMessage response = SendGetRequest(BaseUrl, parameters);
            response.EnsureSuccessStatusCode();

            return GetResponseContent(response);
        }

        public string GetWorkCalendarByMonth(int month, int year, bool includeCovidNonWorkingDays = false)
        {
            var parameters = new Dictionary<string, object>
            {
                { "year", year },
                { "month", month },
                { "covid", includeCovidNonWorkingDays ? 0 : 1 }
            };

            return GetResponseResult(parameters);
        }
    }
}
