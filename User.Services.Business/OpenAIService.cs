using User.Data.Access.Helpers;
using User.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace User.Services.Business;
public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _config;

    public OpenAIService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> ExtractInformationFromTextAsync(string text)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            string prompt = $"Extract the following information from the CV " +
                        $"First Name, Last Name, Skills, Country, State, City, Eductation, Experience." +
                        $"For First Name, Last Name, Country, State and City get only one for each." +
                        $"I need it exactly like the following format:" +
                        $"First Name: " + "\n{First Name}\n" +
                        $"Last Name: " + "\n{Last Name}\n" +
                        $"Skills: " + "\n{skill1} \n{skill2} \n{skill3}..." +
                        $"Country: " + "\n{country}\n" +
                        $"State: " + "\n{state}\n" +
                        $"City: " + "\n{city}\n" +
                        $"Education: " + "\n{education1} \n{education2} \n{education3}..." +
                        $"Experience: " + "\n{experience1} \n{experience2} \n{experience3}..." +
                        "Every extracted information should be in a new line." +
                        //"If you can't extract any information, please don't write anything" +
                        //"Don't write anything other like than the extracted information." +
                        //"Don't start new line with a space, tab or any other character." + 
                        "The extreacted Country and State should be in ISO2 format." +
                        $"CV Text:\n{text}";

            var requestBody = new
            {
                model = "gpt-3.5-turbo-0125",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 1500,
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(AppConstants.OPENAI_API_URL, requestContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to extract information from the CV.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            return jsonResponse.choices[0].message.content.ToString();
        }
    }
}
