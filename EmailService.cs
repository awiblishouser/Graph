using System.IO;
using Azure.Identity;
using LilliesToyBox.Models;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.SendMail;

namespace LilliesToyBox
{
    public class EmailService
    {
        private readonly GraphServiceClient _graphClient;

        public record AttachmentInput(string FileName, byte[] Content, string? ContentType = null);

        public EmailService(IOptions<GraphSettings> options)
        {
            var cfg = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(cfg.TenantId) ||
                string.IsNullOrWhiteSpace(cfg.ClientId) ||
                string.IsNullOrWhiteSpace(cfg.ClientSecret))
            {
                throw new InvalidOperationException("Graph credentials are missing. Check appsettings.json:Graph.");
            }

            var credential = new ClientSecretCredential(cfg.TenantId, cfg.ClientId, cfg.ClientSecret);

            // In Graph SDK v5, pass the .default scope explicitly.
            _graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
        }

        public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body, string user)
        {
            try
            {
                var requestBody = new SendMailPostRequestBody
                {
                    Message = new Message
                    {
                        Subject = subject,
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Html,
                            Content = body,
                        },
                        ToRecipients = new List<Recipient>
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress { Address = toAddress }
                            }
                        },
                        CcRecipients = new List<Recipient>
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress { Address = user }
                            }
                        },
                        // Ensure BccRecipients is never null to avoid Graph null-collection errors.
                        BccRecipients = new List<Recipient>()
                    },
                    SaveToSentItems = true,
                };

                await _graphClient.Users[fromAddress].SendMail.PostAsync(requestBody);
                Console.WriteLine("Email sent successfully.");
            }
            catch (ODataError e)
            {
                Console.WriteLine($"Error sending email: {e.Error?.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task SendMailToManyWithAttachmentsAsync(
            string fromAddress,
            IEnumerable<string> toAddresses,
            string subject,
            string htmlBody,
            IEnumerable<string>? ccAddresses = null,
            IEnumerable<string>? bccAddresses = null,
            IEnumerable<AttachmentInput>? attachments = null)
        {
            try
            {
                if (toAddresses is null)
                    throw new ArgumentNullException(nameof(toAddresses));

                static List<string> Clean(IEnumerable<string>? src) =>
                    (src ?? Array.Empty<string>())
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => a.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var toList = Clean(toAddresses);
                if (toList.Count == 0)
                    throw new ArgumentException("At least one 'To' address is required.", nameof(toAddresses));

                var ccList = Clean(ccAddresses);
                ccList.RemoveAll(a => toList.Contains(a, StringComparer.OrdinalIgnoreCase));

                var bccList = Clean(bccAddresses);
                bccList.RemoveAll(a => toList.Contains(a, StringComparer.OrdinalIgnoreCase) ||
                                       ccList.Contains(a, StringComparer.OrdinalIgnoreCase));

                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody { ContentType = BodyType.Html, Content = htmlBody },
                    ToRecipients = toList.Select(a => new Recipient { EmailAddress = new EmailAddress { Address = a } }).ToList(),
                    CcRecipients = ccList.Select(a => new Recipient { EmailAddress = new EmailAddress { Address = a } }).ToList(),
                    BccRecipients = bccList.Select(a => new Recipient { EmailAddress = new EmailAddress { Address = a } }).ToList()
                };

                // Optional attachments
                var fileAttachments = (attachments ?? Array.Empty<AttachmentInput>())
                    .Where(a => a is not null && !string.IsNullOrWhiteSpace(a.FileName) && a.Content?.Length > 0)
                    .Select(a => (Attachment)new FileAttachment
                    {
                        OdataType = "#microsoft.graph.fileAttachment",
                        Name = a.FileName,
                        ContentBytes = a.Content,
                        ContentType = string.IsNullOrWhiteSpace(a.ContentType) ? GuessContentType(a.FileName) : a.ContentType
                    })
                    .ToList();

                if (fileAttachments.Count > 0)
                    message.Attachments = fileAttachments;

                var requestBody = new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                };

                await _graphClient.Users[fromAddress].SendMail.PostAsync(requestBody);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError e)
            {
                Console.WriteLine($"Graph error: {e.Error?.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }

            static string GuessContentType(string fileName)
            {
                var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                return ext switch
                {
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".ppt" => "application/vnd.ms-powerpoint",
                    ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    ".txt" => "text/plain",
                    ".csv" => "text/csv",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };
            }
        }
    }
}



