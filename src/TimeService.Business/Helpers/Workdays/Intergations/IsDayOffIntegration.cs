using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
    public class IsDayOffIntegration : ICalendar
    {
        private const string BaseUrl = "https://isdayoff.ru/api/getdata";
        private const string BadRequest = "100";
        private const string NotFound = "101";
        private const string ServerError = "199";
        private const char NonWorkingDay = '1';
        private const char WorkingDay = '0';
        private const char CovidWorkingDay = '4';

        private readonly HttpClient _httpClient;

        public IsDayOffIntegration()
        {
            _httpClient = new HttpClient();
        }

        private string SendGetRequest(string url, IDictionary<string, string> parameters)
        {
            url = QueryHelpers.AddQueryString(url, parameters);

            return _httpClient.GetStringAsync(url).Result;
        }

        private string GetResponseResult(IDictionary<string, string> parameters)
        {
            string response = SendGetRequest(BaseUrl, parameters);

            if (response == BadRequest)
            {
                throw new InvalidOperationException("Incorrect format of data.");
            }

            if (response == NotFound)
            {
                throw new InvalidOperationException("Data was not found.");
            }

            if (response == ServerError)
            {
                throw new InvalidOperationException("Server error.");
            }

            return response.Replace(CovidWorkingDay, WorkingDay);
        }

        public string GetWorkCalendarByMonth(int month, int year, bool includeCovidNonWorkingDays = false)
        {
            var parameters = new Dictionary<string, string>
            {
                { "year", year.ToString() },
                { "month", month.ToString() },
                { "covid", includeCovidNonWorkingDays ? "0" : "1" }
            };

            return GetResponseResult(parameters);
        }
    }
}
