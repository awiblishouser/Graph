EmailService – Microsoft Graph Integration for ASP.NET Core MVC

The EmailService class provides a secure, reusable integration with the Microsoft Graph API for sending transactional emails in an ASP.NET Core MVC application. It supports single and bulk email delivery, CC/BCC, HTML content, and file attachments, all authenticated using Azure Active Directory and the Azure Identity library.

If you haven’t already, you’ll need to configure your appsettings.json and update Program.cs to register this service.

Features

* Single & Bulk Email Sending – Send emails to one or multiple recipients.
* HTML Email Support – Fully supports HTML-formatted messages.
* CC & BCC Support – Easily include carbon copy and blind carbon copy recipients.
* File Attachments – Attach one or more files with automatic MIME type detection.
* Microsoft Graph Integration – Uses the latest Graph SDK for secure communication.
* Dependency Injection Ready – Designed to be registered and injected into controllers.

Authentication

* Uses Azure.Identity.ClientSecretCredential for secure, app-based authentication.
* Requires the following fields in appsettings.json:

  * TenantId
  * ClientId
  * ClientSecret

Example: Sending a Single Email

// Injected via constructor
private readonly EmailService \_emailService;

await \_emailService.SendEmailAsync(
fromAddress: "[sender@domain.com](mailto:sender@domain.com)",
toAddress: "[recipient@domain.com](mailto:recipient@domain.com)",
subject: "Welcome to Lillie's Toy Box!",
body: "<h1>Thank you for signing up!</h1>",
user: "[ccuser@domain.com](mailto:ccuser@domain.com)"
);

Example: Sending to Multiple Recipients with Attachments

var attachments = new List\<EmailService.AttachmentInput>
{
new EmailService.AttachmentInput(
FileName: "Invoice.pdf",
Content: await System.IO.File.ReadAllBytesAsync("Invoice.pdf"),
ContentType: "application/pdf")
};

await \_emailService.SendMailToManyWithAttachmentsAsync(
fromAddress: "[sender@domain.com](mailto:sender@domain.com)",
toAddresses: new\[] { "[user1@domain.com](mailto:user1@domain.com)", "[user2@domain.com](mailto:user2@domain.com)" },
subject: "Monthly Report",
htmlBody: "<p>Attached is your monthly report.</p>",
ccAddresses: new\[] { "[manager@domain.com](mailto:manager@domain.com)" },
attachments: attachments
);

Requirements

* An Azure AD app registration with the Mail.Send API permission.
* Properly configured appsettings.json for Graph credentials.
* Program.cs must register the EmailService and bind configuration.

Technologies Used

* .NET 8 / ASP.NET Core MVC
* Microsoft Graph SDK
* Azure Identity & OAuth 2.0
* Dependency Injection & Configuration Management
* HTML Email & File Attachments
