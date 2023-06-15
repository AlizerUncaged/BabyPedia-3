using System.Net;
using System.Net.Mail;
using System.Text;
using Mailgun.Messages;
using Mailgun.Service;
using RestSharp;
using RestSharp.Authenticators;

namespace BabyPedia.Data;

public class EmailHandler
{
    public EmailHandler()
    {
    
    }

    private const string apiKey = "8d796925ba2d49ce45d4ca573b4f6c0e-15b35dee-4fc276b8";

    public async Task SendEmail(string toEmail, string subject, string body)
    {
        var domain = "sandbox80f1ca7e7eea4f069fd62bf6876cbd11.mailgun.org";
        RestClient client = new RestClient();
        client.Options.BaseUrl =
            new Uri("https://api.mailgun.net/v3");
        client.Authenticator =
            new HttpBasicAuthenticator("api",
                apiKey);

        RestRequest request = new RestRequest();
        request.AddParameter("domain", domain, ParameterType.UrlSegment);
        request.Resource = $"{domain}/messages";
        request.AddParameter("from", $"babypedia@{domain}");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("text", body);
        request.Method = Method.Post;
        client.ExecuteAsync(request);
    }
}